public class RunState : State<PlayerStateMachine>
{
    public RunState(PlayerStateMachine machine) : base(machine) { }

    public override void Enter()
    {
        Machine.Animator.SetBool("IsMoving", true);
    }

    public override void Update()
    {
        Machine.NavMeshAgent.speed = Machine.Stats.moveSpeed.Value;

        if (Machine.NavMeshAgent.pathPending) return;

        if (Machine.AttackMoveTarget != null)
        {
            HealthComponent health = Machine.AttackMoveTarget.GetComponent<HealthComponent>();
            if (health == null || health.IsDead)
            {
                Machine.AttackMoveTarget = null;
                Machine.ChangeState(new IdleState(Machine));
                return;
            }
            if (!Machine.BasicAttack.IsOffCooldown())
            {
                Machine.AttackMoveTarget = null;
                Machine.ChangeState(new IdleState(Machine));
                return;
            }
            if (Machine.BasicAttack.IsInRange(Machine.AttackMoveTarget))
            {
                var target = Machine.AttackMoveTarget;
                Machine.AttackMoveTarget = null;
                Machine.BasicAttack.Attack(target);
            }
            return;
        }

        if (Machine.NavMeshAgent.remainingDistance <= Machine.NavMeshAgent.stoppingDistance)
            Machine.ChangeState(new IdleState(Machine));
    }

    public override void Exit() { }
}