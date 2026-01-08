using FishNet.Object;
using UnityEngine;

public class AutoAttackSystem : NetworkBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private float lastAttackTime;
    private Transform currentTarget;

    private void Update()
    {
        if (!IsOwner) return;

        // Auto-attack on mouse click or space
        if (Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space))
        {
            TryAutoAttack();
        }
    }

    private void TryAutoAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;

        // Find closest enemy in range
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);
        float closestDistance = attackRange;
        Transform closestEnemy = null;

        foreach (Collider hit in hits)
        {
            if (hit.transform == transform) continue;

            float distance = Vector3.Distance(transform.position, hit.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = hit.transform;
            }
        }

        if (closestEnemy != null)
        {
            Debug.Log("Found target!");
            PerformAttack(closestEnemy.gameObject);
        }
    }

    private void PerformAttack(GameObject target)
    {
        lastAttackTime = Time.time;

        // Play animation
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // Look at target
        Vector3 direction = (target.transform.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        // Deal damage on server
        ServerAttack(target);
    }

    [ServerRpc]
    private void ServerAttack(GameObject target)
    {
        if (target == null) return;

        HealthSystem targetHealth = target.GetComponent<HealthSystem>();
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(attackDamage, gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
