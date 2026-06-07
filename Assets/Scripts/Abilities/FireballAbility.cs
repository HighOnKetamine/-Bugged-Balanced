using FishNet;
using UnityEngine;
using FishNet.Object;

public class FireballAbility : SkillshotAbility
{
    [Header("Fireball")]
    [SerializeField] private DamageType damageType = DamageType.Magical;
    [SerializeField] private NetworkObject fireballPrefab;

    [ServerRpc]
    protected override void ServerCast(Vector3 origin, Vector3 direction)
    {
        if (fireballPrefab == null)
        {
            Debug.LogWarning("[FireballAbility] No fireball prefab assigned!");
            return;
        }
        NetworkObject proj = Instantiate(fireballPrefab, origin, Quaternion.identity);
        InstanceFinder.ServerManager.Spawn(proj);
        proj.GetComponent<FireballProjectile>()?.Initialize(
            direction, GetScaledDamage(), damageType, GetComponent<CharacterStats>(), gameObject);
    }
}