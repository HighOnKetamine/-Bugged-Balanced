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
            GameObject root = col.transform.root.gameObject;
            if (IsStructure(root)) continue;

            TeamComponent   targetTeam   = root.GetComponent<TeamComponent>();
            HealthComponent targetHealth = root.GetComponent<HealthComponent>();

            if (targetTeam == null || targetHealth == null || targetHealth.IsDead) continue;
            if (!Machine.Team.IsEnemy(targetTeam)) continue;

            float d = Vector3.Distance(Machine.transform.position, root.transform.position);
            if (d < nearestD) { nearestD = d; nearest = root; }
        }

        return nearest;
    }

    // Returns the nearest alive enemy STRUCTURE (tower / inhibitor / nexus).
    // Uses all-layers search: structure colliders are often on child meshes so
    // each hit is resolved to its transform root before component checks.
    private GameObject FindNearestStructure()
    {
        Collider[] hits = Physics.OverlapSphere(
            Machine.transform.position,
            Machine.aggroRange * 2f,
            ~0
        );

        GameObject nearest  = null;
        float      nearestD = Mathf.Infinity;
        // Deduplicate: multiple child colliders on the same structure root.
        var seen = new System.Collections.Generic.HashSet<GameObject>();

        foreach (Collider col in hits)
        {
            GameObject root = col.transform.root.gameObject;
            if (!seen.Add(root)) continue;
            if (!IsStructure(root)) continue;

            TeamComponent   targetTeam   = root.GetComponent<TeamComponent>();
            HealthComponent targetHealth = root.GetComponent<HealthComponent>();

            if (targetTeam == null || targetHealth == null || targetHealth.IsDead) continue;
            if (!Machine.Team.IsEnemy(targetTeam)) continue;

            float d = Vector3.Distance(Machine.transform.position, root.transform.position);
            if (d < nearestD) { nearestD = d; nearest = root; }
        }

        return nearest;
    }

    internal static bool IsStructure(GameObject go) =>
        go.GetComponent<TowerStateMachine>()      != null ||
        go.GetComponent<InhibitorStateMachine>()  != null ||
        go.GetComponent<NexusStateMachine>()      != null;
}
