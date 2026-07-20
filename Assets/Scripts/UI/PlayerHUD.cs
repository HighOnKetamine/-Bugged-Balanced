using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private Image hpBarFill;
    [SerializeField] private TextMeshProUGUI hpText;

    [Header("Mana")]
    [SerializeField] private Image manaBarFill;
    [SerializeField] private TextMeshProUGUI manaText;

    [Header("Experience")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI xpText;
    [SerializeField] private Image xpBarFill;

    [Header("Stats")]
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI killsText;
    [SerializeField] private TextMeshProUGUI deathsText;

    [Header("Abilities")]
    [SerializeField] private GameObject abilitySlotPrefab;
    [SerializeField] private Transform abilitiesContainer;

    [Header("Inventory")]
    [SerializeField] private GameObject inventorySlotPrefab;
    [SerializeField] private Transform inventoryContainer;
    [SerializeField] private InventoryTooltipUI inventoryTooltip;

    [Header("Panel")]
    [SerializeField] private GameObject hudPanel;

    private HealthComponent _health;
    private ManaComponent _mana;
    private ExperienceComponent _experience;
    private PlayerScoreComponent _score;
    private GoldComponent _gold;
    private InventoryComponent _inventory;
    private ShopComponent _shopComponent;
    private readonly List<AbilitySlot> _abilitySlots = new List<AbilitySlot>();
    private readonly List<InventorySlotUI> _inventorySlots = new List<InventorySlotUI>();

    private void Awake()
    {
        if (hudPanel != null) hudPanel.SetActive(false);
        else gameObject.SetActive(false);
    }

    public void Initialize(GameObject playerObject)
    {
        if (hudPanel != null) hudPanel.SetActive(true);
        else gameObject.SetActive(true);

        _health = playerObject.GetComponent<HealthComponent>();
        _mana = playerObject.GetComponent<ManaComponent>();
        _experience = playerObject.GetComponent<ExperienceComponent>();
        _score = playerObject.GetComponent<PlayerScoreComponent>();
        _gold = playerObject.GetComponent<GoldComponent>();
        _shopComponent = playerObject.GetComponent<ShopComponent>();

        if (_health != null)
        {
            _health.OnHealthChanged += RefreshHealth;
            RefreshHealth(_health.Current, _health.Max);
        }

        if (_mana != null)
        {
            _mana.OnManaChanged += RefreshMana;
            RefreshMana(_mana.Current, _mana.Max);
        }

        if (_experience != null)
        {
            _experience.OnXPChanged += RefreshExperience;
            _experience.OnLevelChanged += RefreshLevel;
            RefreshLevel(_experience.Level.Value);
            RefreshExperience(_experience.CurrentXP.Value, _experience.XPToNextLevel.Value, _experience.Level.Value);
        }

        if (_score != null)
        {
            _score.OnScoreChanged += RefreshScore;
            RefreshScore(_score.Kills.Value, _score.Deaths.Value);
        }

        if (_gold != null)
        {
            _gold.OnGoldChanged += RefreshGold;
            RefreshGold(_gold.Gold.Value);
        }

        InitializeAbilities(playerObject);
        InitializeInventory(playerObject);
    }

    private void InitializeAbilities(GameObject playerObject)
    {
        if (abilitySlotPrefab == null || abilitiesContainer == null) return;

        foreach (Transform child in abilitiesContainer)
            Destroy(child.gameObject);
        _abilitySlots.Clear();

        AbilityBase[] abilities = playerObject.GetComponentsInChildren<AbilityBase>(true);
        foreach (AbilityBase ability in abilities)
        {
            GameObject slotGO = Instantiate(abilitySlotPrefab, abilitiesContainer);
            AbilitySlot slot = slotGO.GetComponent<AbilitySlot>();
            if (slot != null)
            {
                slot.Initialize(ability, ability.AbilityIcon);
                _abilitySlots.Add(slot);
            }
        }
    }

    private void InitializeInventory(GameObject playerObject)
    {
        _inventory = playerObject.GetComponent<InventoryComponent>();
        if (_inventory == null || inventorySlotPrefab == null || inventoryContainer == null) return;

        _inventory.OnItemAdded += HandleInventoryChanged;
        _inventory.OnItemRemoved += HandleInventoryChanged;

        foreach (Transform child in inventoryContainer)
            Destroy(child.gameObject);
        _inventorySlots.Clear();

        for (int i = 0; i < _inventory.MaxItemCount; i++)
        {
            GameObject slotGO = Instantiate(inventorySlotPrefab, inventoryContainer);
            InventorySlotUI slot = slotGO.GetComponent<InventorySlotUI>();
            if (slot != null)
            {
                slot.SetEmpty();
                slot.OnClicked += HandleInventorySlotClicked;
                slot.OnHoverEnter += HandleInventorySlotHoverEnter;
                slot.OnHoverExit += HandleInventorySlotHoverExit;
                _inventorySlots.Add(slot);
            }
        }

        RefreshInventory(null);
    }

    private void HandleInventoryChanged(ItemData item) => RefreshInventory(item);

    private void RefreshInventory(ItemData _)
    {
        if (_inventory == null) return;

        for (int i = 0; i < _inventorySlots.Count; i++)
        {
            ItemData item = i < _inventory.OwnedItemIds.Count
                ? ShopManager.Instance?.GetItem(_inventory.OwnedItemIds[i])
                : null;

            if (item != null)
                _inventorySlots[i].SetItem(item);
            else
                _inventorySlots[i].SetEmpty();
        }
    }

    private void HandleInventorySlotClicked(ItemData item)
    {
        if (!ShopZone.LocalPlayerInShop) return;
        _shopComponent?.ServerRequestSell(item.GetId());
    }

    private void HandleInventorySlotHoverEnter(ItemData item, InventorySlotUI slot)
    {
        inventoryTooltip?.Show(item, slot.GetComponent<RectTransform>());
    }

    private void HandleInventorySlotHoverExit(InventorySlotUI slot)
    {
        inventoryTooltip?.Hide();
    }

    private void OnDestroy()
    {
        if (_health != null) _health.OnHealthChanged -= RefreshHealth;
        if (_mana != null) _mana.OnManaChanged -= RefreshMana;
        if (_experience != null)
        {
            _experience.OnXPChanged -= RefreshExperience;
            _experience.OnLevelChanged -= RefreshLevel;
        }
        if (_score != null) _score.OnScoreChanged -= RefreshScore;
        if (_gold != null) _gold.OnGoldChanged -= RefreshGold;
        if (_inventory != null)
        {
            _inventory.OnItemAdded -= HandleInventoryChanged;
            _inventory.OnItemRemoved -= HandleInventoryChanged;
        }
    }

    private void RefreshHealth(float current, float max)
    {
        if (hpBarFill != null)
            hpBarFill.fillAmount = max <= 0 ? 0f : Mathf.Clamp01(current / max);
        if (hpText != null)
            hpText.text = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
    }

    private void RefreshMana(float current, float max)
    {
        if (manaBarFill != null)
            manaBarFill.fillAmount = max <= 0 ? 0f : Mathf.Clamp01(current / max);
        if (manaText != null)
            manaText.text = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
    }

    private void RefreshLevel(int level)
    {
        if (levelText != null)
            levelText.text = $"Lvl {level}";
    }

    private void RefreshExperience(int currentXp, int xpToNext, int level)
    {
        if (xpText != null)
            xpText.text = $"{currentXp}/{xpToNext}";
        if (xpBarFill != null)
            xpBarFill.fillAmount = xpToNext <= 0 ? 0f : Mathf.Clamp01((float)currentXp / xpToNext);
        if (levelText != null)
            levelText.text = $"Lvl {level}";
    }

    private void RefreshGold(int gold)
    {
        if (goldText != null)
            goldText.text = $"{gold}";
    }

    private void RefreshScore(int kills, int deaths)
    {
        if (killsText != null)
            killsText.text = $"{kills}";
        if (deathsText != null)
            deathsText.text = $"{deaths}";
    }
}