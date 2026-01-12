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
        Camera cam = GetComponentInChildren<Camera>();
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            // Rotate player towards target (ignore Y difference)
            Vector3 lookTarget = hit.point;
            lookTarget.y = transform.position.y;
            transform.LookAt(lookTarget);

            // Calculate shoot direction from shootPoint to actual hit point, but flat on Y
            Vector3 targetPoint = hit.point;
            targetPoint.y = shootPoint.position.y;
            Vector3 shootDirection = (targetPoint - shootPoint.position).normalized;
            
            ServerSpawnFireball(shootPoint.position, shootDirection);
        }
        else
        {
            // Fallback if no hit (e.g. looking at sky): shoot forward
            ServerSpawnFireball(shootPoint.position, transform.forward);
        }
    }

    [ServerRpc]
    private void ServerSpawnFireball(Vector3 position, Vector3 direction)
    {
        if (fireballPrefab == null) return;

        GameObject fireball = Instantiate(fireballPrefab, position, Quaternion.LookRotation(direction));
        Spawn(fireball);

        FireballProjectile projectile = fireball.GetComponent<FireballProjectile>();
        if (projectile != null)
        {
            projectile.Initialize(damage, this.gameObject);
            projectile.Launch(direction * speed);
        }

        // Destroy after range time
        Destroy(fireball, range / speed);
    }
}
