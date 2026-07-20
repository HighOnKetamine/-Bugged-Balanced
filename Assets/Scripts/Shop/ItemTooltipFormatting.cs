using UnityEngine;

public static class ItemTooltipFormatting
{
    public static string FormatSellValue(ItemData item)
    {
        int refund = Mathf.FloorToInt(item.cost * item.sellRefundPercent);
        int percent = Mathf.RoundToInt(item.sellRefundPercent * 100f);
        return $"Sell: {percent}% ({refund}g)";
    }

    public static string FormatModifier(ItemModifier modifier)
    {
        bool isPercent = modifier.modifierType == ModifierType.Percent;
        float displayValue = isPercent ? modifier.value * 100f : modifier.value;
        string sign = displayValue >= 0 ? "+" : "";
        string suffix = isPercent ? "%" : "";
        return $"{sign}{displayValue:0.##}{suffix} {StatDisplayName(modifier.stat)}";
    }

    public static string StatDisplayName(CharacterStatType stat) => stat switch
    {
        CharacterStatType.MaxHealth => "Health",
        CharacterStatType.HealthRegen => "Health Regen",
        CharacterStatType.MaxMana => "Mana",
        CharacterStatType.ManaRegen => "Mana Regen",
        CharacterStatType.AttackDamage => "AD",
        CharacterStatType.AbilityPower => "AP",
        CharacterStatType.AttackSpeed => "Attack Speed",
        CharacterStatType.AttackRange => "Attack Range",
        CharacterStatType.AbilityHaste => "Ability Haste",
        CharacterStatType.Lethality => "Lethality",
        CharacterStatType.ArmorPenPercent => "Armor Pen",
        CharacterStatType.FlatMagicPen => "Magic Pen",
        CharacterStatType.MagicPenPercent => "Magic Pen",
        CharacterStatType.Armor => "Armor",
        CharacterStatType.MagicResist => "Magic Resist",
        CharacterStatType.MoveSpeed => "Move Speed",
        CharacterStatType.VisionRange => "Vision Range",
        _ => stat.ToString()
    };
}
