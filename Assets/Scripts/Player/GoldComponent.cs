using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class GoldComponent : NetworkBehaviour
{
    public readonly SyncVar<int> Gold = new SyncVar<int>(0);
    public event Action<int> OnGoldChanged;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        Gold.OnChange += HandleGoldChanged;
    }

    [Server]
    public void Award(int amount)
    {
        Gold.Value += amount;
        Debug.Log($"[GoldComponent] {gameObject.name} received {amount} gold. Total: {Gold.Value}");
    }

    [Server]
    public bool Spend(int amount)
    {
        if (amount <= 0) return false;
        if (Gold.Value < amount) return false;

        Gold.Value -= amount;
        Debug.Log($"[GoldComponent] {gameObject.name} spent {amount} gold. Total: {Gold.Value}");
        return true;
    }

    private void HandleGoldChanged(int oldValue, int newValue, bool asServer)
    {
        OnGoldChanged?.Invoke(newValue);
    }
}