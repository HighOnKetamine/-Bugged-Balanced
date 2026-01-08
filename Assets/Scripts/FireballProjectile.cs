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
        HandleHit(other.gameObject, other.transform, other.isTrigger);
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleHit(collision.gameObject, collision.transform, false);
    }

    private void HandleHit(GameObject hitObj, Transform hitTransform, bool isTrigger)
    {
        if (!IsServerInitialized) return;
        if (isTrigger) return; // Don't explode on other triggers
        if (hitTransform.root == owner.transform.root) return; // Don't hit owner or owner's children

        // Create a separate variable for hit log to avoid spam if feasible, but here we just log
        Debug.Log($"Fireball hit object: {hitObj.name}");

        HealthSystem health = hitObj.GetComponentInParent<HealthSystem>();
        if (health != null)
        {
            Debug.Log($"Found HealthSystem on {health.gameObject.name}, dealing {damage} damage.");
            health.TakeDamage(damage, owner);
        }
        else
        {
            Debug.Log("No HealthSystem found on hit object.");
        }

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

