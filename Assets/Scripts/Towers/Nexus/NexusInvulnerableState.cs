using UnityEngine;

public class NexusInvulnerableState : State<NexusStateMachine>
{
    public NexusInvulnerableState(NexusStateMachine machine) : base(machine) { }

    public override void Enter()
    {
        Machine.Health.Invulnerable = true;
        Debug.Log($"[NexusInvulnerableState] {Machine.gameObject.name} is invulnerable.");
    }

    public override void Update() { }

    public override void Exit() { }
}