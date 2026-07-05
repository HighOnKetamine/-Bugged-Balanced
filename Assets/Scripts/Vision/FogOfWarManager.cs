using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Drop one instance of this prefab/component into the Main scene.
/// It builds the fog-of-war plane procedurally and keeps the vision texture up to date.
///
/// Per-unit setup:
///   • Add VisionSource  to players, minions, towers.
///   • Add VisibilityTarget to players and minions (NOT towers — towers are always visible).
/// </summary>
public class FogOfWarManager : MonoBehaviour
{
    public static FogOfWarManager Instance { get; private set; }

    // ── Map bounds (world XZ) ────────────────────────────────────────────────
    [Header("Map Bounds (XZ world space)")]
    [SerializeField] private float mapMinX = -120f;
    [SerializeField] private float mapMaxX =  120f;
    [SerializeField] private float mapMinZ = -120f;
    [SerializeField] private float mapMaxZ =  120f;

    // ── Fog plane ────────────────────────────────────────────────────────────
    [Header("Fog Plane")]
    [Tooltip("Y height of the fog plane. Must be BELOW the camera and ABOVE tall geometry.")]
    [SerializeField] private float fogPlaneY = 15f;
    [SerializeField] private Color fogColor  = new Color(0f, 0f, 0.05f, 0.88f);
    [SerializeField] private Shader fogShader;  // assign FogOfWar.shader; found by name if null

    // ── Vision texture ───────────────────────────────────────────────────────
    [Header("Vision Texture")]
    [Tooltip("Resolution of the internal vision texture. Higher = sharper edges but more cost.")]
    [SerializeField] private int textureResolution = 256;
    [Tooltip("Pixel-width of the soft falloff at the edge of each vision circle.")]
    [SerializeField] private int edgeSoftnessPx    = 10;
    [Tooltip("Vision texture updates per second. 10 is plenty for gameplay.")]
    [SerializeField] private float updatesPerSecond = 10f;

    // ── Static registration lists (survive manager lifetime) ─────────────────
    private static readonly List<VisionSource>     _sources = new();
    private static readonly List<VisibilityTarget> _targets = new();

    public static void RegisterSource  (VisionSource     s) { if (!_sources.Contains(s)) _sources.Add(s); }
    public static void UnregisterSource(VisionSource     s) => _sources.Remove(s);
    public static void RegisterTarget  (VisibilityTarget t) { if (!_targets.Contains(t)) _targets.Add(t); }
    public static void UnregisterTarget(VisibilityTarget t) => _targets.Remove(t);

    [Header("Fog Animation")]
    [Tooltip("How fast fog clears as an area enters vision (brightness units/s, 0-255 scale).")]
    [SerializeField] private float revealSpeed = 2000f;
    [Tooltip("How fast fog returns as an area leaves vision.")]
    [SerializeField] private float hideSpeed   = 400f;

    // ── Internal state ───────────────────────────────────────────────────────
    private Texture2D _visionTex;
    private Color32[] _targetPixels;   // rebuilt at 10 Hz — gameplay truth
    private Color32[] _displayPixels;  // lerped toward target every frame — what the shader sees
    private Material  _fogMaterial;
    private float     _nextUpdate;
    private sbyte     _localTeamId = TeamComponent.Neutral;

    // ────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        InitVisionTexture();
        InitFogPlane();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        _sources.Clear();
        _targets.Clear();
        if (_visionTex  != null) Destroy(_visionTex);
        if (_fogMaterial != null) Destroy(_fogMaterial);
    }

    // ── Vision texture ───────────────────────────────────────────────────────
    private void InitVisionTexture()
    {
        _visionTex = new Texture2D(textureResolution, textureResolution, TextureFormat.R8, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode   = TextureWrapMode.Clamp,
        };
        int n = textureResolution * textureResolution;
        _targetPixels  = new Color32[n];
        _displayPixels = new Color32[n];
        for (int i = 0; i < n; i++)
        {
            _targetPixels[i]  = new Color32(0, 0, 0, 255);
            _displayPixels[i] = new Color32(0, 0, 0, 255);
        }
        _visionTex.SetPixels32(_displayPixels);
        _visionTex.Apply(false);
    }

    // ── Fog plane ────────────────────────────────────────────────────────────
    private void InitFogPlane()
    {
        if (fogShader == null)
            fogShader = Shader.Find("Custom/FogOfWar");
        if (fogShader == null)
        {
            Debug.LogError("[FogOfWarManager] FogOfWar shader not found. Assign it in the Inspector.");
            return;
        }

        _fogMaterial = new Material(fogShader);
        _fogMaterial.SetColor("_FogColor", fogColor);
        _fogMaterial.SetTexture("_VisionTex", _visionTex);

        var fogGO = new GameObject("FogPlane");
        fogGO.transform.SetParent(transform, worldPositionStays: false);

        var mf = fogGO.AddComponent<MeshFilter>();
        var mr = fogGO.AddComponent<MeshRenderer>();
        mr.material           = _fogMaterial;
        mr.shadowCastingMode  = ShadowCastingMode.Off;
        mr.receiveShadows     = false;
        mf.mesh               = BuildFogMesh();
    }

    private Mesh BuildFogMesh()
    {
        float mapW = mapMaxX - mapMinX;
        float mapH = mapMaxZ - mapMinZ;
        // 20-unit buffer so the fog covers the camera's view beyond the map edge.
        float buf  = 20f;

        float x0 = mapMinX - buf;
        float x1 = mapMaxX + buf;
        float z0 = mapMinZ - buf;
        float z1 = mapMaxZ + buf;

        // UV 0 = mapMin, UV 1 = mapMax.  Buffer area has UV <0 or >1 → clamps to edge (fogged).
        float uMin = -buf / mapW;
        float uMax =  1f + buf / mapW;
        float vMin = -buf / mapH;
        float vMax =  1f + buf / mapH;

        var mesh = new Mesh { name = "FogPlane" };
        mesh.vertices  = new Vector3[]
        {
            new(x0, fogPlaneY, z0),
            new(x1, fogPlaneY, z0),
            new(x0, fogPlaneY, z1),
            new(x1, fogPlaneY, z1),
        };
        mesh.uv = new Vector2[]
        {
            new(uMin, vMin),
            new(uMax, vMin),
            new(uMin, vMax),
            new(uMax, vMax),
        };
        // Winding order: visible from above (camera looks down).
        mesh.triangles = new int[] { 0, 2, 1,  1, 2, 3 };
        mesh.RecalculateBounds();
        return mesh;
    }

    // ── Update loop ──────────────────────────────────────────────────────────
    private void Update()
    {
        if (_localTeamId == TeamComponent.Neutral)
            TryFindLocalTeam();

        if (Time.time >= _nextUpdate)
        {
            _nextUpdate = Time.time + 1f / updatesPerSecond;
            RebuildVisionTexture();
            UpdateTargetVisibility();
        }

        LerpDisplayToTarget();
    }

    private void LerpDisplayToTarget()
    {
        float reveal = revealSpeed * Time.deltaTime;
        float hide   = hideSpeed   * Time.deltaTime;
        bool dirty = false;

        for (int i = 0; i < _displayPixels.Length; i++)
        {
            float t = _targetPixels[i].r;
            float d = _displayPixels[i].r;
            if (d == t) continue;

            dirty = true;
            float next = d < t ? Mathf.Min(d + reveal, t)
                                : Mathf.Max(d - hide,   t);
            _displayPixels[i] = new Color32((byte)next, 0, 0, 255);
        }

        if (!dirty) return;
        _visionTex.SetPixels32(_displayPixels);
        _visionTex.Apply(false);
    }

    // Inspect already-registered VisionSources for the locally-owned one.
    private void TryFindLocalTeam()
    {
        foreach (var src in _sources)
        {
            if (src == null) continue;
            var nob = src.GetComponent<NetworkObject>();
            if (nob == null || !nob.IsOwner) continue;
            sbyte id = src.GetTeamId();
            if (id == TeamComponent.Neutral) continue;
            _localTeamId = id;
            Debug.Log($"[FogOfWarManager] Local player team: {_localTeamId}");
            return;
        }
    }

    // ── Vision texture rebuild ───────────────────────────────────────────────
    private void RebuildVisionTexture()
    {
        // Clear to fully fogged.
        for (int i = 0; i < _targetPixels.Length; i++) _targetPixels[i] = new Color32(0, 0, 0, 255);

        // Paint a circle for each friendly vision source.
        foreach (var src in _sources)
        {
            if (src == null) continue;
            if (_localTeamId != TeamComponent.Neutral && src.GetTeamId() != _localTeamId) continue;
            PaintCircle(src.transform.position, src.VisionRadius);
        }

    }

    private void PaintCircle(Vector3 worldPos, float worldRadius)
    {
        float mapW = mapMaxX - mapMinX;
        float mapH = mapMaxZ - mapMinZ;
        int   res  = textureResolution;

        float cx = (worldPos.x - mapMinX) / mapW * res;
        float cy = (worldPos.z - mapMinZ) / mapH * res;   // Z → texture Y
        float r  = worldRadius / mapW * res;
        float rs = r + edgeSoftnessPx;

        int x0 = Mathf.Max(0,       Mathf.FloorToInt(cx - rs));
        int x1 = Mathf.Min(res - 1, Mathf.CeilToInt (cx + rs));
        int y0 = Mathf.Max(0,       Mathf.FloorToInt(cy - rs));
        int y1 = Mathf.Min(res - 1, Mathf.CeilToInt (cy + rs));

        for (int y = y0; y <= y1; y++)
        {
            for (int x = x0; x <= x1; x++)
            {
                float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                if (dist >= rs) continue;

                byte brightness = dist <= r
                    ? (byte)255
                    : (byte)(255 * (1f - (dist - r) / edgeSoftnessPx));

                int idx = y * res + x;
                if (_targetPixels[idx].r < brightness)
                    _targetPixels[idx] = new Color32(brightness, brightness, brightness, 255);
            }
        }
    }

    // ── Minimap accessors ────────────────────────────────────────────────────
    public sbyte     LocalTeamId       => _localTeamId;
    public Color32[] TargetPixels      => _targetPixels;
    public Color32[] DisplayPixels     => _displayPixels;
    public int       TextureResolution => textureResolution;

    // ── Enemy visibility ─────────────────────────────────────────────────────

    /// <summary>
    /// Returns true if worldPos is currently inside the local team's vision.
    /// Works for any unit type — does not require a VisibilityTarget component.
    /// </summary>
    public bool IsPositionVisible(Vector3 worldPos)
    {
        if (_localTeamId == TeamComponent.Neutral) return true;
        float u = (worldPos.x - mapMinX) / (mapMaxX - mapMinX);
        float v = (worldPos.z - mapMinZ) / (mapMaxZ - mapMinZ);
        int   px = Mathf.Clamp(Mathf.RoundToInt(u * (textureResolution - 1)), 0, textureResolution - 1);
        int   py = Mathf.Clamp(Mathf.RoundToInt(v * (textureResolution - 1)), 0, textureResolution - 1);
        return _targetPixels[py * textureResolution + px].r > 25;
    }

    /// <summary>
    /// Immediately re-evaluate a single target.
    /// Called by VisibilityTarget.OnTeamChanged so the renderer correction
    /// happens the frame the team SyncVar arrives, not on the next FoW tick.
    /// </summary>
    public void EvaluateTarget(VisibilityTarget tgt)
    {
        if (tgt == null) return;
        sbyte teamId = tgt.GetTeamId();
        if (_localTeamId == TeamComponent.Neutral || teamId == _localTeamId)
        {
            tgt.SetVisible(true);
            return;
        }
        float mapW = mapMaxX - mapMinX;
        float mapH = mapMaxZ - mapMinZ;
        int   res  = textureResolution;
        float u = (tgt.transform.position.x - mapMinX) / mapW;
        float v = (tgt.transform.position.z - mapMinZ) / mapH;
        int   px = Mathf.Clamp(Mathf.RoundToInt(u * (res - 1)), 0, res - 1);
        int   py = Mathf.Clamp(Mathf.RoundToInt(v * (res - 1)), 0, res - 1);
        tgt.SetVisible(_targetPixels[py * res + px].r > 25);
    }

    private void UpdateTargetVisibility()
    {
        float mapW = mapMaxX - mapMinX;
        float mapH = mapMaxZ - mapMinZ;
        int   res  = textureResolution;

        foreach (var tgt in _targets)
        {
            if (tgt == null) continue;

            sbyte teamId = tgt.GetTeamId();

            // Allies (and own character) are always visible.
            if (_localTeamId == TeamComponent.Neutral || teamId == _localTeamId)
            {
                tgt.SetVisible(true);
                continue;
            }

            // Sample the pixel at this target's world XZ position.
            float u = (tgt.transform.position.x - mapMinX) / mapW;
            float v = (tgt.transform.position.z - mapMinZ) / mapH;
            int   px = Mathf.Clamp(Mathf.RoundToInt(u * (res - 1)), 0, res - 1);
            int   py = Mathf.Clamp(Mathf.RoundToInt(v * (res - 1)), 0, res - 1);

            // Threshold: >25 avoids hiding targets exactly at the edge circle.
            tgt.SetVisible(_targetPixels[py * res + px].r > 25);
        }
    }
}
