using FishNet.Object;
using UnityEngine;
using System.Collections;

/// <summary>
/// Server-authoritative respawn manager.
/// Handles player death state and respawning at team spawn points.
/// </summary>
public class RespawnManager : NetworkBehaviour
{
    public static RespawnManager Instance { get; private set; }

    [Header("Respawn Settings")]
    [SerializeField] private float respawnDelay = 10f;
    [SerializeField] private float respawnInvulnerabilityDuration = 3f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    [Header("Champion Detection")]
    [SerializeField] private LayerMask championLayer;

    public override void OnStartServer()
    {
        base.OnStartServer();

        // Subscribe to all player/champion death events
        HealthSystem[] allHealthSystems = FindObjectsOfType<HealthSystem>();
        foreach (HealthSystem hs in allHealthSystems)
        {
            // Check if this is a champion by layer
            if (((1 << hs.gameObject.layer) & championLayer) != 0)
            {
                hs.OnDeath += () => OnPlayerDeath(hs.gameObject);
            }
        }
    }

    [Server]
    private void OnPlayerDeath(GameObject player)
    {
        if (player == null) return;

        Debug.Log($"[RespawnManager] Player {player.name} died. Starting respawn timer...");

        // Disable player immediately
        DisablePlayer(player);

        // Start respawn coroutine
        StartCoroutine(RespawnCoroutine(player));
    }

    [Server]
    private void DisablePlayer(GameObject player)
    {
        // Disable movement via PlayerController if it exists
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.SetInputEnabled(false);
        }

        var navAgent = player.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (navAgent != null)
        {
            navAgent.ResetPath();
            navAgent.enabled = false;
        }

        // Disable visuals on all clients
        RpcDisablePlayerVisuals(player.GetComponent<NetworkObject>());
    }

    [ObserversRpc]
    private void RpcDisablePlayerVisuals(NetworkObject playerNetObj)
    {
        if (playerNetObj == null) return;

        // Disable renderers
        Renderer[] renderers = playerNetObj.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.enabled = false;
        }

        Debug.Log($"[Client] Disabled visuals for {playerNetObj.gameObject.name}");
    }

    [Server]
    private IEnumerator RespawnCoroutine(GameObject player)
    {
        yield return new WaitForSeconds(respawnDelay);

        if (player == null)
        {
            Debug.LogWarning("[RespawnManager] Player object was destroyed before respawn!");
            yield break;
        }

        RespawnPlayer(player);
    }

    [Server]
    private void RespawnPlayer(GameObject player)
    {
        Debug.Log($"[RespawnManager] Respawning {player.name}");

        // Get player's team
        TeamComponent playerTeam = player.GetComponent<TeamComponent>();
        if (playerTeam == null)
        {
            Debug.LogError("[RespawnManager] Player has no TeamComponent!");
            return;
        }

        // Find spawn point for player's team
        SpawnPoint spawnPoint = FindSpawnPointForTeam(playerTeam.Team);
        if (spawnPoint == null)
        {
            Debug.LogError($"[RespawnManager] No spawn point found for team {playerTeam.Team}!");
            return;
        }

        // Teleport player to spawn point
        player.transform.position = spawnPoint.transform.position;
        player.transform.rotation = spawnPoint.transform.rotation;

        // Reset health and mana
        HealthSystem health = player.GetComponent<HealthSystem>();
        if (health != null)
        {
            health.Heal(health.MaxHealth); // Full heal
        }

        // Re-enable movement
        var navAgent = player.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (navAgent != null)
        {
            navAgent.enabled = true;
            navAgent.Warp(spawnPoint.transform.position);
        }

        // Re-enable input if player has PlayerController
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.SetInputEnabled(true);
        }

        // Re-enable visuals on all clients
        RpcEnablePlayerVisuals(player.GetComponent<NetworkObject>());

        Debug.Log($"[RespawnManager] {player.name} respawned at {spawnPoint.transform.position}");
    }

    [ObserversRpc]
    private void RpcEnablePlayerVisuals(NetworkObject playerNetObj)
    {
        if (playerNetObj == null) return;

        // Enable renderers
        Renderer[] renderers = playerNetObj.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.enabled = true;
        }

        Debug.Log($"[Client] Enabled visuals for {playerNetObj.gameObject.name}");
    }

    [Server]
    private SpawnPoint FindSpawnPointForTeam(TeamId team)
    {
        SpawnPoint[] allSpawnPoints = FindObjectsOfType<SpawnPoint>();
        
        foreach (SpawnPoint sp in allSpawnPoints)
        {
            if (sp.Team == team)
            {
                return sp;
            }
        }

        return null;
    }

    /// <summary>
    /// Call this when a new player/champion spawns to subscribe to their death event
    /// </summary>
    [Server]
    public void RegisterPlayer(GameObject player)
    {
        HealthSystem health = player.GetComponent<HealthSystem>();
        if (health != null)
        {
            health.OnDeath += () => OnPlayerDeath(player);
            Debug.Log($"[RespawnManager] Registered player {player.name} for respawn tracking");
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
