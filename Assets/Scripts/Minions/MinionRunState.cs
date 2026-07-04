using UnityEngine;

public class MinionRunState : State<MinionStateMachine>
{
    public MinionRunState(MinionStateMachine machine) : base(machine) { }

    public override void Enter()
    {
        Machine.SetMoving(true);
        AdvanceToWaypoint();
    }

    public override void Update()
    {
        // Units first — structures are lower priority than minions/players.
        GameObject target = FindNearestUnit();
        if (target == null)
            target = FindNearestStructure();

        if (target != null)
        {
            Machine.CurrentTarget = target;
            Machine.ChangeState(new MinionChaseAttackState(Machine));
            return;
        }

        var agent = Machine.NavMeshAgent;
        if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh) return;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            if (Machine.AssignedLane.IsLastWaypoint(Machine.CurrentWaypointIndex))
                return;

            Machine.CurrentWaypointIndex++;
            AdvanceToWaypoint();
        }
    }

    public override void Exit() { }

    private void AdvanceToWaypoint()
    {
        Transform wp = Machine.AssignedLane.GetWaypoint(Machine.CurrentWaypointIndex);
        if (wp != null)
            Machine.NavMeshAgent.SetDestination(wp.position);
    }

    // Returns the nearest alive enemy UNIT (minion / player) within aggroRange.
    private GameObject FindNearestUnit()
    {
        Collider[] hits = Physics.OverlapSphere(
            Machine.transform.position,
            Machine.aggroRange,
            LayerMask.GetMask("Targetable")
        );

        GameObject nearest  = null;
        float      nearestD = Mathf.Infinity;

        foreach (Collider col in hits)
        {
            if (IsStructure(col.gameObject)) continue;

            TeamComponent   targetTeam   = col.GetComponent<TeamComponent>();
            HealthComponent targetHealth = col.GetComponent<HealthComponent>();

            if (targetTeam == null || targetHealth == null || targetHealth.IsDead) continue;
            if (!Machine.Team.IsEnemy(targetTeam)) continue;

            float d = Vector3.Distance(Machine.transform.position, col.transform.position);
            if (d < nearestD) { nearestD = d; nearest = col.gameObject; }
        }

        return nearest;
    }

    // Returns the nearest alive enemy STRUCTURE (tower / inhibitor / nexus).
    // Uses all-layers search with a wider radius since structures may not be on
    // the "Targetable" layer and sit at the end of each lane segment.
    private GameObject FindNearestStructure()
    {
        Collider[] hits = Physics.OverlapSphere(
            Machine.transform.position,
            Machine.aggroRange * 2f,
            ~0
        );

        GameObject nearest  = null;
        float      nearestD = Mathf.Infinity;

        foreach (Collider col in hits)
        {
            if (!IsStructure(col.gameObject)) continue;

            TeamComponent   targetTeam   = col.GetComponent<TeamComponent>();
            HealthComponent targetHealth = col.GetComponent<HealthComponent>();

            if (targetTeam == null || targetHealth == null || targetHealth.IsDead) continue;
            if (!Machine.Team.IsEnemy(targetTeam)) continue;

            float d = Vector3.Distance(Machine.transform.position, col.transform.position);
            if (d < nearestD) { nearestD = d; nearest = col.gameObject; }
        }

        return nearest;
    }

    internal static bool IsStructure(GameObject go) =>
        go.GetComponent<TowerStateMachine>()      != null ||
        go.GetComponent<InhibitorStateMachine>()  != null ||
        go.GetComponent<NexusStateMachine>()      != null;
}
