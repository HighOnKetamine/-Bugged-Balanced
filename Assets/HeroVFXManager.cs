using UnityEngine;
using System.Collections;
using TMPro;

public class HeroVFXManager : MonoBehaviour
{
    [Header("Board Game VFX (Q/W)")]
    [SerializeField] private Renderer characterRenderer;
    [SerializeField] private float transitionSpeed = 3f;
    [SerializeField] private Color plasticColor = Color.white;
    [SerializeField] private float sinkDepth = 1.2f;

    [Header("Damage Popup (Space)")]
    [SerializeField] private GameObject damagePopupPrefab;
    [SerializeField] private float spawnHeightOffset = 2.0f;
    [SerializeField] private float jitterAmount = 0.5f;

    [Header("Unbox Revive")]
    [Tooltip("Color of the box/package outline")]
    [SerializeField] private Color boxColor = new Color(0.2f, 0.6f, 1f, 1f);
    [Tooltip("How tall the box is relative to character bounds")]
    [SerializeField] private float boxHeightMultiplier = 1.3f;
    [Tooltip("How wide the box is relative to character bounds")]
    [SerializeField] private float boxWidthMultiplier = 1.2f;
    [Tooltip("Thickness of the box wireframe lines")]
    [SerializeField] private float boxLineWidth = 0.04f;
    [Tooltip("Total duration of the unbox sequence in seconds")]
    [SerializeField] private float unboxDuration = 1.8f;

    private Material _mat;
    private Color _originalColor;
    private Vector3 _originalScale;
    private Quaternion _originalRotation;
    private Vector3 _originalPosition;
    private bool _isDead = false;

    void Awake()
    {
        if (characterRenderer == null) characterRenderer = GetComponentInChildren<Renderer>();

        if (characterRenderer != null)
        {
            _mat = characterRenderer.material;
            if (_mat.HasProperty("_BaseColor"))
                _originalColor = _mat.GetColor("_BaseColor");
            else
                _originalColor = _mat.color;
        }

        _originalScale = transform.localScale;
        _originalRotation = transform.localRotation;
        _originalPosition = transform.localPosition;
    }

    void Update()
    {
        // Q to Die
        if (Input.GetKeyDown(KeyCode.Q) && !_isDead)
        {
            _isDead = true;
            StopAllCoroutines();
            StartCoroutine(DeathRoutine());
        }

        // W to Revive
        if (Input.GetKeyDown(KeyCode.W) && _isDead)
        {
            _isDead = false;
            StopAllCoroutines();
            StartCoroutine(ReviveRoutine());
        }

        // Space to spawn Damage Popup
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnDamagePopup(Random.Range(10, 99));
        }
    }

    public void SpawnDamagePopup(int amount)
    {
        if (damagePopupPrefab == null) return;

        Vector3 spawnPos = transform.position + (Vector3.up * spawnHeightOffset);
        Vector3 jitter = new Vector3(
            Random.Range(-jitterAmount, jitterAmount), 0,
            Random.Range(-jitterAmount, jitterAmount));

        Instantiate(damagePopupPrefab, spawnPos + jitter, Quaternion.identity);
    }

    // ── Death ──────────────────────────────────────────────────────
    private IEnumerator DeathRoutine()
    {
        float t = 0;
        Quaternion deadRot = _originalRotation * Quaternion.Euler(90, 0, 0);
        Vector3 deadPos = _originalPosition + (Vector3.down * sinkDepth);

        while (t < 1.1f)
        {
            t += Time.deltaTime * transitionSpeed;
            transform.localRotation = Quaternion.Lerp(_originalRotation, deadRot, t);
            transform.localPosition = Vector3.Lerp(_originalPosition, deadPos, t);
            SetAlpha(Mathf.Clamp01(1 - t));
            yield return null;
        }
    }

    // ── Revive : Unbox sequence ────────────────────────────────────
    // Timeline (normalized 0→1):
    //   0.00 – 0.10  : make character invisible, reset pose
    //   0.10 – 0.45  : box scales in from flat → full height (cardboard slide-in)
    //   0.45 – 0.65  : box holds, glows
    //   0.65 – 0.85  : box scales back OUT upward (lid removed), char fades in
    //   0.85 – 1.00  : box fully gone, char snaps to original color
    private IEnumerator ReviveRoutine()
    {
        // ── 0. Reset character to invisible at correct pose ─────────
        transform.localRotation = _originalRotation;
        transform.localPosition = _originalPosition;
        transform.localScale = _originalScale;
        SetAlpha(0f);

        // ── 1. Build the box ────────────────────────────────────────
        // Measure the character so the box fits snugly
        Bounds charBounds = GetCharacterBounds();
        float boxH = charBounds.size.y * boxHeightMultiplier;
        float boxW = charBounds.size.x * boxWidthMultiplier;
        float boxD = charBounds.size.z * boxWidthMultiplier;

        // Create a parent at the character's feet
        GameObject boxRoot = new GameObject("UnboxVFX");
        boxRoot.transform.position = transform.position;
        boxRoot.transform.rotation = Quaternion.identity;

        // Build 12 edges of a cuboid using LineRenderers
        LineRenderer[] edges = BuildBoxEdges(boxRoot, boxW, boxH, boxD);

        // Start with box flat (scale Y = 0)
        boxRoot.transform.localScale = new Vector3(1f, 0f, 1f);

        // ── 2. Animate ──────────────────────────────────────────────
        float elapsed = 0f;
        float boxAlpha = 1f;

        while (elapsed < unboxDuration)
        {
            elapsed += Time.deltaTime;
            float n = Mathf.Clamp01(elapsed / unboxDuration); // normalized 0→1

            // --- Phase A: box rises in (0 → 0.45) ---
            if (n < 0.45f)
            {
                float phaseT = n / 0.45f;
                float eased = EaseOutBack(phaseT);
                boxRoot.transform.localScale = new Vector3(1f, eased, 1f);
                SetBoxAlpha(edges, 1f);
            }
            // --- Phase B: box holds + glow pulse (0.45 → 0.65) ---
            else if (n < 0.65f)
            {
                boxRoot.transform.localScale = Vector3.one;
                float pulse = 0.75f + 0.25f * Mathf.Sin((n - 0.45f) / 0.20f * Mathf.PI * 4f);
                SetBoxAlpha(edges, pulse);

                // Char starts ghosting in during glow phase
                float charT = Mathf.InverseLerp(0.55f, 0.65f, n);
                SetAlpha(charT * 0.4f); // partial reveal
            }
            // --- Phase C: box flies off upward, char fully appears (0.65 → 0.85) ---
            else if (n < 0.85f)
            {
                float phaseT = Mathf.InverseLerp(0.65f, 0.85f, n);
                float eased = EaseInCubic(phaseT);

                // Box shoots upward and fades
                boxRoot.transform.localScale = new Vector3(1f - eased * 0.3f, 1f + eased * 1.5f, 1f - eased * 0.3f);
                boxRoot.transform.localPosition = new Vector3(0f, eased * boxH * 0.8f, 0f);
                SetBoxAlpha(edges, 1f - eased);

                // Character fades to full
                float charAlpha = Mathf.Lerp(0.4f, 1f, phaseT);
                SetAlpha(charAlpha);

                // Color transition from plasticColor → originalColor
                Color lerpColor = Color.Lerp(plasticColor, _originalColor, phaseT);
                SetColor(lerpColor);
            }
            // --- Phase D: cleanup (0.85 → 1.0) ---
            else
            {
                SetBoxAlpha(edges, 0f);
                SetAlpha(1f);
                SetColor(_originalColor);
            }

            yield return null;
        }

        // ── 3. Destroy box, ensure char is fully restored ───────────
        Destroy(boxRoot);
        SetAlpha(1f);
        SetColor(_originalColor);
        transform.localPosition = _originalPosition;
        transform.localScale = _originalScale;
    }

    // ── Box construction ───────────────────────────────────────────
    // Builds 12 LineRenderer edges forming a cuboid of size (w, h, d).
    // Origin is at the bottom-center of the box.
    private LineRenderer[] BuildBoxEdges(GameObject parent, float w, float h, float d)
    {
        // 8 corners (bottom 4, top 4)
        Vector3 bl = new Vector3(-w / 2f, 0f, -d / 2f);
        Vector3 br = new Vector3(w / 2f, 0f, -d / 2f);
        Vector3 fr = new Vector3(w / 2f, 0f, d / 2f);
        Vector3 fl = new Vector3(-w / 2f, 0f, d / 2f);

        Vector3 blT = bl + Vector3.up * h;
        Vector3 brT = br + Vector3.up * h;
        Vector3 frT = fr + Vector3.up * h;
        Vector3 flT = fl + Vector3.up * h;

        // 12 edges: 4 bottom, 4 top, 4 vertical
        Vector3[][] edgePairs = new Vector3[][]
        {
            // Bottom ring
            new[] { bl, br }, new[] { br, fr }, new[] { fr, fl }, new[] { fl, bl },
            // Top ring
            new[] { blT, brT }, new[] { brT, frT }, new[] { frT, flT }, new[] { flT, blT },
            // Vertical pillars
            new[] { bl, blT }, new[] { br, brT }, new[] { fr, frT }, new[] { fl, flT },
        };

        LineRenderer[] lrs = new LineRenderer[edgePairs.Length];

        for (int i = 0; i < edgePairs.Length; i++)
        {
            GameObject edgeGO = new GameObject($"Edge_{i}");
            edgeGO.transform.SetParent(parent.transform, false);

            LineRenderer lr = edgeGO.AddComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.positionCount = 2;
            lr.SetPosition(0, edgePairs[i][0]);
            lr.SetPosition(1, edgePairs[i][1]);
            lr.startWidth = boxLineWidth;
            lr.endWidth = boxLineWidth;

            // Use a simple unlit material so it's always visible
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = boxColor;
            lr.endColor = boxColor;

            lrs[i] = lr;
        }

        return lrs;
    }

    private void SetBoxAlpha(LineRenderer[] edges, float alpha)
    {
        Color c = new Color(boxColor.r, boxColor.g, boxColor.b, alpha);
        foreach (var lr in edges)
        {
            if (lr == null) continue;
            lr.startColor = c;
            lr.endColor = c;
        }
    }

    // ── Bounds helper ──────────────────────────────────────────────
    private Bounds GetCharacterBounds()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return new Bounds(transform.position, Vector3.one);

        Bounds b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            b.Encapsulate(renderers[i].bounds);
        return b;
    }

    // ── Easing functions ───────────────────────────────────────────
    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    private float EaseInCubic(float t) => t * t * t;

    // ── Material helpers ───────────────────────────────────────────
    private void SetColor(Color c)
    {
        if (_mat == null) return;
        if (_mat.HasProperty("_BaseColor")) _mat.SetColor("_BaseColor", c);
        else _mat.color = c;
    }

    private void SetAlpha(float alpha)
    {
        if (_mat == null) return;
        Color c = _mat.HasProperty("_BaseColor") ? _mat.GetColor("_BaseColor") : _mat.color;
        c.a = alpha;

        if (alpha < 0.05f && _mat.HasProperty("_Smoothness")) _mat.SetFloat("_Smoothness", 0f);
        else if (_mat.HasProperty("_Smoothness")) _mat.SetFloat("_Smoothness", 0.5f);

        SetColor(c);
    }
}