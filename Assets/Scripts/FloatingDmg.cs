using UnityEngine;
using TMPro;

public class FloatingDmg : MonoBehaviour
{
    private TextMeshProUGUI _textMesh;
    private CanvasGroup _canvasGroup;

    [Header("Timing Settings")]
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float visibleTimer = 0.6f;
    [SerializeField] private float fadeOutSpeed = 3.5f;

    void Awake()
    {
        _textMesh = GetComponentInChildren<TextMeshProUGUI>();
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void SetDamageValue(int amount)
    {
        if (_textMesh != null)
        {
            _textMesh.text = amount.ToString();
        }
    }

    void Update()
    {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        visibleTimer -= Time.deltaTime;

        if (visibleTimer <= 0)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha -= fadeOutSpeed * Time.deltaTime;

                if (_canvasGroup.alpha <= 0)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}