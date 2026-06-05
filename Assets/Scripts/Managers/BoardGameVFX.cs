using UnityEngine;
using System.Collections;

public class BoardGameVFX : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Renderer characterRenderer;

    [Header("Settings")]
    [SerializeField] private float transitionSpeed = 3f;
    [SerializeField] private Color plasticColor = Color.white;
    [SerializeField] private float sinkDepth = 0.5f; // How far underground it goes

    private Material _mat;
    private Color _originalColor;
    private Vector3 _originalScale;
    private Quaternion _originalRotation;
    private Vector3 _originalPosition;

    void Awake()
    {
        if (characterRenderer == null) characterRenderer = GetComponent<Renderer>();
        if (characterRenderer == null) return;

        _mat = characterRenderer.material;

        if (_mat.HasProperty("_BaseColor"))
            _originalColor = _mat.GetColor("_BaseColor");
        else
            _originalColor = _mat.color;

        _originalColor.a = 1f;

        _originalScale = transform.localScale;
        _originalRotation = transform.localRotation;
        _originalPosition = transform.localPosition;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K)) PlayDeath();
        if (Input.GetKeyDown(KeyCode.E)) PlayRevive();
    }

    public void PlayDeath()
    {
        StopAllCoroutines();
        StartCoroutine(DeathRoutine());
    }

    public void PlayRevive()
    {
        StopAllCoroutines();
        StartCoroutine(ReviveRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        float t = 0;
        Quaternion deadRot = _originalRotation * Quaternion.Euler(90, 0, 0);
        Vector3 deadPos = _originalPosition + (Vector3.down * sinkDepth);

        while (t < 1.1f)
        {
            t += Time.deltaTime * transitionSpeed;

            // Tip and Sink
            transform.localRotation = Quaternion.Lerp(_originalRotation, deadRot, t);
            transform.localPosition = Vector3.Lerp(_originalPosition, deadPos, t);

            SetAlpha(Mathf.Clamp01(1 - t));
            yield return null;
        }
    }

    private IEnumerator ReviveRoutine()
    {
        float t = 0;
        transform.localRotation = _originalRotation;
        Vector3 startPos = _originalPosition + (Vector3.down * sinkDepth);

        SetColor(plasticColor);

        while (t < 1.1f)
        {
            t += Time.deltaTime * transitionSpeed;

            // Scale and Rise
            transform.localScale = Vector3.Lerp(Vector3.zero, _originalScale, t);
            transform.localPosition = Vector3.Lerp(startPos, _originalPosition, t);

            Color lerpColor = Color.Lerp(plasticColor, _originalColor, t);
            SetColor(lerpColor);

            yield return null;
        }
    }

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

        // Kill smoothness to remove the "ghost" highlights
        if (alpha <= 0.05f)
        {
            if (_mat.HasProperty("_Smoothness")) _mat.SetFloat("_Smoothness", 0f);
        }
        else
        {
            if (_mat.HasProperty("_Smoothness")) _mat.SetFloat("_Smoothness", 0.5f);
        }

        SetColor(c);
    }
}