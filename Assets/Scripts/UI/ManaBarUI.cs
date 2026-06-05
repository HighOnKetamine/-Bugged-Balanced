using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ManaBarUI : MonoBehaviour
{
    [SerializeField] private ManaComponent manaComponent;
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI manaText;

    [Header("World Space Settings")]
    [SerializeField] private bool useFixedRotation = true;
    [SerializeField] private Vector3 fixedRotation = new Vector3(45, 0, 0);

    private Canvas _canvas;

    private void Awake()
    {
        _canvas = GetComponentInChildren<Canvas>();
        if (_canvas != null)
            _canvas.renderMode = RenderMode.WorldSpace;
    }

    private void Start()
    {
        if (manaComponent == null)
        {
            Debug.LogError("[ManaBarUI] No ManaComponent assigned!", this);
            return;
        }

        manaComponent.OnManaChanged += Refresh;
        Refresh(manaComponent.Current, manaComponent.Max);
    }

    private void LateUpdate()
    {
        if (_canvas == null) return;

        if (useFixedRotation)
            _canvas.transform.rotation = Quaternion.Euler(fixedRotation);
    }

    private void Refresh(float current, float max)
    {
        if (max <= 0) return;

        if (fillImage != null)
            fillImage.fillAmount = current / max;

        if (manaText != null)
            manaText.text = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
    }

    private void OnDestroy()
    {
        if (manaComponent != null)
            manaComponent.OnManaChanged -= Refresh;
    }
}