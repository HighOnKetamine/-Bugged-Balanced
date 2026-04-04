public abstract class PlayerState
{
    protected PlayerStateMachine _machine;

    public PlayerState(PlayerStateMachine machine)
    {
        _machine = machine;
    }

    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}