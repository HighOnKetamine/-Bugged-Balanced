public abstract class State<T> where T : class
{
    protected T Machine { get; private set; }

    protected State(T machine)
    {
        Machine = machine;
    }

    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}