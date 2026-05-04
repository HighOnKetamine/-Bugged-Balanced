using UnityEngine;
using UnityEngine.AI;
using FishNet.Object;
using FishNet.Object.Synchronizing;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(HealthComponent))]
[RequireComponent(typeof(TeamComponent))]
public class MinionStateMachine : StateMachine<MinionStateMachine>
{
    [Header("Stats")]
    public float attackDamage = 25f;
    public float attackRange = 2f;
    public float attackCooldown = 1.25f;
    public float aggroRange = 8f;
    public int goldReward = 25;

    // --- References ---
    public NavMeshAgent NavMeshAgent { get; private set; }
    public HealthComponent Health { get; private set; }
    public TeamComponent Team { get; private set; }

    // --- Lane state ---
    public Lane AssignedLane { get; private set; }
    public int CurrentWaypointIndex { get; set; }

    // --- Combat state ---
    public GameObject CurrentTarget { get; set; }
    public float LastAttackTime { get; set; }

    private void Awake()
    {
        NavMeshAgent = GetComponent<NavMeshAgent>();
        Health = GetComponent<HealthComponent>();
        Team = GetComponent<TeamComponent>();

        if (NavMeshAgent == null) Debug.LogError($"[MinionStateMachine] Missing NavMeshAgent on {gameObject.name}");
        if (Health == null) Debug.LogError($"[MinionStateMachine] Missing HealthComponent on {gameObject.name}");
        if (Team == null) Debug.LogError($"[MinionStateMachine] Missing TeamComponent on {gameObject.name}");
    }

    [Server]
    public void Initialize(Lane lane)
    {
        AssignedLane = lane;
        Team.SetTeam(lane.teamId);
        CurrentWaypointIndex = 0;

        Health.OnDeath += _ => ChangeState(new MinionDeathState(this));
        ChangeState(new MinionRunState(this));
    }
}