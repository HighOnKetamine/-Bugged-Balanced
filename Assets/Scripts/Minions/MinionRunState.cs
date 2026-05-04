using UnityEngine;

public class MinionRunState : State<MinionStateMachine>
{
    public MinionRunState(MinionStateMachine machine) : base(machine) { }

    public override void Enter()
    {
        Machine.NavMeshAgent.isStopped = false;
        AdvanceToWaypoint();
    }

    public override void Update()
    {
        GameObject target = FindNearestEnemy();
        if (target != null)
        {
            Machine.CurrentTarget = target;
            Machine.ChangeState(new MinionChaseAttackState(Machine));
            return;
        }

        if (!Machine.NavMeshAgent.pathPending &&
            Machine.NavMeshAgent.remainingDistance < 0.5f)
        {
            if (Machine.AssignedLane.IsLastWaypoint(Machine.CurrentWaypointIndex))
                return; // TODO: útok na nexus

            Machine.CurrentWaypointIndex++;
            AdvanceToWaypoint();
        }
    }

    public override void Exit()
    {
        Machine.NavMeshAgent.isStopped = true;
    }

    private void AdvanceToWaypoint()
    {
        Transform wp = Machine.AssignedLane.GetWaypoint(Machine.CurrentWaypointIndex);
        if (wp != null)
            Machine.NavMeshAgent.SetDestination(wp.position);
    }

    private GameObject FindNearestEnemy()
    {
        Collider[] hits = Physics.OverlapSphere(
            Machine.transform.position,
            Machine.aggroRange,
            LayerMask.GetMask("Characters")
        );

        GameObject nearest = null;
        float nearestDist = Mathf.Infinity;

        foreach (Collider col in hits)
        {
            TeamComponent targetTeam = col.GetComponent<TeamComponent>();
            HealthComponent targetHealth = col.GetComponent<HealthComponent>();

            if (targetTeam == null || targetHealth == null || targetHealth.IsDead) continue;
            if (!Machine.Team.IsEnemy(targetTeam)) continue;

            float dist = Vector3.Distance(Machine.transform.position, col.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = col.gameObject;
            }
        }

        return nearest;
    }
}