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
            Debug.LogWarning($"[InventoryComponent] {gameObject.name} is missing CharacterStats.");
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

    [Server]
    public bool AddItem(ItemData item)
    {
        if (item == null)
            return false;

        if (!CanAddItem(item))
            return false;

        string itemId = item.GetId();
        if (string.IsNullOrWhiteSpace(itemId))
            return false;

        if (!item.allowMultiple && OwnedItemIds.Contains(itemId))
            return false;

        OwnedItemIds.Add(itemId);
        return true;
    }

    public bool CanAddItem(ItemData item)
    {
        if (item == null)
            return false;

        if (OwnedItemIds.Count >= maxItemCount)
            return false;

        if (!item.allowMultiple && OwnedItemIds.Contains(item.GetId()))
            return false;

        return true;
    }

    public bool HasItem(string itemId)
    {
        return OwnedItemIds.Contains(itemId);
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

    private void ApplyItem(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId) || _appliedItemIds.Contains(itemId))
            return;

        ItemData item = ShopManager.Instance?.GetItem(itemId);
        if (item == null)
        {
            Debug.LogWarning($"[InventoryComponent] Item {itemId} not found in ShopManager.");
            return;
        }

        if (_stats == null)
        {
            Debug.LogWarning($"[InventoryComponent] Cannot apply item {itemId} because CharacterStats is missing.");
            return;
        }

        foreach (ItemModifier modifier in item.modifiers)
            ApplyModifier(modifier);

        _appliedItemIds.Add(itemId);
        OnItemAdded?.Invoke(item);
    }

    private void RemoveItem(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId) || !_appliedItemIds.Contains(itemId))
            return;

        ItemData item = ShopManager.Instance?.GetItem(itemId);
        if (item == null)
            return;

        if (_stats == null)
            return;

        foreach (ItemModifier modifier in item.modifiers)
            RemoveModifier(modifier);

        _appliedItemIds.Remove(itemId);
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

        if (modifier.modifierType == ModifierType.Flat)
            stat.AddFlat(modifier.value);
        else
            stat.AddPercent(modifier.value);
    }

    private void RemoveModifier(ItemModifier modifier)
    {
        Stat stat = GetStat(modifier.stat);
        if (stat == null) return;

        if (modifier.modifierType == ModifierType.Flat)
            stat.RemoveFlat(modifier.value);
        else
            stat.RemovePercent(modifier.value);
    }

    private Stat GetStat(CharacterStatType statType)
    {
        if (_stats == null)
            return null;

        return statType switch
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
            _ => null,
        };
    }
}
