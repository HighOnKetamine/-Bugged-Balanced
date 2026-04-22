using UnityEngine;

public class DeathState : State<PlayerStateMachine>
{
    public DeathState(PlayerStateMachine machine) : base(machine) { }

    public override void Enter()
    {
        Debug.Log("[DeathState] Enter");
        Machine.NavMeshAgent.ResetPath();
        Machine.NavMeshAgent.velocity = Vector3.zero;
        Machine.CanAttack = false;
        Machine.CanCast = false;
        Machine.CanMove = false;
        Machine.Animator.SetTrigger("Death");
    }

    public override void Update() { }

    public override void Exit()
    {
        Machine.Animator.Rebind();
        Machine.CanAttack = true;
        Machine.CanCast = true;
        Machine.CanMove = true;
    }
}