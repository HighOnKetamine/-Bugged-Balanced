using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

/// <summary>
/// Server-authoritative Tower AI with "Call for Help" aggro swap.
/// Prioritizes enemies that attack allied champions within range.
/// </summary>
[RequireComponent(typeof(TeamComponent), typeof(HealthSystem))]
public class TowerAI : NetworkBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float detectionRange = 12f;
    [SerializeField] private float attackDamage = 50f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private Transform projectileSpawnPoint;

    [Header("Visuals")]
    [SerializeField] private GameObject projectilePrefab;

    [Header("Champion Detection")]
    [SerializeField] private LayerMask championLayer;

    // Synced current target for visual feedback
    [SerializeField]
    private readonly SyncVar<NetworkObject> currentTargetNetObj = new SyncVar<NetworkObject>(new SyncTypeSettings(WritePermission.ServerOnly, ReadPermission.Observers));

    // Server-only variables
    private TeamComponent teamComponent;
    private HealthSystem healthSystem;
    private Transform currentTarget;
    private float lastAttackTime;
    private SphereCollider detectionCollider;

    public Transform GetCurrentTarget() => currentTarget;

    private void Awake()
    {
        teamComponent = GetComponent<TeamComponent>();
        healthSystem = GetComponent<HealthSystem>();

        // Create detection sphere collider
        detectionCollider = gameObject.AddComponent<SphereCollider>();
        detectionCollider.isTrigger = true;
        detectionCollider.radius = detectionRange;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        // Subscribe to all HealthSystem damage events in range to detect "Call for Help"
        InvokeRepeating(nameof(ServerUpdateTargeting), 0f, 0.5f);
    }

    private void Update()
    {
        if (!IsServerInitialized) return;

        if (currentTarget != null && IsTargetValid(currentTarget))
        {
            // Attack on cooldown
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                PerformAttack();
            }
        }
    }

    [Server]
    private void ServerUpdateTargeting()
    {
        // Check for "Call for Help" - enemy champions attacking allied champions
        Transform callForHelpTarget = CheckForCallForHelp();
        
        if (callForHelpTarget != null)
        {
            // Immediately switch to aggressor
            if (currentTarget != callForHelpTarget)
            {
                Debug.Log($"[TowerAI] Call for Help! Switching to {callForHelpTarget.name}");
                currentTarget = callForHelpTarget;
                UpdateSyncedTarget();
            }
            return;
        }

        // If current target is still valid, keep it
        if (currentTarget != null && IsTargetValid(currentTarget))
        {
            return;
        }

        // Find new target using standard priority
        Transform newTarget = TargetingUtility.GetBestTarget(
            transform.position,
            teamComponent,
            detectionRange,
            championLayer);

        if (newTarget != currentTarget)
        {
            currentTarget = newTarget;
            UpdateSyncedTarget();
        }
    }

    [Server]
    private Transform CheckForCallForHelp()
    {
        // Find all entities in range
        Collider[] allInRange = Physics.OverlapSphere(transform.position, detectionRange);

        foreach (Collider col in allInRange)
        {
            // Must have HealthSystem to be attackable
            HealthSystem health = col.GetComponent<HealthSystem>();
            if (health == null || health.IsDead) continue;

            TeamComponent entityTeam = col.GetComponent<TeamComponent>();
            if (entityTeam == null) continue;

            // Check if this is an enemy
            if (!teamComponent.IsEnemyOf(entityTeam)) continue;

            // Check if this enemy is attacking an allied champion
            AutoAttackSystem attackSystem = col.GetComponent<AutoAttackSystem>();
            if (attackSystem == null) continue;

            Transform enemyTarget = attackSystem.GetCurrentTarget();
            if (enemyTarget == null) continue;

            // Check if the enemy's target is an allied champion (check by layer)
            TeamComponent targetTeam = enemyTarget.GetComponent<TeamComponent>();
            if (targetTeam != null && teamComponent.IsAllyOf(targetTeam))
            {
                bool isChampion = ((1 << enemyTarget.gameObject.layer) & championLayer) != 0;
                if (isChampion)
                {
                    // This enemy is attacking an allied champion!
                    return col.transform;
                }
            }
        }

        return null;
    }

    [Server]
    private void PerformAttack()
    {
        if (currentTarget == null) return;

        lastAttackTime = Time.time;

        // Deal damage
        HealthSystem targetHealth = currentTarget.GetComponent<HealthSystem>();
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(attackDamage, gameObject);
            Debug.Log($"[TowerAI] {gameObject.name} attacked {currentTarget.name} for {attackDamage} damage");
        }

        // Spawn projectile visual on all clients
        Vector3 spawnPos = projectileSpawnPoint != null ? projectileSpawnPoint.position : transform.position + Vector3.up * 2f;
        RpcSpawnProjectile(spawnPos, currentTarget.position);
    }

    [ObserversRpc]
    private void RpcSpawnProjectile(Vector3 startPos, Vector3 targetPos)
    {
        if (projectilePrefab != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, startPos, Quaternion.identity);
            TowerProjectile projScript = projectile.GetComponent<TowerProjectile>();
            
            if (projScript != null)
            {
                projScript.Initialize(startPos, targetPos);
            }
            else
            {
                // Fallback: just destroy after a delay
                Destroy(projectile, 2f);
            }
        }
    }

    [Server]
    private bool IsTargetValid(Transform target)
    {
        if (target == null) return false;

        // Check if target is still alive
        HealthSystem targetHealth = target.GetComponent<HealthSystem>();
        if (targetHealth != null && targetHealth.IsDead) return false;

        // Check if target is still an enemy
        TeamComponent targetTeam = target.GetComponent<TeamComponent>();
        if (targetTeam == null || !teamComponent.IsEnemyOf(targetTeam)) return false;

        // Check if target is still in range
        float distance = Vector3.Distance(transform.position, target.position);
        if (distance > detectionRange) return false;

        return true;
    }

    [Server]
    private void UpdateSyncedTarget()
    {
        if (currentTarget != null)
        {
            NetworkObject netObj = currentTarget.GetComponent<NetworkObject>();
            currentTargetNetObj.Value = netObj;
        }
        else
        {
            currentTargetNetObj.Value = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        if (currentTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
    }

    private void OnDestroy()
    {
        if (IsServerInitialized)
        {
            CancelInvoke(nameof(ServerUpdateTargeting));
        }
    }
}
