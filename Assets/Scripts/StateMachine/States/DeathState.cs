using UnityEngine;

public class DeathState : PlayerState
{
    public DeathState(PlayerStateMachine machine) : base(machine) { }

    public override void Enter()
    {
        _machine.NavMeshAgent.ResetPath();
        _machine.NavMeshAgent.velocity = Vector3.zero;
        _machine.Animator.SetTrigger("Death");
    }

    public override void Update()
    {
        // dead, no transitions
    }

    public override void Exit()
    {
        // called on respawn
        _machine.Animator.Rebind();
    }
}