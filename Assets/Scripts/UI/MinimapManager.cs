using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drop one instance of this in the Main scene (not on a player prefab).
/// It creates its own Screen Space Overlay Canvas and renders every frame.
///
/// Setup required in the Inspector:
///   • Set map bounds to match FogOfWarManager (default ±120 XZ).
///
/// Per-unit setup (prefabs):
///   • Add MinimapIcon to each player, minion, tower, inhibitor, and nexus prefab.
///   • Set IconType accordingly (Player / Minion / Structure).
///   • For players: assign MinimapSprite — the icon is drawn as a small portrait image.
/// </summary>
public class MinimapManager : MonoBehaviour
{
    public static MinimapManager Instance { get; private set; }

    // ── Map bounds — must match FogOfWarManager ──────────────────────────────
    [Header("Map Bounds (XZ world space — keep in sync with FogOfWarManager)")]
    [SerializeField] private float mapMinX = -120f;
    [SerializeField] private float mapMaxX =  120f;
    [SerializeField] private float mapMinZ = -120f;
    [SerializeField] private float mapMaxZ =  120f;

    // ── Display ──────────────────────────────────────────────────────────────
    [Header("Display")]
    [SerializeField] private int   textureSize   = 256;
    [SerializeField] private float displaySizePx = 200f;
    [SerializeField] private float borderPx      = 5f;
    [SerializeField] private float marginPx      = 10f;
    [Tooltip("Pixel size of player portrait icons on the minimap.")]
    [SerializeField] private float iconSizePx    = 14f;
    [Tooltip("Scale multiplier for the local player's icon.")]
    [SerializeField] private float localPlayerScale = 1.4f;

    // ── Terrain colors ───────────────────────────────────────────────────────
    [Header("Terrain")]
    [SerializeField] private Color32 foggedColor  = new Color32( 18,  25,  18, 255);
    [SerializeField] private Color32 visibleColor = new Color32( 42,  72,  42, 255);

    // ── Icon colors (dot fallback + portrait tint) ───────────────────────────
    [Header("Icon Colors")]
    [SerializeField] private Color32 localPlayerColor = new Color32(255, 255,   0, 255);
    [SerializeField] private Color32 allyPlayerColor  = new Color32( 80, 160, 255, 255);
    [SerializeField] private Color32 allyMinionColor  = new Color32(140, 200, 255, 255);
    [SerializeField] private Color32 allyStructColor  = new Color32( 60, 120, 220, 255);
    [SerializeField] private Color32 enemyPlayerColor = new Color32(255,  60,  60, 255);
    [SerializeField] private Color32 enemyMinionColor = new Color32(255, 140,  80, 255);
    [SerializeField] private Color32 enemyStructColor = new Color32(200,  50,  50, 255);

    // ── Static icon registry ─────────────────────────────────────────────────
    private static readonly List<MinimapIcon> _icons = new();
    public static void Register  (MinimapIcon icon) { if (!_icons.Contains(icon)) _icons.Add(icon); }
    public static void Unregister(MinimapIcon icon) => _icons.Remove(icon);

    // ── Internal ─────────────────────────────────────────────────────────────
    private Texture2D     _tex;
    private Color32[]     _pixels;
    private RectTransform _minimapRect;
    private GameObject    _canvasGO;

    // One Image child per player icon that has a sprite assigned.
    private readonly Dictionary<MinimapIcon, RectTransform> _playerImageRTs = new();

    // ────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildTexture();
        BuildUI();
        _canvasGO.SetActive(false);
        NetworkGameManager.OnGameStarted += ShowMinimap;
        NetworkGameManager.OnGameOver    += HideMinimap;
    }

    private void OnDestroy()
    {
        NetworkGameManager.OnGameStarted -= ShowMinimap;
        NetworkGameManager.OnGameOver    -= HideMinimap;
        if (Instance == this) Instance = null;
        _icons.Clear();
        if (_tex != null) Destroy(_tex);
    }

    private void ShowMinimap()              => _canvasGO.SetActive(true);
    private void HideMinimap(sbyte _winner) => _canvasGO.SetActive(false);

    // ── UI construction ──────────────────────────────────────────────────────
    private void BuildTexture()
    {
        _tex = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode   = TextureWrapMode.Clamp,
        };
        _pixels = new Color32[textureSize * textureSize];
    }

    private void BuildUI()
    {
        _canvasGO = new GameObject("MinimapCanvas");
        _canvasGO.transform.SetParent(transform, false);
        var canvas = _canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        _canvasGO.AddComponent<CanvasScaler>();
        _canvasGO.AddComponent<GraphicRaycaster>();

        // Black border panel
        float outerSize = displaySizePx + borderPx * 2f;
        var bgGO   = new GameObject("MinimapBorder");
        bgGO.transform.SetParent(_canvasGO.transform, false);
        var bgRect = bgGO.AddComponent<RectTransform>();
        bgRect.anchorMin        = bgRect.anchorMax = Vector2.zero;
        bgRect.pivot            = Vector2.zero;
        bgRect.anchoredPosition = new Vector2(marginPx, marginPx);
        bgRect.sizeDelta        = new Vector2(outerSize, outerSize);
        var bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0f, 0f, 0f, 0.9f);

        // Minimap area — RectMask2D clips player icons that wander to the edge.
        var mapGO   = new GameObject("MinimapArea");
        mapGO.transform.SetParent(bgGO.transform, false);
        _minimapRect = mapGO.AddComponent<RectTransform>();
        _minimapRect.anchorMin        = _minimapRect.anchorMax = new Vector2(0.5f, 0.5f);
        _minimapRect.pivot            = new Vector2(0.5f, 0.5f);
        _minimapRect.anchoredPosition = Vector2.zero;
        _minimapRect.sizeDelta        = new Vector2(displaySizePx, displaySizePx);
        mapGO.AddComponent<RectMask2D>();

        // Fog/terrain texture (behind player icons)
        var texGO   = new GameObject("MinimapTexture");
        texGO.transform.SetParent(mapGO.transform, false);
        var texRect = texGO.AddComponent<RectTransform>();
        texRect.anchorMin = Vector2.zero;
        texRect.anchorMax = Vector2.one;
        texRect.offsetMin = texRect.offsetMax = Vector2.zero;
        var raw = texGO.AddComponent<RawImage>();
        raw.texture = _tex;
    }

    // ── Render loop ──────────────────────────────────────────────────────────
    private void Update()
    {
        var   fogMgr    = FogOfWarManager.Instance;
        sbyte localTeam = fogMgr != null ? fogMgr.LocalTeamId : TeamComponent.Neutral;

        BuildBackground(fogMgr);
        DrawDotIcons(localTeam);
        _tex.SetPixels32(_pixels);
        _tex.Apply(false);

        SyncPlayerImages(localTeam);
    }

    // ── Fog-aware terrain background ─────────────────────────────────────────
    private void BuildBackground(FogOfWarManager fogMgr)
    {
        Color32[] fogPixels = fogMgr?.DisplayPixels;
        int       fogRes    = fogMgr?.TextureResolution ?? textureSize;

        for (int i = 0; i < _pixels.Length; i++)
        {
            if (fogPixels == null) { _pixels[i] = visibleColor; continue; }
            int   x    = i % textureSize;
            int   y    = i / textureSize;
            int   fx   = Mathf.Clamp(x * fogRes / textureSize, 0, fogRes - 1);
            int   fy   = Mathf.Clamp(y * fogRes / textureSize, 0, fogRes - 1);
            float fogV = fogPixels[fy * fogRes + fx].r / 255f;
            _pixels[i] = Lerp32(foggedColor, visibleColor, fogV);
        }
    }

    // ── Dot icons (minions + structures + players without sprites) ────────────
    private void DrawDotIcons(sbyte localTeam)
    {
        foreach (var icon in _icons)
        {
            if (icon == null) continue;
            // Players with a sprite are rendered as UI Images in SyncPlayerImages.
            if (icon.Type == MinimapIcon.IconType.Player && icon.MinimapSprite != null) continue;
            if (!icon.ShouldShow(localTeam)) continue;

            Color32 color  = ChooseColor(icon, localTeam);
            int     radius = ChooseRadius(icon);
            PlaceDot(icon.transform.position, color, radius);
        }
    }

    private Color32 ChooseColor(MinimapIcon icon, sbyte localTeam)
    {
        if (icon.IsLocalPlayer) return localPlayerColor;
        bool ally = icon.TeamId == localTeam;
        return icon.Type switch
        {
            MinimapIcon.IconType.Player    => ally ? allyPlayerColor  : enemyPlayerColor,
            MinimapIcon.IconType.Structure => ally ? allyStructColor  : enemyStructColor,
            _                             => ally ? allyMinionColor  : enemyMinionColor,
        };
    }

    private int ChooseRadius(MinimapIcon icon) => icon.Type switch
    {
        MinimapIcon.IconType.Structure => 5,
        MinimapIcon.IconType.Player    => icon.IsLocalPlayer ? 6 : 4,
        _                             => 2,
    };

    private void PlaceDot(Vector3 worldPos, Color32 color, int radius)
    {
        float mapW = mapMaxX - mapMinX;
        float mapH = mapMaxZ - mapMinZ;
        int   cx   = Mathf.Clamp(Mathf.RoundToInt((worldPos.x - mapMinX) / mapW * (textureSize - 1)), 0, textureSize - 1);
        int   cy   = Mathf.Clamp(Mathf.RoundToInt((worldPos.z - mapMinZ) / mapH * (textureSize - 1)), 0, textureSize - 1);

        for (int dy = -radius; dy <= radius; dy++)
        for (int dx = -radius; dx <= radius; dx++)
        {
            if (dx * dx + dy * dy > radius * radius) continue;
            int px = cx + dx, py = cy + dy;
            if ((uint)px >= (uint)textureSize || (uint)py >= (uint)textureSize) continue;
            _pixels[py * textureSize + px] = color;
        }
    }

    // ── Portrait image icons (players with MinimapSprite) ────────────────────
    private void SyncPlayerImages(sbyte localTeam)
    {
        // Clean up destroyed entries.
        var dead = new List<MinimapIcon>();
        foreach (var kvp in _playerImageRTs)
            if (kvp.Key == null || kvp.Value == null) dead.Add(kvp.Key);
        foreach (var k in dead)
        {
            if (_playerImageRTs.TryGetValue(k, out var rt) && rt != null) Destroy(rt.gameObject);
            _playerImageRTs.Remove(k);
        }

        foreach (var icon in _icons)
        {
            if (icon == null) continue;
            if (icon.Type != MinimapIcon.IconType.Player) continue;
            if (icon.MinimapSprite == null) continue;

            bool show = icon.ShouldShow(localTeam);

            // Create the image GO on first encounter.
            if (!_playerImageRTs.TryGetValue(icon, out RectTransform rt) || rt == null)
            {
                rt = BuildPlayerImage(icon);
                _playerImageRTs[icon] = rt;
            }

            rt.gameObject.SetActive(show);
            if (!show) continue;

            // Position on minimap.
            rt.anchoredPosition = WorldToMinimapLocal(icon.transform.position);

            // Scale: local player is larger.
            float s = icon.IsLocalPlayer ? localPlayerScale : 1f;
            rt.localScale = new Vector3(s, s, 1f);

            // Tint by team / local status.
            Color tint = icon.IsLocalPlayer ? Color.white
                       : (Color)(icon.TeamId == localTeam ? allyPlayerColor : enemyPlayerColor);
            rt.GetComponent<Image>().color = tint;
        }
    }

    private RectTransform BuildPlayerImage(MinimapIcon icon)
    {
        var go = new GameObject($"MM_{icon.name}");
        go.transform.SetParent(_minimapRect, false);

        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta        = new Vector2(iconSizePx, iconSizePx);
        rt.anchorMin        = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;

        var img = go.AddComponent<Image>();
        img.sprite         = icon.MinimapSprite;
        img.preserveAspect = true;

        return rt;
    }

    // Converts a world XZ position to an anchoredPosition local to _minimapRect.
    // _minimapRect is center-pivoted, so (0,0) = map center.
    private Vector2 WorldToMinimapLocal(Vector3 worldPos)
    {
        float u    = Mathf.Clamp01((worldPos.x - mapMinX) / (mapMaxX - mapMinX));
        float v    = Mathf.Clamp01((worldPos.z - mapMinZ) / (mapMaxZ - mapMinZ));
        float half = displaySizePx * 0.5f;
        return new Vector2(Mathf.Lerp(-half, half, u), Mathf.Lerp(-half, half, v));
    }

    // ── Helpers ──────────────────────────────────────────────────────────────
    private static Color32 Lerp32(Color32 a, Color32 b, float t)
    {
        t = Mathf.Clamp01(t);
        return new Color32(
            (byte)(a.r + (b.r - a.r) * t),
            (byte)(a.g + (b.g - a.g) * t),
            (byte)(a.b + (b.b - a.b) * t),
            255);
    }
}
