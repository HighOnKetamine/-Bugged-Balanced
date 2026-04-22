using UnityEngine;

public class StunnedState : State<PlayerStateMachine>
{
    private float _duration;
    private float _timer;

    public StunnedState(PlayerStateMachine machine, float duration) : base(machine)
    {
        _duration = duration;
    }

    public override void Enter()
    {
        _timer = 0f;
        Machine.NavMeshAgent.ResetPath();
        Machine.NavMeshAgent.velocity = Vector3.zero;
        Machine.Animator.SetBool("IsStunned", true);
    }

    public override void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= _duration)
            Machine.ChangeState(new IdleState(Machine));
    }

    public override void Exit()
    {
        Machine.Animator.SetBool("IsStunned", false);
    }
}