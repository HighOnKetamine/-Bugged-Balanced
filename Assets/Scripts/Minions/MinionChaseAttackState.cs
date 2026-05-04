using UnityEngine;

public class MinionChaseAttackState : State<MinionStateMachine>
{
    public MinionChaseAttackState(MinionStateMachine machine) : base(machine) { }

    public override void Enter()
    {
        Machine.NavMeshAgent.isStopped = false;
    }

    public override void Update()
    {
        if (Machine.CurrentTarget == null ||
            Machine.CurrentTarget.GetComponent<HealthComponent>()?.IsDead == true)
        {
            Machine.CurrentTarget = null;
            Machine.ChangeState(new MinionRunState(Machine));
            return;
        }

        float dist = Vector3.Distance(Machine.transform.position, Machine.CurrentTarget.transform.position);

        if (dist > Machine.aggroRange * 1.5f)
        {
            Machine.CurrentTarget = null;
            Machine.ChangeState(new MinionRunState(Machine));
            return;
        }

        if (dist <= Machine.attackRange)
        {
            Machine.NavMeshAgent.isStopped = true;

            if (Time.time - Machine.LastAttackTime >= Machine.attackCooldown)
            {
                Machine.LastAttackTime = Time.time;
                Machine.CurrentTarget.GetComponent<HealthComponent>()?.TakeDamage(Machine.attackDamage, DamageType.Physical);
            }
        }
        else
        {
            Machine.NavMeshAgent.isStopped = false;
            Machine.NavMeshAgent.SetDestination(Machine.CurrentTarget.transform.position);
        }
    }

    public override void Exit()
    {
        Machine.NavMeshAgent.isStopped = false;
    }
}