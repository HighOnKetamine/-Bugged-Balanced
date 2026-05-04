using FishNet;
using UnityEngine;

public class MinionDeathState : State<MinionStateMachine>
{
    private const float DespawnDelay = 2f;
    private float _timer;

    public MinionDeathState(MinionStateMachine machine) : base(machine) { }

    public override void Enter()
    {
        Machine.NavMeshAgent.isStopped = true;
        Machine.NavMeshAgent.enabled = false;
        _timer = 0f;
        // TODO: gold drop event
    }

    public override void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= DespawnDelay)
            InstanceFinder.ServerManager.Despawn(Machine.NetworkObject);
    }

    public override void Exit() { }
}