using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using System;

public enum MatchState
{
    Waiting,
    InProgress,
    Victory,
    Defeat
}

/// <summary>
/// Server-authoritative game state manager that tracks match state and win/loss conditions.
/// Singleton pattern for easy access across the game.
/// </summary>
public class GameStateManager : NetworkBehaviour
{
    public static GameStateManager Instance { get; private set; }

    [Header("Match State")]
    [SerializeField]
    private readonly SyncVar<MatchState> currentState = new SyncVar<MatchState>(new SyncTypeSettings(WritePermission.ServerOnly, ReadPermission.Observers));

    [Header("Base/Nexus References")]
    [SerializeField] private HealthSystem blueBaseHealth;
    [SerializeField] private HealthSystem redBaseHealth;

    public MatchState CurrentState => currentState.Value;
    public event Action<MatchState> OnMatchStateChanged;
    public event Action<TeamId> OnVictory;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        currentState.OnChange += OnStateChanged;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        
        // Subscribe to base health events
        if (blueBaseHealth != null)
        {
            blueBaseHealth.OnDeath += () => OnBaseDestroyed(TeamId.Blue);
        }
        
        if (redBaseHealth != null)
        {
            redBaseHealth.OnDeath += () => OnBaseDestroyed(TeamId.Red);
        }

        // Start the match
        currentState.Value = MatchState.InProgress;
    }

    [Server]
    private void OnBaseDestroyed(TeamId destroyedTeam)
    {
        if (currentState.Value != MatchState.InProgress) return;

        Debug.Log($"[GameStateManager] {destroyedTeam} base destroyed!");

        // The team that destroyed the base wins
        TeamId winningTeam = destroyedTeam == TeamId.Blue ? TeamId.Red : TeamId.Blue;
        
        EndMatch(winningTeam);
    }

    [Server]
    public void EndMatch(TeamId winningTeam)
    {
        if (currentState.Value != MatchState.InProgress) return;

        Debug.Log($"[GameStateManager] Match ended! Winner: {winningTeam}");
        
        // Broadcast victory to all clients
        RpcAnnounceVictory(winningTeam);
        
        currentState.Value = MatchState.Victory;
        OnVictory?.Invoke(winningTeam);
    }

    [ObserversRpc]
    private void RpcAnnounceVictory(TeamId winningTeam)
    {
        Debug.Log($"[Client] {winningTeam} team won the match!");
        OnVictory?.Invoke(winningTeam);
        
        // Disable all player input
        DisableAllPlayerInput();
    }

    /// <summary>
    /// Disables input for all PlayerController instances in the scene.
    /// Called on all clients when the match ends.
    /// </summary>
    private void DisableAllPlayerInput()
    {
        PlayerController[] players = FindObjectsOfType<PlayerController>();
        foreach (PlayerController player in players)
        {
            player.SetInputEnabled(false);
        }
        Debug.Log($"[GameStateManager] Disabled input for {players.Length} players");
    }

    private void OnStateChanged(MatchState oldState, MatchState newState, bool asServer)
    {
        OnMatchStateChanged?.Invoke(newState);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
