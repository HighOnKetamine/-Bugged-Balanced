using UnityEngine;

public enum ChampionType { Warrior, Mage }

[DisallowMultipleComponent]
public class ChampionLoadout : MonoBehaviour
{
    [Header("Champion Loadout")]
    [SerializeField] private ChampionType championType = ChampionType.Warrior;

    private void Awake()
    {
        ValidateRequiredComponents();
        AddChampionAbilities();
    }

    private void ValidateRequiredComponents()
    {
        if (GetComponent<EffectComponent>() == null)
            Debug.LogError($"[ChampionLoadout] {gameObject.name} missing EffectComponent — add it to the prefab!");
        if (GetComponent<ExperienceComponent>() == null)
            Debug.LogError($"[ChampionLoadout] {gameObject.name} missing ExperienceComponent — add it to the prefab!");
        if (GetComponent<GoldComponent>() == null)
            Debug.LogError($"[ChampionLoadout] {gameObject.name} missing GoldComponent — add it to the prefab!");
        if (GetComponent<PlayerScoreComponent>() == null)
            Debug.LogError($"[ChampionLoadout] {gameObject.name} missing PlayerScoreComponent — add it to the prefab!");
        if (GetComponent<InventoryComponent>() == null)
            Debug.LogError($"[ChampionLoadout] {gameObject.name} missing InventoryComponent — add it to the prefab!");
        if (GetComponent<ShopComponent>() == null)
            Debug.LogError($"[ChampionLoadout] {gameObject.name} missing ShopComponent — add it to the prefab!");
        if (GetComponent<CharacterStats>() == null)
            Debug.LogError($"[ChampionLoadout] {gameObject.name} missing CharacterStats — add it to the prefab!");
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
}