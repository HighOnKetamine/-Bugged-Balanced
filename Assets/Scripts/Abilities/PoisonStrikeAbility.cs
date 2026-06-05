using UnityEngine;
using FishNet.Object;

public class PoisonStrikeAbility : TargetedAbility
{
    [Header("Poison Strike")]
    [SerializeField] private DamageType damageType = DamageType.Magical;
    [SerializeField] private uint maxStacks = 3;
    [SerializeField] private float tickInterval = 1f;
    [SerializeField] private float duration = 5f;

    [ServerRpc]
    protected override void ServerCast(GameObject target)
    {
        if (target == null) return;
        TeamComponent myTeam = GetComponent<TeamComponent>();
        TeamComponent targetTeam = target.GetComponentInParent<TeamComponent>();
        if (myTeam == null || targetTeam == null || !myTeam.IsEnemy(targetTeam)) return;
        EffectComponent effectComp = target.GetComponent<EffectComponent>();
        if (effectComp == null)
        {
            Debug.LogWarning("[PoisonStrikeAbility] Target has no EffectComponent.");
            return;
        }
        effectComp.ApplyEffect(new DoTEffect(
            target, duration, GetCurrentDamage(), damageType,
            maxStacks, tickInterval, StackBehavior.RefreshDuration));
    }
}