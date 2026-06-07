using FishNet.Object;
using UnityEngine;

public class EmpowerAbility : AbilityBase
{
    [Header("Empower")]
    [SerializeField] private float duration = 5f;
    [SerializeField] private float attackDamagePercent = 0.25f;
    [SerializeField] private float moveSpeedPercent = 0.15f;

    protected override void CastAbility()
    {
        ServerCast();
    }

    [ServerRpc]
    private void ServerCast()
    {
        EffectComponent effectComp = GetComponent<EffectComponent>();
        if (effectComp == null)
        {
            Debug.LogWarning("[EmpowerAbility] No EffectComponent found.");
            return;
        }
        effectComp.ApplyEffect(new StatModifierEffect(
            gameObject, duration,
            stats => stats.attackDamage, attackDamagePercent,
            StatModifierEffect.ModifierType.Percent));
        effectComp.ApplyEffect(new StatModifierEffect(
            gameObject, duration,
            stats => stats.moveSpeed, moveSpeedPercent,
            StatModifierEffect.ModifierType.Percent));
    }
}