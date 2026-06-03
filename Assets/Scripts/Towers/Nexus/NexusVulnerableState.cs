using UnityEngine;

public class NexusVulnerableState : State<NexusStateMachine>
{
    public NexusVulnerableState(NexusStateMachine machine) : base(machine) { }

    public override void Enter()
    {
        Machine.Health.Invulnerable = false;
        Debug.Log($"[NexusVulnerableState] {Machine.gameObject.name} is now vulnerable!");
    }

    public override void Update() { }

    public override void Exit() { }
}