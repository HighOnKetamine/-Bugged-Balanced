using FishNet;
using UnityEngine;

public class MinionDeathState : State<MinionStateMachine>
{
    private const float DespawnDelay = 2f;
    private float _timer;
    private readonly GameObject _killer;

    public MinionDeathState(MinionStateMachine machine, GameObject killer) : base(machine)
    {
        _killer = killer;
    }

    public override void Enter()
    {
        Machine.NavMeshAgent.enabled = false;
        Machine.NavMeshObstacle.enabled = false; // dead minion shouldn't block pathing
        _timer = 0f;

        AwardGold();
    }

    public override void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= DespawnDelay)
            InstanceFinder.ServerManager.Despawn(Machine.NetworkObject);
    }

    public override void Exit() { }

    private void AwardGold()
    {
        if (_killer == null) return;

        GoldComponent gold = _killer.GetComponent<GoldComponent>();
        if (gold == null)
        {
            Debug.LogWarning($"[MinionDeathState] Killer {_killer.name} has no GoldComponent.");
            return;
        }

        gold.Award(Machine.Stats.goldReward);
    }
}