using UnityEngine;

public class FloatingDmgSpawner : MonoBehaviour
{
    [SerializeField] private HealthComponent healthComponent;
    [SerializeField] private GameObject floatingDmgPrefab;
    [SerializeField] private Vector3 offset = new Vector3(0, 2f, 0);

    [Header("Colors")]
    [SerializeField] private Color physicalColor = new Color(1f, 0.5f, 0f);   // orange
    [SerializeField] private Color magicalColor = new Color(0.3f, 0.5f, 1f); // blue
    [SerializeField] private Color trueColor = Color.white;
    [SerializeField] private Color healColor = Color.green;

    private void Start()
    {
        if (healthComponent == null)
        {
            Debug.LogError("[FloatingDmgSpawner] No HealthComponent assigned!", this);
            return;
        }
        healthComponent.OnDamageTaken += OnDamageTaken;
        healthComponent.OnHealed += OnHealed;
    }

    private void OnDestroy()
    {
        if (healthComponent != null)
        {
            healthComponent.OnDamageTaken -= OnDamageTaken;
            healthComponent.OnHealed -= OnHealed;
        }
    }

    private void OnDamageTaken(float amount, DamageType damageType)
    {
        Color color = damageType switch
        {
            DamageType.Physical => physicalColor,
            DamageType.Magical => magicalColor,
            DamageType.True => trueColor,
            _ => trueColor
        };
        Spawn(amount, color);
    }

    private void OnHealed(float amount)
    {
        Spawn(amount, healColor);
    }

    private void Spawn(float amount, Color color)
    {
        if (floatingDmgPrefab == null) return;
        Debug.Log($"[FloatingDmgSpawner] Spawning at {transform.position + offset}");
        GameObject go = Instantiate(floatingDmgPrefab, transform.position + offset, Quaternion.identity);
        go.GetComponent<FloatingDmg>()?.SetDamageValue(Mathf.RoundToInt(amount), color);
    }
}