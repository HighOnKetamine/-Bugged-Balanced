using UnityEngine;
using UnityEngine.AI;
using FishNet.Object;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NavMeshObstacle))]
[RequireComponent(typeof(HealthComponent))]
[RequireComponent(typeof(TeamComponent))]
[RequireComponent(typeof(CharacterStats))]
public class MinionStateMachine : StateMachine<MinionStateMachine>
{
    [Header("AI")]
    public float aggroRange = 8f;

    public NavMeshAgent NavMeshAgent { get; private set; }
    public NavMeshObstacle NavMeshObstacle { get; private set; }
    public HealthComponent Health { get; private set; }
    public TeamComponent Team { get; private set; }
    public CharacterStats Stats { get; private set; }

    public Lane AssignedLane { get; private set; }
    public int CurrentWaypointIndex { get; set; }
    public GameObject CurrentTarget { get; set; }
    public float LastAttackTime { get; set; }

    private void Awake()
    {
        NavMeshAgent = GetComponent<NavMeshAgent>();
        NavMeshObstacle = GetComponent<NavMeshObstacle>();
        Health = GetComponent<HealthComponent>();
        Team = GetComponent<TeamComponent>();
        Stats = GetComponent<CharacterStats>();

        if (NavMeshAgent == null) Debug.LogError($"[MinionStateMachine] Missing NavMeshAgent on {gameObject.name}");
        if (NavMeshObstacle == null) Debug.LogError($"[MinionStateMachine] Missing NavMeshObstacle on {gameObject.name}");
        if (Health == null) Debug.LogError($"[MinionStateMachine] Missing HealthComponent on {gameObject.name}");
        if (Team == null) Debug.LogError($"[MinionStateMachine] Missing TeamComponent on {gameObject.name}");
        if (Stats == null) Debug.LogError($"[MinionStateMachine] Missing CharacterStats on {gameObject.name}");
    }

    [Server]
    public void Initialize(Lane lane, sbyte teamId)
    {
        AssignedLane = lane;
        CurrentWaypointIndex = 0;
        Team.SetTeam(teamId);

        Health.OnDeath += _ => ChangeState(new MinionDeathState(this));
        ChangeState(new MinionRunState(this));
    }

    public void SetMoving(bool moving)
    {
        if (moving)
        {
            NavMeshObstacle.enabled = false;
            NavMeshAgent.enabled = true;
            NavMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
        }
        else
        {
            NavMeshAgent.isStopped = true;
            NavMeshAgent.ResetPath();
            NavMeshAgent.enabled = false;
            NavMeshObstacle.enabled = true;
        }
    }
}