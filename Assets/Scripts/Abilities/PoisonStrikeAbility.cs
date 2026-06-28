using UnityEngine;
using FishNet.Object;

public class PoisonStrikeAbility : TargetedAbility
{
    [Header("Poison Strike")]
    [SerializeField] private DamageType damageType = DamageType.Magical;
    [SerializeField] private uint maxStacks = 3;
    [SerializeField] private float tickInterval = 1f;
    [SerializeField] private float duration = 5f;
    [SerializeField] private float spreadRadius = 5f;
    [SerializeField] private LayerMask spreadMask;
    [SerializeField] private GameObject _hitVfxPrefab;

    protected override GameObject hitVfxPrefab => _hitVfxPrefab;

    [ServerRpc]
    protected override void ServerCast(GameObject target)
    {
        ApplyEffect(target);
    }

    [Server]
    private void ApplyEffect(GameObject target)
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
        CharacterStats characterStats = GetComponent<CharacterStats>();

        effectComp.ApplyEffect(new DoTEffect(
        target, duration, GetScaledDamage(), damageType,
        maxStacks, tickInterval, StackBehavior.RefreshDuration, characterStats));
        RpcSpawnHitVfx(target.transform.position);

        SubscribeToDeathForSpread(target, target.transform.position, myTeam);
    }

    [Server]
    private void SubscribeToDeathForSpread(GameObject target, Vector3 position, TeamComponent myTeam)
    {
        HealthComponent targetHealth = target.GetComponent<HealthComponent>();
        if (targetHealth == null) return;

        Debug.Log($"[PoisonStrike] Subscribing to death of {target.name}");

        void OnDied(GameObject killer)
        {
            Debug.Log($"[PoisonStrike] Chain fired from {target.name}");
            targetHealth.OnDeath -= OnDied;
            SpreadPoison(position, myTeam);
        }

        targetHealth.OnDeath += OnDied;
    }

    [Server]
    private void SpreadPoison(Vector3 position, TeamComponent myTeam)
    {
        Collider[] hits = Physics.OverlapSphere(position, spreadRadius, spreadMask);
        GameObject nearest = null;
        float nearestDist = Mathf.Infinity;

        foreach (Collider col in hits)
        {
            TeamComponent targetTeam = col.GetComponentInParent<TeamComponent>();
            if (targetTeam == null || !myTeam.IsEnemy(targetTeam)) continue;
            HealthComponent health = col.GetComponentInParent<HealthComponent>();
            if (health == null || health.IsDead) continue;
            float dist = Vector3.Distance(position, col.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = col.gameObject;
            }
        }

        if (nearest == null)
        {
            Debug.Log("[PoisonStrike] No spread target found.");
            return;
        }

        Debug.Log($"[PoisonStrike] Spreading to {nearest.name}");

        ApplyEffect(nearest);
    }
}