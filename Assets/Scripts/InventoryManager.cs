using FishNet.Object;
using FishNet.Connection;
using FishNet.Object.Synchronizing;
using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : NetworkBehaviour
{
    public static InventoryManager Instance { get; private set; }

    // Server-side inventory tracking
    private Dictionary<NetworkConnection, List<Item>> playerInventories = new Dictionary<NetworkConnection, List<Item>>();
    private Dictionary<NetworkConnection, List<Item>> playerEquippedItems = new Dictionary<NetworkConnection, List<Item>>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    [Server]
    public void AddItemToInventory(NetworkConnection conn, Item item)
    {
        if (!playerInventories.ContainsKey(conn))
        {
            playerInventories[conn] = new List<Item>();
        }

        playerInventories[conn].Add(item);

        Debug.Log($"[InventoryManager] Added {item.itemName} to {conn}'s inventory");

        // Notify client
        TargetItemAdded(conn, item);
    }

    [Server]
    public bool EquipItem(NetworkConnection conn, Item item)
    {
        if (!playerInventories.ContainsKey(conn) || !playerInventories[conn].Contains(item))
            return false;

        if (!playerEquippedItems.ContainsKey(conn))
        {
            playerEquippedItems[conn] = new List<Item>();
        }

        // Check if item is already equipped
        if (playerEquippedItems[conn].Contains(item)) return true;

        playerEquippedItems[conn].Add(item);

        // Apply item bonuses to champion
        PlayerController player = GetPlayerController(conn);
        if (player != null)
        {
            ChampionStats stats = player.GetComponent<ChampionStats>();
            if (stats != null)
            {
                stats.AddItemBonuses(item);
            }
        }

        Debug.Log($"[InventoryManager] {conn} equipped {item.itemName}");

        TargetItemEquipped(conn, item);
        return true;
    }

    [Server]
    public bool UnequipItem(NetworkConnection conn, Item item)
    {
        if (!playerEquippedItems.ContainsKey(conn) || !playerEquippedItems[conn].Contains(item))
            return false;

        playerEquippedItems[conn].Remove(item);

        // Remove item bonuses from champion
        PlayerController player = GetPlayerController(conn);
        if (player != null)
        {
            ChampionStats stats = player.GetComponent<ChampionStats>();
            if (stats != null)
            {
                stats.RemoveItemBonuses(item);
            }
        }

        Debug.Log($"[InventoryManager] {conn} unequipped {item.itemName}");

        TargetItemUnequipped(conn, item);
        return true;
    }

    [Server]
    public bool TryUpgradeItem(NetworkConnection conn, Item baseItem, Item upgradeItem)
    {
        if (!playerInventories.ContainsKey(conn)) return false;

        // Check if player has required items
        if (!upgradeItem.CanBuildWith(playerInventories[conn].ToArray())) return false;

        // Remove required items
        foreach (Item required in upgradeItem.requiredItems)
        {
            playerInventories[conn].Remove(required);
        }

        // Add upgraded item
        playerInventories[conn].Add(upgradeItem);

        Debug.Log($"[InventoryManager] {conn} upgraded to {upgradeItem.itemName}");

        TargetItemUpgraded(conn, baseItem, upgradeItem);
        return true;
    }

    private PlayerController GetPlayerController(NetworkConnection conn)
    {
        foreach (NetworkObject netObj in ServerManager.Objects.Spawned.Values)
        {
            if (netObj.Owner == conn)
            {
                return netObj.GetComponent<PlayerController>();
            }
        }
        return null;
    }

    // Client RPCs
    [TargetRpc]
    private void TargetItemAdded(NetworkConnection conn, Item item)
    {
        Debug.Log($"[Client] Item added to inventory: {item.itemName}");
    }

    [TargetRpc]
    private void TargetItemEquipped(NetworkConnection conn, Item item)
    {
        Debug.Log($"[Client] Item equipped: {item.itemName}");
    }

    [TargetRpc]
    private void TargetItemUnequipped(NetworkConnection conn, Item item)
    {
        Debug.Log($"[Client] Item unequipped: {item.itemName}");
    }

    [TargetRpc]
    private void TargetItemUpgraded(NetworkConnection conn, Item oldItem, Item newItem)
    {
        Debug.Log($"[Client] Item upgraded from {oldItem.itemName} to {newItem.itemName}");
    }

    // Public getters for UI
    public List<Item> GetPlayerInventory(NetworkConnection conn)
    {
        return playerInventories.ContainsKey(conn) ? new List<Item>(playerInventories[conn]) : new List<Item>();
    }

    public List<Item> GetPlayerEquippedItems(NetworkConnection conn)
    {
        return playerEquippedItems.ContainsKey(conn) ? new List<Item>(playerEquippedItems[conn]) : new List<Item>();
    }
}