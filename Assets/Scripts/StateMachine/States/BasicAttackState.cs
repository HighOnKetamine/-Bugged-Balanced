using UnityEngine;

public class BasicAttackState : PlayerState
{
    public BasicAttackState(PlayerStateMachine machine) : base(machine) { }

    public override void Enter()
    {
        _machine.NavMeshAgent.ResetPath();
        _machine.NavMeshAgent.velocity = Vector3.zero;

        // rotate toward target
        Vector3 direction = (_machine.CurrentAttackTarget.transform.position - _machine.transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
            _machine.transform.rotation = Quaternion.LookRotation(direction);

        _machine.NetworkAnimator.SetTrigger("Attack");
    }

    public override void Update()
    {
        if (!_machine.Animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
            _machine.ChangeState(new IdleState(_machine));
    }

    public override void Exit() { }
}