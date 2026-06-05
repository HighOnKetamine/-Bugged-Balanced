using UnityEngine;

public class ShieldAbility : AbilityBase
{
    [Header("Shield")]
    [SerializeField] private float armorBonus = 15f;
    [SerializeField] private float magicResistBonus = 15f;
    [SerializeField] private float duration = 5f;

    protected override void CastAbility()
    {
        // GetCurrentDamage() used as heal amount here
        HealthComponent health = GetComponent<HealthComponent>();
        if (health != null)
            health.Heal(GetCurrentDamage());

        EffectComponent effectComp = GetComponent<EffectComponent>();
        if (effectComp == null)
        {
            Debug.LogWarning("[ShieldAbility] No EffectComponent found.");
            return;
        }
        effectComp.ApplyEffect(new StatModifierEffect(
            gameObject, duration,
            stats => stats.armor, armorBonus,
            StatModifierEffect.ModifierType.Flat));
        effectComp.ApplyEffect(new StatModifierEffect(
            gameObject, duration,
            stats => stats.magicResist, magicResistBonus,
            StatModifierEffect.ModifierType.Flat));
    }
}