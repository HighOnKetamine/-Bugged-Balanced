using UnityEngine;
using FishNet.Object;

public class AoeCircleAbility : AoeAbility
{
    [Header("AoE Circle")]
    [SerializeField] private DamageType damageType = DamageType.Magical;
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private GameObject _hitVfxPrefab;

    protected override GameObject hitVfxPrefab => _hitVfxPrefab;

    [ServerRpc]
    protected override void ServerCast(Vector3 position)
    {
        CharacterStats attackerStats = GetComponent<CharacterStats>();
        Collider[] hits = GetTargetsInZone(position, targetMask);
        foreach (Collider col in hits)
        {
            if (col.gameObject == gameObject) continue;
            TeamComponent targetTeam = col.GetComponent<TeamComponent>();
            TeamComponent myTeam = GetComponent<TeamComponent>();
            if (targetTeam == null || myTeam == null) continue;
            if (!myTeam.IsEnemy(targetTeam)) continue;
            HealthComponent health = col.GetComponent<HealthComponent>();
            if (health == null || health.IsDead) continue;
            health.TakeDamage(GetCurrentDamage(), damageType, attackerStats);
        }
        RpcSpawnHitVfx(position);
    }
}