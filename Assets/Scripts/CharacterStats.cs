using Unity;
using UnityEngine;

public enum ResourceType { Mana, Energy, None }

public class CharacterStats : MonoBehaviour
{
    [Header("Base stats")]
    public Stat attackDamage = new() { BaseValue = 55 };
    public Stat abilityPower = new() { BaseValue = 0 };
    public Stat armor = new() { BaseValue = 30 };
    public Stat magicResist = new() { BaseValue = 30 };
    public Stat moveSpeed = new() { BaseValue = 345 };
    public Stat attackSpeed = new() { BaseValue = 0.65f };
    public Stat attackRange = new() { BaseValue = 600 };
    public Stat visionRange = new() { BaseValue = 1500 };


}