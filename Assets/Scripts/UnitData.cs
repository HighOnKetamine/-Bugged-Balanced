using UnityEngine;

[CreateAssetMenu(menuName = "MOBA/UnitData")]
public class UnitData : ScriptableObject
{
    public string unitName;

    [Header("Health & Mana")]
    public float baseMaxHealth;
    public float baseHealthRegen;
    public float baseMaxMana;
    public float baseManaRegen;
    public ResourceType resourceType;

    [Header("Offense")]
    public float baseAttackDamage;
    public float baseAbilityPower;
    public float baseAttackSpeed;
    public float baseAttackRange;
    public float baseAbilityHaste;
    public float baseLethality;
    public float baseArmorPenPercent;
    public float baseFlatMagicPen;
    public float baseMagicPenPercent;

    [Header("Defense")]
    public float baseArmor;
    public float baseMagicResist;

    [Header("Mobility")]
    public float baseMoveSpeed;
    public float baseVisionRange;
}