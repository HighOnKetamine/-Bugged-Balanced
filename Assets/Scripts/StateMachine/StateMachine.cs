using FishNet.Object;

public abstract class StateMachine<T> : NetworkBehaviour where T : class
{
    private State<T> _currentState;

    protected virtual void Update()
    {
        if (!IsServerInitialized) return;

        _currentState?.Update();
    }

    public void ChangeState(State<T> newState)
    {
        if (_currentState?.GetType() == newState.GetType()) return;
        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter();
    }
}