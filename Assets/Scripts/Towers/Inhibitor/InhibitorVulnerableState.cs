public class InhibitorVulnerableState : State<InhibitorStateMachine>
{
    public InhibitorVulnerableState(InhibitorStateMachine machine) : base(machine) { }

    public override void Enter()
    {
        Machine.Health.Invulnerable = false;
    }

    public override void Update() { }
    public override void Exit() { }
}