using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class InventoryComponent : NetworkBehaviour
{
    [Header("Inventory")]
    [SerializeField] private int maxItemCount = 6;

    public readonly SyncList<string> OwnedItemIds = new SyncList<string>();

    public event System.Action<ItemData> OnItemAdded;
    public event System.Action<ItemData> OnItemRemoved;

    private readonly List<string> _appliedItemIds = new List<string>();
    private CharacterStats _stats;

    public int MaxItemCount => maxItemCount;
    public int ItemCount => OwnedItemIds.Count;

    private void Awake()
    {
        _stats = GetComponent<CharacterStats>();
        if (_stats == null)
            Debug.LogWarning($"[InventoryComponent] {gameObject.name} missing CharacterStats.");
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        OwnedItemIds.OnChange += HandleInventoryChanged;
        RebuildItems();
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        OwnedItemIds.OnChange -= HandleInventoryChanged;
    }

    // Called from ShopManager — already on server
    public bool AddItem(ItemData item)
    {
        if (item == null || !CanAddItem(item, out _)) return false;
        OwnedItemIds.Add(item.GetId());
        return true;
    }

    public bool CanAddItem(ItemData item, out string reason)
    {
        reason = null;
        if (item == null) { reason = "Item not found."; return false; }
        if (!item.allowMultiple && OwnedItemIds.Contains(item.GetId())) { reason = "Already owned."; return false; }
        if (OwnedItemIds.Count >= maxItemCount) { reason = "Inventory full."; return false; }
        if (item.categoryOwnLimit > 0 && CountOwnedInCategory(item.category) >= item.categoryOwnLimit)
        {
            reason = $"Only {item.categoryOwnLimit} {item.category} item(s) allowed.";
            return false;
        }
        return true;
    }

    private int CountOwnedInCategory(ItemCategory category)
    {
        int count = 0;
        foreach (string ownedId in OwnedItemIds)
        {
            ItemData owned = ShopManager.Instance?.GetItem(ownedId);
            if (owned != null && owned.category == category) count++;
        }
        return count;
    }

    public bool HasItem(string itemId) => OwnedItemIds.Contains(itemId);

    // Called from ShopManager — already on server
    public bool TryRemoveItem(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId)) return false;
        return OwnedItemIds.Remove(itemId);
    }

    private void RebuildItems()
    {
        ClearAppliedItems();
        foreach (string itemId in OwnedItemIds)
            ApplyItem(itemId);
    }

    private void HandleInventoryChanged(SyncListOperation op, int index, string previous, string next, bool asServer)
    {
        switch (op)
        {
            case SyncListOperation.Add:
                ApplyItem(next);
                break;
            case SyncListOperation.RemoveAt:
                RemoveItem(previous);
                break;
            case SyncListOperation.Clear:
                ClearAppliedItems();
                break;
            case SyncListOperation.Insert:
            case SyncListOperation.Set:
                ClearAppliedItems();
                RebuildItems();
                break;
        }
    }

    // _appliedItemIds mirrors OwnedItemIds one-for-one (not a deduplicated
    // set) so a stackable item's 2nd, 3rd, ... copy each independently
    // applies its own modifiers and fires OnItemAdded, instead of being
    // silently dropped because "this id was already applied once."
    private void ApplyItem(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId)) return;
        ItemData item = ShopManager.Instance?.GetItem(itemId);
        if (item == null) { Debug.LogWarning($"[InventoryComponent] Item {itemId} not found."); return; }
        if (_stats == null) { Debug.LogWarning($"[InventoryComponent] Missing CharacterStats."); return; }
        foreach (ItemModifier modifier in item.modifiers)
            ApplyModifier(modifier);
        _appliedItemIds.Add(itemId);
        OnItemAdded?.Invoke(item);
    }

    private void RemoveItem(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId)) return;
        int index = _appliedItemIds.IndexOf(itemId);
        if (index < 0) return;
        ItemData item = ShopManager.Instance?.GetItem(itemId);
        if (item == null || _stats == null) return;
        foreach (ItemModifier modifier in item.modifiers)
            RemoveModifier(modifier);
        _appliedItemIds.RemoveAt(index);
        OnItemRemoved?.Invoke(item);
    }

    private void ClearAppliedItems()
    {
        foreach (string itemId in new List<string>(_appliedItemIds))
            RemoveItem(itemId);
        _appliedItemIds.Clear();
    }

    private void ApplyModifier(ItemModifier modifier)
    {
        Stat stat = GetStat(modifier.stat);
        if (stat == null) return;
        if (modifier.modifierType == ModifierType.Flat) stat.AddFlat(modifier.value);
        else stat.AddPercent(modifier.value);
    }

    private void RemoveModifier(ItemModifier modifier)
    {
        Stat stat = GetStat(modifier.stat);
        if (stat == null) return;
        if (modifier.modifierType == ModifierType.Flat) stat.RemoveFlat(modifier.value);
        else stat.RemovePercent(modifier.value);
    }

    private Stat GetStat(CharacterStatType statType) => statType switch
    {
        CharacterStatType.MaxHealth => _stats.maxHealth,
        CharacterStatType.HealthRegen => _stats.healthRegen,
        CharacterStatType.MaxMana => _stats.maxMana,
        CharacterStatType.ManaRegen => _stats.manaRegen,
        CharacterStatType.AttackDamage => _stats.attackDamage,
        CharacterStatType.AbilityPower => _stats.abilityPower,
        CharacterStatType.AttackSpeed => _stats.attackSpeed,
        CharacterStatType.AttackRange => _stats.attackRange,
        CharacterStatType.AbilityHaste => _stats.abilityHaste,
        CharacterStatType.Lethality => _stats.lethality,
        CharacterStatType.ArmorPenPercent => _stats.armorPenPercent,
        CharacterStatType.FlatMagicPen => _stats.flatMagicPen,
        CharacterStatType.MagicPenPercent => _stats.magicPenPercent,
        CharacterStatType.Armor => _stats.armor,
        CharacterStatType.MagicResist => _stats.magicResist,
        CharacterStatType.MoveSpeed => _stats.moveSpeed,
        CharacterStatType.VisionRange => _stats.visionRange,
        _ => null
    };
}