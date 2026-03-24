public class IdleState : PlayerState
{
    public IdleState(PlayerStateMachine machine) : base(machine) { }

    public override void Enter()
    {
        _machine.Animator.SetBool("IsMoving", false);
    }

    public override void Update()
    {
        if (_machine.NavMeshAgent.remainingDistance > _machine.NavMeshAgent.stoppingDistance)
            _machine.ChangeState(new RunState(_machine));
    }

    public override void Exit() { }
}