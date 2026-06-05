using UnityEngine;
using FishNet.Object;

public class AoeCircleAbility : AoeAbility
{
    [Header("AoE Circle")]
    [SerializeField] private float damage = 60f;
    [SerializeField] private DamageType damageType = DamageType.Magical;
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private GameObject vfxPrefab; // optional — circle effect

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

            float scaledDamage = damage * GetLevelScalingMultiplier();
            health.TakeDamage(scaledDamage, damageType, attackerStats);
        }

        RpcSpawnVFX(position);
    }

    [ObserversRpc]
    private void RpcSpawnVFX(Vector3 position)
    {
        if (vfxPrefab == null) return;
        GameObject vfx = Instantiate(vfxPrefab, position, Quaternion.identity);
        Destroy(vfx, 2f);
    }
}