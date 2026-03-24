using UnityEngine;

public class StunnedState : PlayerState
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
        _machine.NavMeshAgent.ResetPath();
        _machine.NavMeshAgent.velocity = Vector3.zero;
        _machine.Animator.SetBool("IsStunned", true);
    }

    public override void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= _duration)
            _machine.ChangeState(new IdleState(_machine));
    }

    public override void Exit()
    {
        _machine.Animator.SetBool("IsStunned", false);
    }
}