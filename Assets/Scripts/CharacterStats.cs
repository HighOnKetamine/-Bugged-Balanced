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

    [Header("Misc")]
    public int goldReward;

    public int CurrentLevel { get; private set; } = 1;
    private const float LevelGrowthMultiplier = 0.08f;

    private void Awake()
    {
        if (data == null)
        {
            Debug.LogError($"[CharacterStats] No UnitData assigned on {gameObject.name}!");
            return;
        }

        resourceType = data.resourceType;
        SetLevel(CurrentLevel);
    }

    public void SetLevel(int level)
    {
        CurrentLevel = Mathf.Max(1, level);
        RecalculateStats();
    }

    private void RecalculateStats()
    {
        float multiplier = 1f + (CurrentLevel - 1) * LevelGrowthMultiplier;

        maxHealth.BaseValue = data.baseMaxHealth * multiplier;
        healthRegen.BaseValue = data.baseHealthRegen * multiplier;
        maxMana.BaseValue = data.baseMaxMana * multiplier;
        manaRegen.BaseValue = data.baseManaRegen * multiplier;

        attackDamage.BaseValue = data.baseAttackDamage * multiplier;
        abilityPower.BaseValue = data.baseAbilityPower * multiplier;
        attackSpeed.BaseValue = data.baseAttackSpeed * multiplier;
        attackRange.BaseValue = data.baseAttackRange * multiplier;
        abilityHaste.BaseValue = data.baseAbilityHaste * multiplier;
        lethality.BaseValue = data.baseLethality * multiplier;
        armorPenPercent.BaseValue = data.baseArmorPenPercent * multiplier;
        flatMagicPen.BaseValue = data.baseFlatMagicPen * multiplier;
        magicPenPercent.BaseValue = data.baseMagicPenPercent * multiplier;

        armor.BaseValue = data.baseArmor * multiplier;
        magicResist.BaseValue = data.baseMagicResist * multiplier;

        moveSpeed.BaseValue = data.baseMoveSpeed * multiplier;
        visionRange.BaseValue = data.baseVisionRange * multiplier;
        goldReward = Mathf.RoundToInt(data.baseGoldReward * multiplier);
    }
}
