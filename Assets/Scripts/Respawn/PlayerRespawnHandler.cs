using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.AI;

public class PlayerRespawnHandler : NetworkBehaviour
{
    [Header("References — assign in Inspector")]
    [SerializeField] private HealthComponent health;
    [SerializeField] private ManaComponent mana;
    [SerializeField] private PlayerStateMachine stateMachine;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private GameObject[] visualObjects;

    // FIX A: SyncVar<T> jako field, ne [SyncVar] na property
    public readonly SyncVar<float> RespawnTimeRemaining = new SyncVar<float>();

    [Server]
    public void OnDeathRegistered(float duration)
    {
        RespawnTimeRemaining.Value = duration;   // FIX A: .Value
        HideVisualsObservers();
    }

    [Server]
    public void Respawn(Vector3 spawnPosition)
    {
        health.ResetToFull();
        mana.ResetToFull();

        agent.Warp(spawnPosition);

        // FIX C: ChangeState bere instanci State<T>, ne type argument
        stateMachine.ChangeState(new IdleState(stateMachine));

        RespawnTimeRemaining.Value = 0f;         // FIX A: .Value
        ShowVisualsObservers();
    }

    [ObserversRpc]
    private void HideVisualsObservers()
    {
        foreach (var obj in visualObjects)
            obj.SetActive(false);
    }

    [ObserversRpc]
    private void ShowVisualsObservers()
    {
        foreach (var obj in visualObjects)
            obj.SetActive(true);
    }
}