using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("Shop Catalog")]
    [SerializeField] private ItemData[] shopItems;

    private Dictionary<string, ItemData> _itemLookup;

    private void Awake()
    {
        if (Instance != null && Instance != this) return;
        Instance = this;
        BuildLookup();
    }

    private void BuildLookup()
    {
        _itemLookup = new Dictionary<string, ItemData>();
        if (shopItems == null) return;
        foreach (ItemData item in shopItems)
        {
            if (item == null) continue;
            string id = item.GetId();
            if (string.IsNullOrWhiteSpace(id)) continue;
            if (_itemLookup.ContainsKey(id))
            {
                Debug.LogWarning($"[ShopManager] Duplicate item ID: {id}");
                continue;
            }
            _itemLookup[id] = item;
        }
    }

    public ItemData GetItem(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId) || _itemLookup == null) return null;
        _itemLookup.TryGetValue(itemId, out ItemData item);
        return item;
    }

    public ItemData[] GetAvailableItems() => shopItems;

    // Called from ShopComponent.ServerRequestPurchase — already on server
    public bool TryPurchaseItem(GameObject buyer, string itemId, out string reason)
    {
        reason = null;
        if (buyer == null) { reason = "Buyer is null."; return false; }

        GoldComponent gold = buyer.GetComponent<GoldComponent>();
        InventoryComponent inventory = buyer.GetComponent<InventoryComponent>();

        if (gold == null) { reason = "No GoldComponent."; return false; }
        if (inventory == null) { reason = "No InventoryComponent."; return false; }

        ItemData item = GetItem(itemId);
        if (item == null) { reason = "Item not found."; return false; }
        if (!item.allowMultiple && inventory.HasItem(itemId)) { reason = "Already owned."; return false; }
        if (!inventory.CanAddItem(item)) { reason = "Inventory full."; return false; }
        if (gold.Gold.Value < item.cost) { reason = "Not enough gold."; return false; }

        if (!inventory.AddItem(item)) { reason = "Could not add item."; return false; }
        gold.Spend(item.cost);

        Debug.Log($"[ShopManager] {buyer.name} purchased {item.itemName} for {item.cost} gold.");
        return true;
    }
}