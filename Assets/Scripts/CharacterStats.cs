using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [Header("Unit Data")]
    public UnitData data;

    [Header("Health & Mana")]
    public Stat maxHealth = new();
    public Stat healthRegen = new();
    public Stat maxMana = new();
    public Stat manaRegen = new();
    public ResourceType resourceType;

    [Header("Offense")]
    public Stat attackDamage = new();
    public Stat abilityPower = new();
    public Stat attackSpeed = new();
    public Stat attackRange = new();
    public Stat abilityHaste = new();
    public Stat lethality = new();
    public Stat armorPenPercent = new();
    public Stat flatMagicPen = new();
    public Stat magicPenPercent = new();

    [Header("Defense")]
    public Stat armor = new();
    public Stat magicResist = new();

    [Header("Mobility")]
    public Stat moveSpeed = new();
    public Stat visionRange = new();

    private void Awake()
    {
        if (data == null)
        {
            Debug.LogError($"[CharacterStats] No UnitData assigned on {gameObject.name}!");
            return;
        }

        maxHealth.BaseValue = data.baseMaxHealth;
        healthRegen.BaseValue = data.baseHealthRegen;
        maxMana.BaseValue = data.baseMaxMana;
        manaRegen.BaseValue = data.baseManaRegen;
        resourceType = data.resourceType;

        attackDamage.BaseValue = data.baseAttackDamage;
        abilityPower.BaseValue = data.baseAbilityPower;
        attackSpeed.BaseValue = data.baseAttackSpeed;
        attackRange.BaseValue = data.baseAttackRange;
        abilityHaste.BaseValue = data.baseAbilityHaste;
        lethality.BaseValue = data.baseLethality;
        armorPenPercent.BaseValue = data.baseArmorPenPercent;
        flatMagicPen.BaseValue = data.baseFlatMagicPen;
        magicPenPercent.BaseValue = data.baseMagicPenPercent;

        armor.BaseValue = data.baseArmor;
        magicResist.BaseValue = data.baseMagicResist;

        moveSpeed.BaseValue = data.baseMoveSpeed;
        visionRange.BaseValue = data.baseVisionRange;
    }
}