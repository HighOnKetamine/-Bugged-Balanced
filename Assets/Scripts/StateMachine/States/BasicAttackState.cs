using UnityEngine;

public class BasicAttackState : State<PlayerStateMachine>
{
    public BasicAttackState(PlayerStateMachine machine) : base(machine) { }

    public override void Enter()
    {
        Machine.NavMeshAgent.ResetPath();
        Machine.NavMeshAgent.velocity = Vector3.zero;
        Machine.NetworkAnimator.SetTrigger("Attack");
    }

    public override void Update()
    {
        if (Machine.CurrentAttackTarget != null)
        {
            Vector3 direction = (Machine.CurrentAttackTarget.transform.position - Machine.transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
                Machine.transform.rotation = Quaternion.LookRotation(direction);
        }

        if (!Machine.Animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
            Machine.ChangeState(new IdleState(Machine));
    }

    public override void Exit() { }
}