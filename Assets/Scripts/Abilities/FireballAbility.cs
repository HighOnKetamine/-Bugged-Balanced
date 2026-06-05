using UnityEngine;
using FishNet.Object;

public class FireballAbility : SkillshotAbility
{
    [Header("Fireball")]
    [SerializeField] private DamageType damageType = DamageType.Magical;
    [SerializeField] private LayerMask targetMask;

    [ServerRpc]
    protected override void ServerCast(Vector3 origin, Vector3 direction)
    {
        if (!Physics.Raycast(origin, direction, out RaycastHit hit, range, targetMask)) return;
        GameObject hitObject = hit.collider.gameObject;
        if (hitObject == null || hitObject == gameObject) return;
        TeamComponent targetTeam = hitObject.GetComponentInParent<TeamComponent>();
        TeamComponent myTeam = GetComponent<TeamComponent>();
        if (myTeam == null || targetTeam == null || !myTeam.IsEnemy(targetTeam)) return;
        HealthComponent health = hitObject.GetComponentInParent<HealthComponent>();
        if (health == null || health.IsDead) return;
        health.TakeDamage(GetCurrentDamage(), damageType, GetComponent<CharacterStats>());
    }
}