using UnityEngine;

public class TowerIdleState : State<TowerStateMachine>
{
    public TowerIdleState(TowerStateMachine machine) : base(machine) { }

    public override void Enter()
    {
        Machine.CurrentTarget = null;
    }

    public override void Update()
    {
        GameObject target = FindPriorityTarget();
        if (target != null)
        {
            Machine.CurrentTarget = target;
            Machine.ChangeState(new TowerAttackState(Machine));
        }
    }

    public override void Exit() { }

    private GameObject FindPriorityTarget()
    {
        Collider[] hits = Physics.OverlapSphere(
            Machine.transform.position,
            Machine.aggroRange,
            LayerMask.GetMask("Targetable")
        );

        GameObject nearestPlayer = null;
        float nearestPlayerDist = float.MaxValue;
        GameObject nearestMinion = null;
        float nearestMinionDist = float.MaxValue;

        foreach (Collider col in hits)
        {
            TeamComponent tc = col.GetComponent<TeamComponent>();
            HealthComponent hc = col.GetComponent<HealthComponent>();

            if (tc == null || hc == null || hc.IsDead) continue;
            if (!Machine.Team.IsEnemy(tc)) continue;

            float dist = Vector3.Distance(Machine.transform.position, col.transform.position);

            if (col.GetComponent<PlayerStateMachine>() != null)
            {
                if (dist < nearestPlayerDist)
                {
                    nearestPlayerDist = dist;
                    nearestPlayer = col.gameObject;
                }
            }
            else if (col.GetComponent<MinionStateMachine>() != null)
            {
                if (dist < nearestMinionDist)
                {
                    nearestMinionDist = dist;
                    nearestMinion = col.gameObject;
                }
            }
        }

        return nearestMinion ?? nearestPlayer;
    }
}