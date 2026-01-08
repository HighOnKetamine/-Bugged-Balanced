using UnityEngine;
using FishNet.Object;



public class FireballProjectile : NetworkBehaviour
{
    private float damage;
    private GameObject owner;

    [SerializeField] private GameObject hitEffectPrefab;

    public void Initialize(float dmg, GameObject own)
    {
        damage = dmg;
        owner = own;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServerInitialized) return;
        if (other.gameObject == owner) return;

        HealthSystem health = other.GetComponent<HealthSystem>();
        if (health != null)
        {
            health.TakeDamage(damage, owner);

            // Spawn hit effect
            if (hitEffectPrefab != null)
            {
                GameObject effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                Spawn(effect);
                Destroy(effect, 2f);
            }

            Despawn(gameObject);
        }
    }
}

