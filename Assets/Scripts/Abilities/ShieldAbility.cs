using UnityEngine;
using FishNet.Object;

public class ShieldAbility : AbilityBase
{
    [Header("Shield")]
    [SerializeField] private float healAmount = 40f;
    [SerializeField] private float armorBonus = 15f;
    [SerializeField] private float magicResistBonus = 15f;
    [SerializeField] private float duration = 5f;

    protected override void CastAbility()
    {
        float scaledHeal = healAmount * GetLevelScalingMultiplier();
        float scaledArmor = armorBonus * GetLevelScalingMultiplier();
        float scaledMagicResist = magicResistBonus * GetLevelScalingMultiplier();

        HealthComponent health = GetComponent<HealthComponent>();
        if (health != null)
            health.Heal(scaledHeal);

        EffectComponent effectComp = GetComponent<EffectComponent>();
        if (effectComp == null)
        {
            Debug.LogWarning("[ShieldAbility] No EffectComponent found on caster.");
            return;
        }

        effectComp.ApplyEffect(new StatModifierEffect(gameObject, duration, stats => stats.armor, scaledArmor, StatModifierEffect.ModifierType.Flat));
        effectComp.ApplyEffect(new StatModifierEffect(gameObject, duration, stats => stats.magicResist, scaledMagicResist, StatModifierEffect.ModifierType.Flat));
    }
}
