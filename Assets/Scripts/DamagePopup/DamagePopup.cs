using System.Collections;
using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;

    [SerializeField] private float _floatSpeed = 1.5f;
    [SerializeField] private float _fadeDuration = 0.8f;

    public void Setup(float damage, DamageType damageType)
    {
        _text.text = Mathf.RoundToInt(damage).ToString();
        _text.color = damageType switch
        {
            DamageType.Physical => new Color(1f, 0.6f, 0.2f),
            DamageType.Magical => new Color(0.4f, 0.8f, 1f),
            DamageType.True => Color.white,
            _ => Color.white
        };
        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Color startColor = _text.color;

        while (elapsed < _fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _fadeDuration;
            transform.position = startPos + Vector3.up * (_floatSpeed * t);
            _text.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);
            yield return null;
        }

        Destroy(gameObject);
    }
}