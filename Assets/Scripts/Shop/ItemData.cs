using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Shop/Item")]
public class ItemData : ScriptableObject
{
    [Header("Item Info")]
    public string itemId;
    public string itemName;
    public ItemCategory category;
    [TextArea]
    public string description;
    [TextArea]
    public string flavorText;
    public int cost;
    [Range(0f, 1f)]
    public float sellRefundPercent = 0.7f;
    public Sprite icon;
    public bool allowMultiple = false;
    [Tooltip("0 = no category limit. Otherwise, the max number of items " +
        "owned TOTAL from this item's category, across any items sharing " +
        "it (e.g. set every Boots item's limit to 1 so only one pair of " +
        "boots, of any kind, can be owned at once).")]
    public int categoryOwnLimit = 0;
    public ItemModifier[] modifiers;

    public string GetId() => string.IsNullOrWhiteSpace(itemId) ? name : itemId;
}

public enum ItemCategory { Offense, Defense, Mobility, Utility, Penetration }

[Serializable]
public struct ItemModifier
{
    public CharacterStatType stat;
    public ModifierType modifierType;
    public float value;
}

public enum CharacterStatType
{
    MaxHealth, HealthRegen, MaxMana, ManaRegen,
    AttackDamage, AbilityPower, AttackSpeed, AttackRange,
    AbilityHaste, Lethality, ArmorPenPercent, FlatMagicPen,
    MagicPenPercent, Armor, MagicResist, MoveSpeed, VisionRange
}

public enum ModifierType { Flat, Percent }