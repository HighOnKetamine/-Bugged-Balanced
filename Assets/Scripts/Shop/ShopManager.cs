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

    // Pure check (no mutation) shared by the server-side purchase flow and
    // client-side UI (e.g. graying out the Buy button) — safe to call from
    // either side since it only reads synced state.
    public bool CanPurchase(GoldComponent gold, InventoryComponent inventory, ItemData item, out string reason)
    {
        reason = null;
        if (gold == null) { reason = "No GoldComponent."; return false; }
        if (inventory == null) { reason = "No InventoryComponent."; return false; }
        if (item == null) { reason = "Item not found."; return false; }
        if (!inventory.CanAddItem(item, out reason)) return false;
        if (gold.Gold.Value < item.cost) { reason = "Not enough gold."; return false; }
        return true;
    }

    // Called from ShopComponent.ServerRequestPurchase — already on server
    public bool TryPurchaseItem(GameObject buyer, string itemId, out string reason)
    {
        reason = null;
        if (buyer == null) { reason = "Buyer is null."; return false; }

        GoldComponent gold = buyer.GetComponent<GoldComponent>();
        InventoryComponent inventory = buyer.GetComponent<InventoryComponent>();
        ItemData item = GetItem(itemId);

        if (!CanPurchase(gold, inventory, item, out reason)) return false;

        if (!inventory.AddItem(item)) { reason = "Could not add item."; return false; }
        gold.Spend(item.cost);

        Debug.Log($"[ShopManager] {buyer.name} purchased {item.itemName} for {item.cost} gold.");
        return true;
    }

    // Called from ShopComponent.ServerRequestSell — already on server
    public bool TrySellItem(GameObject seller, string itemId, out string reason)
    {
        reason = null;
        if (seller == null) { reason = "Seller is null."; return false; }

        GoldComponent gold = seller.GetComponent<GoldComponent>();
        InventoryComponent inventory = seller.GetComponent<InventoryComponent>();

        if (gold == null) { reason = "No GoldComponent."; return false; }
        if (inventory == null) { reason = "No InventoryComponent."; return false; }

        ItemData item = GetItem(itemId);
        if (item == null) { reason = "Item not found."; return false; }
        if (!inventory.HasItem(itemId)) { reason = "Item not owned."; return false; }

        if (!inventory.TryRemoveItem(itemId)) { reason = "Could not remove item."; return false; }

        int refund = Mathf.FloorToInt(item.cost * item.sellRefundPercent);
        gold.Award(refund);

        Debug.Log($"[ShopManager] {seller.name} sold {item.itemName} for {refund} gold.");
        return true;
    }
}