using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Shop/Item")]
public class ItemData : ScriptableObject
{
    [Header("Item Info")]
    public string itemId;
    public string itemName;
    [TextArea]
    public string description;
    public int cost;
    public Sprite icon;
    public bool allowMultiple = false;
    public ItemModifier[] modifiers;

    public string GetId() => string.IsNullOrWhiteSpace(itemId) ? name : itemId;
}

[Serializable]
public struct ItemModifier
{
    public CharacterStatType stat;
    public ModifierType modifierType;
    public float value;
}

public enum CharacterStatType
{
    MaxHealth,
    HealthRegen,
    MaxMana,
    ManaRegen,
    AttackDamage,
    AbilityPower,
    AttackSpeed,
    AttackRange,
    AbilityHaste,
    Lethality,
    ArmorPenPercent,
    FlatMagicPen,
    MagicPenPercent,
    Armor,
    MagicResist,
    MoveSpeed,
    VisionRange
}

public enum ModifierType
{
    Flat,
    Percent
}
