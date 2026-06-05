using FishNet;
using UnityEngine;

public class MinionDeathState : State<MinionStateMachine>
{
    private const float DespawnDelay = 2f;
    private float _timer;

    public MinionDeathState(MinionStateMachine machine) : base(machine) { }

    public override void Enter()
    {
        Machine.NavMeshAgent.enabled = false;
        Machine.NavMeshObstacle.enabled = false;
        _timer = 0f;
    }

    public override void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= DespawnDelay)
        {
            // Only attempt to despawn via the ServerManager when the server is active
            // and the NetworkObject is valid. Otherwise, safely destroy the object
            // locally to avoid null-reference exceptions.
            if (InstanceFinder.ServerManager != null && InstanceFinder.ServerManager.Started && Machine.NetworkObject != null)
            {
                InstanceFinder.ServerManager.Despawn(Machine.NetworkObject);
            }
            else
            {
                Debug.LogWarning("[MinionDeathState] ServerManager not available or NetworkObject null; performing local destroy.");
                if (Machine.NetworkObject != null)
                    Object.Destroy(Machine.NetworkObject.gameObject);
                else
                    Object.Destroy(Machine.gameObject);
            }
        }
    }

    public override void Exit() { }
}