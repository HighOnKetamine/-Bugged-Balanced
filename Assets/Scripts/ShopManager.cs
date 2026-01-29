using FishNet.Object;
using FishNet.Connection;
using UnityEngine;
using System.Collections.Generic;

public class ShopManager : NetworkBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("Shop Items")]
    [SerializeField] private Item[] availableItems;

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
    public bool TryPurchaseItem(NetworkConnection conn, Item item)
    {
        if (item == null) return false;

        int playerGold = EconomyManager.Instance.GetPlayerGold(conn);
        if (playerGold < item.goldCost) return false;

        // Deduct gold
        EconomyManager.Instance.SpendGold(conn, item.goldCost);

        // Add item to inventory
        InventoryManager.Instance.AddItemToInventory(conn, item);

        // Notify client of successful purchase
        TargetPurchaseSuccessful(conn, item);

        return true;
    }

    [TargetRpc]
    private void TargetPurchaseSuccessful(NetworkConnection conn, Item item)
    {
        Debug.Log($"[Client] Successfully purchased {item.itemName}");
        // UI can listen to this or update directly
    }

    public Item[] GetAvailableItems()
    {
        return availableItems;
    }
}