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
            // Skip anything that is part of a structure hierarchy.
            if (GetStructureRoot(col) != null) continue;

            TeamComponent   targetTeam   = col.GetComponentInParent<TeamComponent>();
            HealthComponent targetHealth = col.GetComponentInParent<HealthComponent>();

            if (targetTeam == null || targetHealth == null || targetHealth.IsDead) continue;
            if (!Machine.Team.IsEnemy(targetTeam)) continue;

            GameObject go = targetTeam.gameObject;
            float d = Vector3.Distance(Machine.transform.position, go.transform.position);
            if (d < nearestD) { nearestD = d; nearest = go; }
        }

        return nearest;
    }

    // Returns the nearest alive enemy STRUCTURE (tower / inhibitor / nexus).
    // Uses all-layers search and GetComponentInParent so colliders nested
    // anywhere under the structure — or under a scene parent group — are found.
    private GameObject FindNearestStructure()
    {
        Collider[] hits = Physics.OverlapSphere(
            Machine.transform.position,
            Machine.aggroRange * 2f,
            ~0
        );

        GameObject nearest  = null;
        float      nearestD = Mathf.Infinity;
        var seen = new System.Collections.Generic.HashSet<GameObject>();

        foreach (Collider col in hits)
        {
            GameObject structureRoot = GetStructureRoot(col);
            if (structureRoot == null) continue;
            if (!seen.Add(structureRoot)) continue;

            TeamComponent   targetTeam   = structureRoot.GetComponent<TeamComponent>();
            HealthComponent targetHealth = structureRoot.GetComponent<HealthComponent>();

            if (targetTeam == null || targetHealth == null || targetHealth.IsDead) continue;
            if (!Machine.Team.IsEnemy(targetTeam)) continue;

            float d = Vector3.Distance(Machine.transform.position, structureRoot.transform.position);
            if (d < nearestD) { nearestD = d; nearest = structureRoot; }
        }

        return nearest;
    }

    // Walks up from a collider using GetComponentInParent so the structure is
    // found whether the collider is a direct child or nested several levels deep,
    // and regardless of scene-level parent groups.
    private static GameObject GetStructureRoot(Collider col)
    {
        var ts = col.GetComponentInParent<TowerStateMachine>();
        if (ts  != null) return ts.gameObject;
        var ims = col.GetComponentInParent<InhibitorStateMachine>();
        if (ims != null) return ims.gameObject;
        var ns  = col.GetComponentInParent<NexusStateMachine>();
        if (ns  != null) return ns.gameObject;
        return null;
    }

    // Used by MinionChaseAttackState to identify a structure target.
    internal static bool IsStructure(GameObject go) =>
        go.GetComponent<TowerStateMachine>()     != null ||
        go.GetComponent<InhibitorStateMachine>() != null ||
        go.GetComponent<NexusStateMachine>()     != null;
}
