using FishNet.Object;
using FishNet.Connection;
using FishNet.Object.Synchronizing;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Server-authoritative economy manager handling gold and XP distribution.
/// Tracks player currencies and distributes rewards on enemy kills.
/// </summary>
public class EconomyManager : NetworkBehaviour
{
    public static EconomyManager Instance { get; private set; }

    [Header("Reward Settings")]
    [SerializeField] private int minionGoldReward = 20;
    [SerializeField] private int championGoldReward = 300;
    [SerializeField] private int towerGoldReward = 150;
    [SerializeField] private int neutralGoldReward = 50;
    [SerializeField] private int neutralXPReward = 100;
    [SerializeField] private int minionXPReward = 50;
    [SerializeField] private int championXPReward = 200;
    [SerializeField] private float xpDistributionRange = 15f;

    // Player gold tracking (server-authoritative)
    private Dictionary<NetworkConnection, int> playerGold = new Dictionary<NetworkConnection, int>();
    private Dictionary<NetworkConnection, int> playerXP = new Dictionary<NetworkConnection, int>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        // Subscribe to all HealthSystem death events
        HealthSystem[] allHealthSystems = FindObjectsOfType<HealthSystem>();
        foreach (HealthSystem hs in allHealthSystems)
        {
            hs.OnDeath += () => OnEntityDeath(hs);
        }
    }

    [Server]
    private void OnEntityDeath(HealthSystem deadEntity)
    {
        if (deadEntity == null) return;

        GameObject killer = deadEntity.GetLastAttacker();
        if (killer == null)
        {
            Debug.Log("[EconomyManager] Entity died with no killer");
            return;
        }

        // Determine reward amounts based on what died
        int goldReward = 0;
        int xpReward = 0;
        Vector3 deathPosition = deadEntity.transform.position;

        // Check what type of entity died
        if (deadEntity.GetComponent<MinionAI>() != null)
        {
            goldReward = minionGoldReward;
            xpReward = minionXPReward;
        }
        else if (deadEntity.GetComponent<PlayerController>() != null)
        {
            goldReward = championGoldReward;
            xpReward = championXPReward;
        }
        else if (deadEntity.GetComponent<TowerAI>() != null)
        {
            goldReward = towerGoldReward;
            xpReward = 0; // Towers don't give XP
        }

        if (goldReward == 0 && xpReward == 0) return;

        // Get killer's NetworkObject to find their connection
        NetworkObject killerNetObj = killer.GetComponent<NetworkObject>();
        if (killerNetObj != null && killerNetObj.Owner != null)
        {
            // Award gold to killer only
            AwardGold(killerNetObj.Owner, goldReward, killer.name);

            // Award XP to all allies in range
            if (xpReward > 0)
            {
                DistributeXP(deathPosition, killerNetObj.Owner, xpReward);
            }
        }
    }

    [Server]
    private void AwardGold(NetworkConnection recipient, int amount, string killerName)
    {
        if (!playerGold.ContainsKey(recipient))
        {
            playerGold[recipient] = 0;
        }

        playerGold[recipient] += amount;

        Debug.Log($"[EconomyManager] {killerName} earned {amount} gold (Total: {playerGold[recipient]})");

        // Send gold update to the specific player
        TargetUpdateGold(recipient, playerGold[recipient], amount);
    }

    [Server]
    private void DistributeXP(Vector3 deathPosition, NetworkConnection killerConnection, int totalXP)
    {
        // Find all player champions in range
        PlayerController[] allPlayers = FindObjectsOfType<PlayerController>();
        List<NetworkConnection> eligiblePlayers = new List<NetworkConnection>();

        NetworkObject killerNetObj = null;
        if (killerConnection != null)
        {
            foreach (NetworkObject netObj in ServerManager.Objects.Spawned.Values)
            {
                if (netObj.Owner == killerConnection)
                {
                    killerNetObj = netObj;
                    break;
                }
            }
        }

        TeamComponent killerTeam = killerNetObj?.GetComponent<TeamComponent>();

        foreach (PlayerController player in allPlayers)
        {
            NetworkObject playerNetObj = player.GetComponent<NetworkObject>();
            if (playerNetObj == null || playerNetObj.Owner == null) continue;

            // Check if player is in range
            float distance = Vector3.Distance(player.transform.position, deathPosition);
            if (distance <= xpDistributionRange)
            {
                // Check if player is on the same team as killer
                TeamComponent playerTeam = player.GetComponent<TeamComponent>();
                if (playerTeam != null && killerTeam != null && playerTeam.Team == killerTeam.Team)
                {
                    eligiblePlayers.Add(playerNetObj.Owner);
                }
            }
        }

        if (eligiblePlayers.Count == 0) return;

        // Distribute XP equally among eligible players
        int xpPerPlayer = totalXP / eligiblePlayers.Count;

        foreach (NetworkConnection conn in eligiblePlayers)
        {
            if (!playerXP.ContainsKey(conn))
            {
                playerXP[conn] = 0;
            }

            playerXP[conn] += xpPerPlayer;

            Debug.Log($"[EconomyManager] Player earned {xpPerPlayer} XP (Total: {playerXP[conn]})");

            // Send XP update to the specific player
            TargetUpdateXP(conn, playerXP[conn], xpPerPlayer);
        }
    }

    [TargetRpc]
    private void TargetUpdateGold(NetworkConnection conn, int totalGold, int goldGained)
    {
        Debug.Log($"[Client] Received {goldGained} gold! Total: {totalGold}");
        // UI should listen to this via events or update directly
        // For now, just log it
    }

    [TargetRpc]
    private void TargetUpdateXP(NetworkConnection conn, int totalXP, int xpGained)
    {
        Debug.Log($"[Client] Received {xpGained} XP! Total: {totalXP}");
        // UI should listen to this via events or update directly
    }

    [Server]
    public void SpendGold(NetworkConnection conn, int amount)
    {
        if (!playerGold.ContainsKey(conn)) return;

        playerGold[conn] -= amount;
        if (playerGold[conn] < 0) playerGold[conn] = 0;

        Debug.Log($"[EconomyManager] {conn} spent {amount} gold (Remaining: {playerGold[conn]})");

        // Send gold update to the specific player
        TargetUpdateGold(conn, playerGold[conn], -amount);
    }

    [Server]
    public int GetPlayerGold(NetworkConnection conn)
    {
        return playerGold.ContainsKey(conn) ? playerGold[conn] : 0;
    }

    [Server]
    public int GetPlayerXP(NetworkConnection conn)
    {
        return playerXP.ContainsKey(conn) ? playerXP[conn] : 0;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}

