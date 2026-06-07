using System;
using System.Collections;
using FishNet;
using FishNet.Connection;
using FishNet.Component.Transforming;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using UnityEngine;

public class NetworkGameManager : NetworkBehaviour
{
    public static NetworkGameManager Instance { get; private set; }

    [Header("Characters")]
    public CharacterDefinition[] characters; // assign CharacterDefinition assets in inspector

    [Header("Settings")]
    [SerializeField] private int minPlayersToStart = 1;

    public readonly SyncVar<GameState> State = new SyncVar<GameState>();
    public readonly SyncList<LobbyPlayerData> Players = new SyncList<LobbyPlayerData>();

    // Nexus fires TriggerGameOver → this broadcasts the winning team to all clients
    public static event Action<sbyte> OnGameOver;
    // LobbyUI subscribes to hide the lobby panel when the game starts
    public static event Action OnGameStarted;

    private RespawnManager _respawnManager;
    private WaveSpawner[] _waveSpawners;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        _respawnManager = FindFirstObjectByType<RespawnManager>();
        _waveSpawners = FindObjectsByType<WaveSpawner>(FindObjectsSortMode.None);
        ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
    }

    // Fires on every client (including host) when this scene object is ready on the network.
    // This is the entry point — every client announces themselves to the server.
    public override void OnStartClient()
    {
        base.OnStartClient();
        ServerJoinLobby(LocalPlayerInfo.Name);
    }

    private void OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        if (args.ConnectionState == RemoteConnectionState.Stopped)
            RemovePlayer(conn.ClientId);
    }

    // --- Server RPCs ---

    [ServerRpc(RequireOwnership = false)]
    public void ServerJoinLobby(string playerName, NetworkConnection conn = null)
    {
        if (State.Value != GameState.Lobby) return;
        RemovePlayer(conn.ClientId); // clear stale entry if reconnecting

        Players.Add(new LobbyPlayerData
        {
            ClientId = conn.ClientId,
            PlayerName = playerName,
            TeamId = AssignTeam(),
            CharacterIndex = 0,
            IsReady = false
        });
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerSetReady(bool ready, NetworkConnection conn = null)
    {
        UpdatePlayer(conn.ClientId, p => { p.IsReady = ready; return p; });
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerSetCharacter(int index, NetworkConnection conn = null)
    {
        if (index < 0 || index >= characters.Length) return;
        UpdatePlayer(conn.ClientId, p => { p.CharacterIndex = index; return p; });
    }

    // Only the first player (the host) can start the game.
    [ServerRpc(RequireOwnership = false)]
    public void ServerRequestStartGame(NetworkConnection conn = null)
    {
        if (Players.Count == 0 || Players[0].ClientId != conn.ClientId) return;
        if (State.Value != GameState.Lobby) return;

        int readyCount = 0;
        foreach (var p in Players) if (p.IsReady) readyCount++;
        if (readyCount < minPlayersToStart)
        {
            Debug.Log($"[NetworkGameManager] Not enough ready players ({readyCount}/{minPlayersToStart}).");
            return;
        }

        StartCoroutine(StartGameRoutine());
    }

    // Called by NexusDeathState (next milestone) to end the game.
    [Server]
    public void TriggerGameOver(sbyte winningTeam)
    {
        if (State.Value != GameState.InGame) return;
        State.Value = GameState.GameOver;
        RpcGameOver(winningTeam);
    }

    // --- Private server logic ---

    private IEnumerator StartGameRoutine()
    {
        State.Value = GameState.InGame;
        RpcOnGameStarted();

        // Spawn one player per frame to avoid a spike
        foreach (var data in Players)
        {
            if (InstanceFinder.ServerManager.Clients.TryGetValue(data.ClientId, out NetworkConnection conn))
                SpawnPlayer(conn, data);
            yield return null;
        }

        foreach (var spawner in _waveSpawners)
            spawner.StartWaves();
    }

    [Server]
    private void SpawnPlayer(NetworkConnection conn, LobbyPlayerData data)
    {
        int charIndex = Mathf.Clamp(data.CharacterIndex, 0, characters.Length - 1);
        CharacterDefinition def = characters[charIndex];
        Vector3 pos = _respawnManager.GetSpawnPoint(data.TeamId);

        NetworkObject nob = Instantiate(def.playerPrefab, pos, Quaternion.identity);
        if (nob.GetComponent<NetworkTransform>() == null)
            nob.gameObject.AddComponent<NetworkTransform>();

        ServerManager.Spawn(nob, conn); // gives ownership to this client
        nob.GetComponent<TeamComponent>()?.SetTeam(data.TeamId);
    }

    [ObserversRpc]
    private void RpcGameOver(sbyte winningTeam) => OnGameOver?.Invoke(winningTeam);

    [ObserversRpc]
    private void RpcOnGameStarted() => OnGameStarted?.Invoke();

    // --- Helpers ---

    // Balances teams: whoever has fewer players gets the next one.
    private sbyte AssignTeam()
    {
        int blue = 0, red = 0;
        foreach (var p in Players)
        {
            if (p.TeamId == 0) blue++;
            else if (p.TeamId == 1) red++;
        }
        return (sbyte)(blue <= red ? 0 : 1);
    }

    private void RemovePlayer(int clientId)
    {
        for (int i = 0; i < Players.Count; i++)
        {
            if (Players[i].ClientId != clientId) continue;
            Players.RemoveAt(i);
            return;
        }
    }

    private void UpdatePlayer(int clientId, Func<LobbyPlayerData, LobbyPlayerData> update)
    {
        for (int i = 0; i < Players.Count; i++)
        {
            if (Players[i].ClientId != clientId) continue;
            Players[i] = update(Players[i]);
            return;
        }
    }
}