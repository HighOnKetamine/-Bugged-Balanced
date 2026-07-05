using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private HealthComponent healthComponent;
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("World Space Settings")]
    [SerializeField] private bool useFixedRotation = true;
    [SerializeField] private Vector3 fixedRotation = new Vector3(45, 0, 0);

    private Canvas _canvas;

    private void Awake()
    {
        _canvas = GetComponentInChildren<Canvas>(true);
        if (_canvas != null)
            _canvas.renderMode = RenderMode.WorldSpace;
    }

    private void Start()
    {
        if (healthComponent == null)
        {
            Debug.LogError("[HealthBarUI] No HealthComponent assigned!", this);
            return;
        }

        healthComponent.OnHealthChanged += Refresh;

        Refresh(healthComponent.Current, healthComponent.Max);
    }

    private void LateUpdate()
    {
        if (_canvas == null) return;

        if (useFixedRotation)
        {
            _canvas.transform.rotation = Quaternion.Euler(fixedRotation);
        }
    }

    private void Refresh(float current, float max)
    {
        if (max <= 0) return;

        if (fillImage != null)
            fillImage.fillAmount = current / max;

        if (healthText != null)
            healthText.text = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
    }

    private void OnDestroy()
    {
        if (healthComponent != null)
            healthComponent.OnHealthChanged -= Refresh;
    }
}