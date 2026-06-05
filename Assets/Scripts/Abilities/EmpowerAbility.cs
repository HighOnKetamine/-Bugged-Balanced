using UnityEngine;

public class EmpowerAbility : AbilityBase
{
    [Header("Empower")]
    [SerializeField] private float duration = 5f;
    [SerializeField] private float attackDamagePercent = 0.25f;
    [SerializeField] private float moveSpeedPercent = 0.15f;

    protected override void CastAbility()
    {
        float scaledDamageBonus = attackDamagePercent * GetLevelScalingMultiplier();
        float scaledMoveSpeedBonus = moveSpeedPercent * GetLevelScalingMultiplier();

        EffectComponent effectComp = GetComponent<EffectComponent>();
        if (effectComp == null)
        {
            Debug.LogWarning("[EmpowerAbility] No EffectComponent found on caster.");
            return;
        }

        effectComp.ApplyEffect(new StatModifierEffect(gameObject, duration, stats => stats.attackDamage, scaledDamageBonus, StatModifierEffect.ModifierType.Percent));
        effectComp.ApplyEffect(new StatModifierEffect(gameObject, duration, stats => stats.moveSpeed, scaledMoveSpeedBonus, StatModifierEffect.ModifierType.Percent));
    }
}
