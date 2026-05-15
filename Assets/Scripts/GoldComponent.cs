using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class GoldComponent : NetworkBehaviour
{
    public readonly SyncVar<int> Gold = new SyncVar<int>();

    [Server]
    public void Award(int amount)
    {
        Gold.Value += amount;
        Debug.Log($"[GoldComponent] {gameObject.name} received {amount} gold. Total: {Gold.Value}");
    }
}