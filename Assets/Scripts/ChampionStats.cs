using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections.Generic;

public class ChampionStats : NetworkBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private float baseHealth = 100f;
    [SerializeField] private float baseDamage = 10f;
    [SerializeField] private float baseArmor = 0f;
    [SerializeField] private float baseMagicResist = 0f;
    [SerializeField] private float baseAttackSpeed = 1f;
    [SerializeField] private float baseMovementSpeed = 5f;

    // Modifiers from items
    private float healthModifier = 0f;
    private float damageModifier = 0f;
    private float armorModifier = 0f;
    private float magicResistModifier = 0f;
    private float attackSpeedModifier = 0f;
    private float movementSpeedModifier = 0f;

    // Current stats (base + modifiers)
    public float MaxHealth => baseHealth + healthModifier;
    public float AttackDamage => baseDamage + damageModifier;
    public float Armor => baseArmor + armorModifier;
    public float MagicResist => baseMagicResist + magicResistModifier;
    public float AttackSpeed => baseAttackSpeed + attackSpeedModifier;
    public float MovementSpeed => baseMovementSpeed + movementSpeedModifier;

    private HealthSystem healthSystem;
    private AutoAttackSystem autoAttackSystem;
    private NavMeshAgent navMeshAgent;

    private void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();
        autoAttackSystem = GetComponent<AutoAttackSystem>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        UpdateStats();
    }

    // Apply item bonuses
    public void AddItemBonuses(Item item)
    {
        healthModifier += item.HealthBonus;
        damageModifier += item.DamageBonus;
        armorModifier += item.ArmorBonus;
        magicResistModifier += item.MagicResistBonus;
        attackSpeedModifier += item.AttackSpeedBonus;
        movementSpeedModifier += item.MovementSpeedBonus;

        UpdateStats();
    }

    // Remove item bonuses (for selling or unequipping)
    public void RemoveItemBonuses(Item item)
    {
        healthModifier -= item.HealthBonus;
        damageModifier -= item.DamageBonus;
        armorModifier -= item.ArmorBonus;
        magicResistModifier -= item.MagicResistBonus;
        attackSpeedModifier -= item.AttackSpeedBonus;
        movementSpeedModifier -= item.MovementSpeedBonus;

        UpdateStats();
    }

    private void UpdateStats()
    {
        // Update HealthSystem max health
        if (healthSystem != null)
        {
            healthSystem.UpdateMaxHealthModifier(healthModifier);
        }

        // Update AutoAttackSystem damage
        if (autoAttackSystem != null)
        {
            autoAttackSystem.AttackDamage = AttackDamage;
        }

        // Update NavMeshAgent speed
        if (navMeshAgent != null)
        {
            navMeshAgent.speed = MovementSpeed;
        }
    }
}