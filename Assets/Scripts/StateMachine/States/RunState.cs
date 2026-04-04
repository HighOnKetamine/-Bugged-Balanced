public class RunState : PlayerState
{
    public RunState(PlayerStateMachine machine) : base(machine) { }

    public override void Enter()
    {
        _machine.Animator.SetBool("IsMoving", true);
    }

    public override void Update()
    {
        if (_machine.NavMeshAgent.remainingDistance <= _machine.NavMeshAgent.stoppingDistance)
            _machine.ChangeState(new IdleState(_machine));
    }

    public override void Exit() { }
}