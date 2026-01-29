using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.AI;

public enum MinionState
{
    LanePush,
    AggroChampion,
    Attacking,
    Dead
}

/// <summary>
/// Server-authoritative Minion AI with FSM.
/// Uses NavMeshAgent for pathfinding and MOBA-standard target priority.
/// Syncs position via NetworkTransform and state via SyncVar.
/// </summary>
[RequireComponent(typeof(NavMeshAgent), typeof(TeamComponent), typeof(HealthSystem))]
public class MinionAI : NetworkBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 1f;

    [Header("Lane Pushing")]
    [SerializeField] private Transform[] laneWaypoints;
    [SerializeField] private int currentWaypointIndex = 0;
    [SerializeField] private float waypointReachDistance = 1f;
    [Tooltip("Maximum distance minion can chase from current waypoint before giving up")]
    [SerializeField] private float maxChaseDistance = 15f;

    [Header("Champion Detection")]
    [SerializeField] private LayerMask championLayer;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    // Synced state
    [SerializeField]
    private readonly SyncVar<MinionState> currentState = new SyncVar<MinionState>(new SyncTypeSettings(WritePermission.ServerOnly, ReadPermission.Observers));

    // Server-only variables
    private NavMeshAgent navAgent;
    private TeamComponent teamComponent;
    private HealthSystem healthSystem;
    private Transform currentTarget;
    private float lastAttackTime;

    public MinionState CurrentState => currentState.Value;
    public Transform GetCurrentTarget() => currentTarget;

    private void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        teamComponent = GetComponent<TeamComponent>();
        healthSystem = GetComponent<HealthSystem>();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        
        currentState.Value = MinionState.LanePush;
        
        // Subscribe to death event
        if (healthSystem != null)
        {
            healthSystem.OnDeath += OnMinionDeath;
        }
    }

    private void Update()
    {
        // Only server runs AI logic
        if (!IsServerInitialized) return;
        if (currentState.Value == MinionState.Dead) return;

        RunFSM();
    }

    [Server]
    private void RunFSM()
    {
        switch (currentState.Value)
        {
            case MinionState.LanePush:
                HandleLanePush();
                break;
            case MinionState.AggroChampion:
                HandleAggroChampion();
                break;
            case MinionState.Attacking:
                HandleAttacking();
                break;
        }
    }

    [Server]
    private void HandleLanePush()
    {
        // Check for enemies in range (now detects anything with HealthSystem)
        Transform target = TargetingUtility.GetBestTarget(
            transform.position,
            teamComponent,
            detectionRange,
            championLayer);

        if (target != null)
        {
            currentTarget = target;
            
            // Check if it's a champion by layer
            bool isChampion = ((1 << target.gameObject.layer) & championLayer) != 0;
            
            Debug.Log($"[MinionAI] {gameObject.name} found target: {target.name} (IsChampion: {isChampion})");
            
            if (isChampion)
            {
                TransitionToState(MinionState.AggroChampion);
            }
            else
            {
                TransitionToState(MinionState.Attacking);
            }
            return;
        }

        // Continue pushing lane
        if (laneWaypoints != null && laneWaypoints.Length > 0)
        {
            Transform targetWaypoint = laneWaypoints[currentWaypointIndex];
            
            if (Vector3.Distance(transform.position, targetWaypoint.position) < waypointReachDistance)
            {
                // Move to next waypoint
                currentWaypointIndex = (currentWaypointIndex + 1) % laneWaypoints.Length;
                targetWaypoint = laneWaypoints[currentWaypointIndex];
            }

            navAgent.SetDestination(targetWaypoint.position);
        }
    }

    [Server]
    private void HandleAggroChampion()
    {
        if (currentTarget == null || !IsTargetValid(currentTarget))
        {
            Debug.Log($"[MinionAI] {gameObject.name} target invalid, returning to LanePush");
            currentTarget = null;
            TransitionToState(MinionState.LanePush);
            return;
        }

        // Check if we've chased too far from waypoint (leash mechanic)
        if (laneWaypoints != null && laneWaypoints.Length > 0)
        {
            Transform currentWaypoint = laneWaypoints[currentWaypointIndex];
            float distanceFromWaypoint = Vector3.Distance(transform.position, currentWaypoint.position);
            
            if (distanceFromWaypoint > maxChaseDistance)
            {
                Debug.Log($"[MinionAI] {gameObject.name} exceeded leash distance ({distanceFromWaypoint:F2} > {maxChaseDistance}), returning to lane");
                currentTarget = null;
                TransitionToState(MinionState.LanePush);
                return;
            }
        }

        // Move toward champion
        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
        
        if (distanceToTarget <= attackRange)
        {
            Debug.Log($"[MinionAI] {gameObject.name} in range ({distanceToTarget:F2} <= {attackRange}), switching to Attacking");
            TransitionToState(MinionState.Attacking);
        }
        else if (distanceToTarget > detectionRange)
        {
            // Lost aggro
            Debug.Log($"[MinionAI] {gameObject.name} lost aggro (distance: {distanceToTarget:F2})");
            currentTarget = null;
            TransitionToState(MinionState.LanePush);
        }
        else
        {
            navAgent.SetDestination(currentTarget.position);
        }
    }

    [Server]
    private void HandleAttacking()
    {
        if (currentTarget == null || !IsTargetValid(currentTarget))
        {
            Debug.Log($"[MinionAI] {gameObject.name} target invalid in Attacking state");
            currentTarget = null;
            TransitionToState(MinionState.LanePush);
            return;
        }

        // Check leash distance even while attacking
        if (laneWaypoints != null && laneWaypoints.Length > 0)
        {
            Transform currentWaypoint = laneWaypoints[currentWaypointIndex];
            float distanceFromWaypoint = Vector3.Distance(transform.position, currentWaypoint.position);
            
            if (distanceFromWaypoint > maxChaseDistance)
            {
                Debug.Log($"[MinionAI] {gameObject.name} exceeded leash during attack, disengaging");
                currentTarget = null;
                TransitionToState(MinionState.LanePush);
                return;
            }
        }

        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);

        if (distanceToTarget > attackRange)
        {
            Debug.Log($"[MinionAI] {gameObject.name} target out of range ({distanceToTarget:F2} > {attackRange})");
            // Move back into range
            bool isChampion = ((1 << currentTarget.gameObject.layer) & championLayer) != 0;
            
            if (isChampion)
            {
                TransitionToState(MinionState.AggroChampion);
            }
            else
            {
                navAgent.SetDestination(currentTarget.position);
            }
            return;
        }

        // Stop moving and attack
        navAgent.ResetPath();
        
        // Face target
        Vector3 direction = (currentTarget.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        // Attack on cooldown
        float timeSinceLastAttack = Time.time - lastAttackTime;
        if (timeSinceLastAttack >= attackCooldown)
        {
            Debug.Log($"[MinionAI] {gameObject.name} performing attack (cooldown ready: {timeSinceLastAttack:F2} >= {attackCooldown})");
            PerformAttack();
        }
    }

    [Server]
    private void PerformAttack()
    {
        if (currentTarget == null) return;

        lastAttackTime = Time.time;

        // Play animation
        RpcPlayAttackAnimation();

        // Deal damage
        HealthSystem targetHealth = currentTarget.GetComponent<HealthSystem>();
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(attackDamage, gameObject);
            Debug.Log($"[MinionAI] {gameObject.name} attacked {currentTarget.name} for {attackDamage} damage");
        }
    }

    [ObserversRpc]
    private void RpcPlayAttackAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }

    [Server]
    private bool IsTargetValid(Transform target)
    {
        if (target == null) return false;

        HealthSystem targetHealth = target.GetComponent<HealthSystem>();
        if (targetHealth != null && targetHealth.IsDead) return false;

        TeamComponent targetTeam = target.GetComponent<TeamComponent>();
        if (targetTeam == null || !teamComponent.IsEnemyOf(targetTeam)) return false;

        return true;
    }

    [Server]
    private void TransitionToState(MinionState newState)
    {
        if (currentState.Value == newState) return;

        Debug.Log($"[MinionAI] {gameObject.name} transitioning from {currentState.Value} to {newState}");
        currentState.Value = newState;
    }

    [Server]
    private void OnMinionDeath()
    {
        Debug.Log($"[MinionAI] {gameObject.name} died");
        currentState.Value = MinionState.Dead;
        
        if (navAgent != null)
        {
            navAgent.ResetPath();
            navAgent.enabled = false;
        }

        // Economy manager will handle rewards
        // Object despawn should be handled by a separate system or after a delay
    }

    private void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Lane waypoints
        if (laneWaypoints != null && laneWaypoints.Length > 0)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < laneWaypoints.Length; i++)
            {
                if (laneWaypoints[i] != null)
                {
                    Gizmos.DrawSphere(laneWaypoints[i].position, 0.5f);
                    
                    if (i < laneWaypoints.Length - 1 && laneWaypoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(laneWaypoints[i].position, laneWaypoints[i + 1].position);
                    }
                }
            }
        }
    }
}
