using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class PlayerScoreComponent : NetworkBehaviour
{
    public readonly SyncVar<int> Kills = new SyncVar<int>(0);
    public readonly SyncVar<int> Deaths = new SyncVar<int>(0);

    public event Action<int, int> OnScoreChanged;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        Kills.OnChange += HandleScoreChanged;
        Deaths.OnChange += HandleScoreChanged;
    }

    public void AwardKill()
    {
        Kills.Value++;
        Debug.Log($"[PlayerScoreComponent] {gameObject.name} kills={Kills.Value}");
    }

    public void AwardDeath()
    {
        Deaths.Value++;
        Debug.Log($"[PlayerScoreComponent] {gameObject.name} deaths={Deaths.Value}");
    }

    private void HandleScoreChanged(int oldValue, int newValue, bool asServer)
    {
        OnScoreChanged?.Invoke(Kills.Value, Deaths.Value);
    }
}