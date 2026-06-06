public class InhibitorInvulnerableState : State<InhibitorStateMachine>
{
    public InhibitorInvulnerableState(InhibitorStateMachine machine) : base(machine) { }

    public override void Enter()
    {
        Machine.Health.Invulnerable = true;
    }

    public override void Update() { }
    public override void Exit() { }
}