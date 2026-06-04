using UnityEngine;
using TMPro;

public class FloatingDmg : MonoBehaviour
{
    private TextMeshProUGUI _textMesh;
    private CanvasGroup _canvasGroup;

    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float visibleTimer = 0.6f;
    [SerializeField] private float fadeOutSpeed = 3.5f;

    private float _timer;

    private void Awake()
    {
        _textMesh = GetComponentInChildren<TextMeshProUGUI>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _timer = visibleTimer;
    }

    public void SetDamageValue(int amount, Color color)
    {
        if (_textMesh != null)
        {
            _textMesh.text = amount.ToString();
            _textMesh.color = color;
        }
    }

    private void Update()
    {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;
        _timer -= Time.deltaTime;

        if (_timer <= 0 && _canvasGroup != null)
        {
            _canvasGroup.alpha -= fadeOutSpeed * Time.deltaTime;
            if (_canvasGroup.alpha <= 0)
                Destroy(gameObject);
        }
    }
}