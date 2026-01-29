using FishNet.Object;
using FishNet.Connection;
using UnityEngine;
using UnityEngine.AI;

public class AutoAttackSystem : NetworkBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float baseAttackDamage = 10f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 1f;

    public float AttackDamage { get; set; }

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private float lastAttackTime;
    private Transform currentTarget;
    private NavMeshAgent navMeshAgent;

    public Transform GetCurrentTarget() => currentTarget;

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        AttackDamage = baseAttackDamage;
    }

    private void Update()
    {
        if (!IsOwner) return;

        // Auto-attack on Shift + Left Click
        if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetMouseButton(0))
        {
            TryAutoAttack();
        }
    }

    private void TryAutoAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;

        // Find all entities with HealthSystem in range
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange);
        float closestDistance = attackRange;
        Transform closestEnemy = null;

        TeamComponent myTeam = GetComponent<TeamComponent>();

        Debug.Log($"[AutoAttack] {gameObject.name} searching for targets. My team: {(myTeam != null ? myTeam.Team.ToString() : "NO TEAM")}");

        foreach (Collider hit in hits)
        {
            if (hit.transform == transform) continue;

            // Check if it has a health system (is damageable)
            HealthSystem healthSystem = hit.GetComponent<HealthSystem>();
            if (healthSystem == null || healthSystem.IsDead) continue;

            // Check if it's an enemy (not on our team)
            TeamComponent targetTeam = hit.GetComponent<TeamComponent>();
            if (targetTeam != null && myTeam != null)
            {
                // Skip allies and neutrals
                if (!myTeam.IsEnemyOf(targetTeam))
                {
                    Debug.Log($"[AutoAttack] {gameObject.name} skipping {hit.name} (not enemy)");
                    continue;
                }
            }
            else if (targetTeam == null)
            {
                Debug.Log($"[AutoAttack] {hit.name} has no TeamComponent, treating as valid target");
            }

            float distance = Vector3.Distance(transform.position, hit.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = hit.transform;
            }
        }

        if (closestEnemy != null)
        {
            Debug.Log($"[AutoAttack] {gameObject.name} found target: {closestEnemy.name}");
            PerformAttack(closestEnemy);
        }
        else
        {
            Debug.Log($"[AutoAttack] {gameObject.name} found no valid targets in range");
        }
    }

    private void PerformAttack(Transform target)
    {
        currentTarget = target;
        lastAttackTime = Time.time;

        // Stop the player from moving
        if (navMeshAgent != null)
        {
            navMeshAgent.ResetPath();
            navMeshAgent.velocity = Vector3.zero;
        }

        // Play animation
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // Look at target
        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        // Get NetworkObject for network-safe reference
        NetworkObject targetNetObj = target.GetComponentInParent<NetworkObject>();
        if (targetNetObj != null)
        {
            Debug.Log($"Sending attack to server for target: {target.name} (NetObj ID: {targetNetObj.ObjectId})");
            ServerAttack(targetNetObj);
        }
        else
        {
            Debug.LogWarning($"Target {target.name} has no NetworkObject component!");
        }
    }

    [ServerRpc]
    private void ServerAttack(NetworkObject targetNetObj)
    {
        if (targetNetObj == null)
        {
            Debug.LogWarning("ServerAttack: targetNetObj is null");
            return;
        }

        Debug.Log($"Server received attack for target: {targetNetObj.name}");

        HealthSystem targetHealth = targetNetObj.GetComponent<HealthSystem>();
        if (targetHealth != null)
        {
            Debug.Log($"Dealing {AttackDamage} damage to {targetNetObj.name}");
            targetHealth.TakeDamage(AttackDamage, gameObject);
        }
        else
        {
            Debug.LogWarning($"Target {targetNetObj.name} has no HealthSystem component!");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
