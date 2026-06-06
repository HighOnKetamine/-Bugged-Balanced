using System;
using System.Collections;
using FishNet;
using UnityEngine;

public class InhibitorDeathState : State<InhibitorStateMachine>
{
    public static event Action<InhibitorStateMachine> OnInhibitorDied;
    public static event Action<InhibitorStateMachine> OnInhibitorRespawned;

    private readonly GameObject _killer;
    private float _timer;

    public InhibitorDeathState(InhibitorStateMachine machine, GameObject killer) : base(machine)
    {
        _killer = killer;
    }

    public override void Enter()
    {
        _timer = 0f;
        AwardGold();
        OnInhibitorDied?.Invoke(Machine);
        Machine.Health.Invulnerable = true; // can't damage a dead inhibitor
        Machine.StartCoroutine(RespawnRoutine());
    }

    public override void Update() { }
    public override void Exit() { }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(Machine.respawnTime);
        OnInhibitorRespawned?.Invoke(Machine);
        Machine.Respawn();
    }

    private void AwardGold()
    {
        if (_killer == null) return;
        GoldComponent gold = _killer.GetComponent<GoldComponent>();
        if (gold == null) return;
        gold.Award(Machine.Stats.goldReward);
    }
}