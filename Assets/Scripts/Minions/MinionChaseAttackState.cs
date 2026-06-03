using UnityEngine;

public class MinionChaseAttackState : State<MinionStateMachine>
{
    private bool _isStopped = false;
    private Vector3 _leashPoint;
    private const float LeashRadius = 10f;

    public MinionChaseAttackState(MinionStateMachine machine) : base(machine) { }

    public override void Enter()
    {
        _isStopped = false;
        _leashPoint = Machine.transform.position;
        Machine.SetMoving(true);
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

        float distFromLeash = Vector3.Distance(Machine.transform.position, _leashPoint);
        if (distFromLeash > LeashRadius)
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

        if (dist <= Machine.Stats.attackRange.Value)
        {
            if (!_isStopped)
            {
                Machine.SetMoving(false);
                _isStopped = true;
            }

            float cooldown = 1f / Machine.Stats.attackSpeed.Value;
            if (Time.time - Machine.LastAttackTime >= cooldown)
            {
                Machine.LastAttackTime = Time.time;
                Machine.CurrentTarget.GetComponent<HealthComponent>()?
                    .TakeDamage(Machine.Stats.attackDamage.Value, DamageType.Physical, Machine.Stats, DamageSource.AutoAttack);
            }
        }
        else
        {
            if (_isStopped)
            {
                Machine.SetMoving(true);
                _isStopped = false;
            }
            Machine.NavMeshAgent.SetDestination(Machine.CurrentTarget.transform.position);
        }
    }

    public override void Exit()
    {
        _isStopped = false;
        Machine.SetMoving(true);
    }
}