using System;
using FishNet;
using UnityEngine;

public class TowerDeathState : State<TowerStateMachine>
{
    // Nexus system subscribes to this — fired on the server, picked up wherever needed.
    // Next milestone: NexusManager.OnTowerDied += HandleTowerDied;
    public static event Action<TowerStateMachine> OnTowerDied;

    private const float DespawnDelay = 3f;
    private float _timer;
    private readonly GameObject _killer;

    public TowerDeathState(TowerStateMachine machine, GameObject killer) : base(machine)
    {
        _killer = killer;
    }

    public override void Enter()
    {
        _timer = 0f;
        AwardGold();
        OnTowerDied?.Invoke(Machine);
    }

    public override void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= DespawnDelay)
        {
            if (InstanceFinder.ServerManager != null && InstanceFinder.ServerManager.Started && Machine.NetworkObject != null)
            {
                InstanceFinder.ServerManager.Despawn(Machine.NetworkObject);
            }
            else
            {
                Debug.LogWarning("[TowerDeathState] ServerManager not available or NetworkObject null; performing local destroy.");
                if (Machine.NetworkObject != null)
                    Object.Destroy(Machine.NetworkObject.gameObject);
                else
                    Object.Destroy(Machine.gameObject);
            }
        }
    }

    public override void Exit() { }

    private void AwardGold()
    {
        if (_killer == null) return;

        GoldComponent gold = _killer.GetComponent<GoldComponent>();
        if (gold == null)
        {
            Debug.LogWarning($"[TowerDeathState] Killer {_killer.name} has no GoldComponent.");
            return;
        }

        gold.Award(Machine.Stats.goldReward);
    }
}