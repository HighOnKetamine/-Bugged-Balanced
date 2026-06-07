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
        // Rotate toward target
        if (Machine.CurrentAttackTarget != null)
        {
            Vector3 direction = (Machine.CurrentAttackTarget.transform.position - Machine.transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
                Machine.transform.rotation = Quaternion.LookRotation(direction);
        }

        // Target died or gone
        if (Machine.CurrentAttackTarget == null ||
            Machine.CurrentAttackTarget.GetComponent<HealthComponent>()?.IsDead == true)
        {
            Machine.CurrentAttackTarget = null;
            Machine.ChangeState(new IdleState(Machine));
            return;
        }

        // Out of range — chase
        if (!Machine.BasicAttack.IsInRange(Machine.CurrentAttackTarget))
        {
            Machine.NavMeshAgent.SetDestination(Machine.CurrentAttackTarget.transform.position);
            return;
        }

        // Stop moving and attack
        Machine.NavMeshAgent.ResetPath();
        Machine.NavMeshAgent.velocity = Vector3.zero;

        if (Machine.BasicAttack.IsOffCooldown())
        {
            Machine.NetworkAnimator.SetTrigger("Attack");
            Machine.BasicAttack.Attack(Machine.CurrentAttackTarget);
        }
    }

    public override void Exit() { }
}