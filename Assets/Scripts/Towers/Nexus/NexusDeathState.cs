// NexusDeathState.cs
using UnityEngine;

public class NexusDeathState : State<NexusStateMachine>
{
    public NexusDeathState(NexusStateMachine machine) : base(machine) { }

    public override void Enter()
    {
        sbyte winningTeam = Machine.Team.teamId.Value == 0 ? (sbyte)1 : (sbyte)0;
        Debug.Log($"[NexusDeathState] Nexus destroyed! Team {winningTeam} wins.");
        NetworkGameManager.Instance.TriggerGameOver(winningTeam);
    }

    public override void Update() { }

    public override void Exit() { }
}