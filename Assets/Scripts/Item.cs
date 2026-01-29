using UnityEngine;

public enum ItemCategory
{
    Health,
    Damage,
    Utility
}

[CreateAssetMenu(fileName = "New Item", menuName = "Item")]
public class Item : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;
    public string description;
    public Sprite icon;
    public ItemCategory category;
    public int goldCost;

    [Header("Stat Bonuses")]
    public float healthBonus;
    public float damageBonus;
    public float armorBonus;
    public float magicResistBonus;
    public float attackSpeedBonus;
    public float movementSpeedBonus;

    [Header("Combinations")]
    public Item[] requiredItems; // Items needed to build this item
    public Item upgradedItem; // What this item upgrades to

    public bool CanBuildWith(Item[] inventoryItems)
    {
        if (requiredItems.Length == 0) return true;

        foreach (Item required in requiredItems)
        {
            bool found = false;
            foreach (Item inventoryItem in inventoryItems)
            {
                if (inventoryItem == required)
                {
                    found = true;
                    break;
                }
            }
            if (!found) return false;
        }
        return true;
    }
}