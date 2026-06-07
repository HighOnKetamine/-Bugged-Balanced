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
        var agent = Machine.NavMeshAgent;
        if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh) return;

        if (agent.pathPending) return;
        if (agent.remainingDistance > agent.stoppingDistance)
            Machine.ChangeState(new RunState(Machine));
    }

    public override void Exit() { }
}