using UnityEngine;

public enum ChampionType
{
    Warrior,
    Mage
}

[DisallowMultipleComponent]
public class ChampionLoadout : MonoBehaviour
{
    [Header("Champion Loadout")]
    [SerializeField] private ChampionType championType = ChampionType.Warrior;

    private void Awake()
    {
        AddRequiredComponents();
        AddChampionAbilities();
    }

    private void AddRequiredComponents()
    {
        EnsureComponent<EffectComponent>();
        EnsureComponent<ExperienceComponent>();
        EnsureComponent<GoldComponent>();
        EnsureComponent<PlayerScoreComponent>();
        EnsureComponent<InventoryComponent>();
        EnsureComponent<ShopComponent>();

        if (GetComponent<CharacterStats>() == null)
            Debug.LogWarning($"[ChampionLoadout] {gameObject.name} is missing CharacterStats. Add CharacterStats manually for proper stat scaling.");
    }

    private void AddChampionAbilities()
    {
        switch (championType)
        {
            case ChampionType.Warrior:
                EnsureAbility<DashAbility>();
                EnsureAbility<AoeCircleAbility>();
                EnsureAbility<EmpowerAbility>();
                break;
            case ChampionType.Mage:
                EnsureAbility<FireballAbility>();
                EnsureAbility<PoisonStrikeAbility>();
                EnsureAbility<ShieldAbility>();
                break;
        }
    }

    private void EnsureAbility<T>() where T : AbilityBase
    {
        if (GetComponent<T>() != null) return;
        gameObject.AddComponent<T>();
    }

    private void EnsureComponent<T>() where T : Component
    {
        if (GetComponent<T>() != null) return;
        gameObject.AddComponent<T>();
    }
}