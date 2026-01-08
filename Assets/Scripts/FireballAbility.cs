using FishNet.Object;
using UnityEngine;

public class FireballAbility : AbilityBase
{
    [Header("Fireball Settings")]
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private float damage = 30f;
    [SerializeField] private float speed = 10f;
    [SerializeField] private float range = 15f;
    [SerializeField] private Transform shootPoint;

    protected override void CastAbility()
    {
        Vector3 shootDirection = transform.forward;

        // Raycast to get target position
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, range))
        {
            shootDirection = (hit.point - shootPoint.position).normalized;
        }

        ServerSpawnFireball(shootPoint.position, shootDirection);
    }

    [ServerRpc]
    private void ServerSpawnFireball(Vector3 position, Vector3 direction)
    {
        if (fireballPrefab == null) return;

        GameObject fireball = Instantiate(fireballPrefab, position, Quaternion.LookRotation(direction));
        Spawn(fireball);

        Rigidbody rb = fireball.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }

        FireballProjectile projectile = fireball.GetComponent<FireballProjectile>();
        if (projectile != null)
        {
            projectile.Initialize(damage, this.gameObject);
        }

        // Destroy after range time
        Destroy(fireball, range / speed);
    }
}
