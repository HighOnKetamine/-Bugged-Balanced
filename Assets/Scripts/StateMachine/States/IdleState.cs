using UnityEngine;

public class IdleState : State<PlayerStateMachine>
{
    public IdleState(PlayerStateMachine machine) : base(machine) { }

    public override void Enter()
    {
        Debug.Log("[IdleState] Enter");
        Machine.Animator.SetBool("IsMoving", false);
    }

    public override void Update()
    {
        if (Machine.NavMeshAgent.pathPending) return;
        if (Machine.NavMeshAgent.remainingDistance > Machine.NavMeshAgent.stoppingDistance)
            Machine.ChangeState(new RunState(Machine));
    }

    public override void Exit() { }
}