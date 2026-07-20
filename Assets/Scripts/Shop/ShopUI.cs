using System;
using System.Collections.Generic;
using FishNet.Object;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button closeButton;

    [Header("Filter Bar")]
    [SerializeField] private Transform filterBarContainer;
    [SerializeField] private GameObject filterButtonPrefab;

    [Header("Grid")]
    [SerializeField] private Transform itemsContainer;
    [SerializeField] private GameObject itemSlotPrefab;

    [Header("Details Panel")]
    [SerializeField] private ShopDetailsPanelUI detailsPanel;

    private ShopComponent _shopComponent;
    private GoldComponent _goldComponent;
    private InventoryComponent _inventory;
    private bool _isOpen = false;

    private ItemCategory? _activeCategory = null;
    private ItemData _selectedItem;
    private readonly List<ShopItemSlotUI> _spawnedSlots = new List<ShopItemSlotUI>();
    private readonly List<(ShopFilterButtonUI button, ItemCategory? category)> _filterButtons = new List<(ShopFilterButtonUI, ItemCategory?)>();

    private void Awake()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
        if (closeButton != null) closeButton.onClick.AddListener(CloseShop);
    }

    public void Initialize(GameObject playerObject)
    {
        _shopComponent = playerObject.GetComponent<ShopComponent>();
        _goldComponent = playerObject.GetComponent<GoldComponent>();
        _inventory = playerObject.GetComponent<InventoryComponent>();

        if (_goldComponent != null)
            _goldComponent.OnGoldChanged += UpdateGold;

        if (_inventory != null)
        {
            _inventory.OnItemAdded += HandleInventoryChanged;
            _inventory.OnItemRemoved += HandleInventoryChanged;
        }

        if (detailsPanel != null)
            detailsPanel.OnBuyClicked += OnBuyClicked;

        shopPanel.SetActive(false);
        BuildFilterBar();
        PopulateItems();
    }

    private void OnDestroy()
    {
        if (_goldComponent != null)
            _goldComponent.OnGoldChanged -= UpdateGold;

        if (_inventory != null)
        {
            _inventory.OnItemAdded -= HandleInventoryChanged;
            _inventory.OnItemRemoved -= HandleInventoryChanged;
        }

        if (detailsPanel != null)
            detailsPanel.OnBuyClicked -= OnBuyClicked;
    }

    private void HandleInventoryChanged(ItemData item) => RefreshBuyState();

    private void Update()
    {
        // --- AUTO-CLOSE LOGIC ---
        // If the shop is open and the player walks too far away, close it automatically.
        if (_isOpen && !IsNearBase())
        {
            CloseShop();
        }

        // Handle manual opening/closing via keypress
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (_isOpen)
            {
                CloseShop();
                return;
            }

            if (!IsNearBase())
            {
                if (statusText != null)
                {
                    statusText.text = "Return to base to shop!";
                    statusText.gameObject.SetActive(true);
                    CancelInvoke(nameof(HideStatus));
                    Invoke(nameof(HideStatus), 2f);
                }
                return;
            }

            OpenShop();
        }
    }

    private static bool IsNearBase() => ShopZone.LocalPlayerInShop;

    private void OpenShop()
    {
        _isOpen = true;
        shopPanel.SetActive(true);
        UpdateGold(_goldComponent?.Gold.Value ?? 0);

        if (_selectedItem == null)
            detailsPanel?.ShowEmpty();
        else
            RefreshBuyState();

        PlayerController.InputDisabled = true;
    }

    private void CloseShop()
    {
        _isOpen = false;
        shopPanel.SetActive(false);
        PlayerController.InputDisabled = false;
    }

    private void BuildFilterBar()
    {
        if (filterBarContainer == null || filterButtonPrefab == null) return;

        foreach (Transform child in filterBarContainer)
            Destroy(child.gameObject);
        _filterButtons.Clear();

        CreateFilterButton("All", null);
        foreach (ItemCategory category in Enum.GetValues(typeof(ItemCategory)))
            CreateFilterButton(category.ToString(), category);

        ApplyFilter(null);
    }

    private void CreateFilterButton(string label, ItemCategory? category)
    {
        GameObject buttonObj = Instantiate(filterButtonPrefab, filterBarContainer);
        ShopFilterButtonUI filterButton = buttonObj.GetComponent<ShopFilterButtonUI>();
        if (filterButton == null) return;

        filterButton.Setup(label);
        filterButton.OnClicked += () => ApplyFilter(category);
        _filterButtons.Add((filterButton, category));
    }

    private void ApplyFilter(ItemCategory? category)
    {
        _activeCategory = category;

        foreach (ShopItemSlotUI slot in _spawnedSlots)
            slot.gameObject.SetActive(category == null || slot.Item.category == category);

        foreach ((ShopFilterButtonUI button, ItemCategory? buttonCategory) in _filterButtons)
            button.SetSelected(buttonCategory == category);
    }

    private void PopulateItems()
    {
        if (ShopManager.Instance == null || itemsContainer == null || itemSlotPrefab == null) return;

        ItemData[] items = ShopManager.Instance.GetAvailableItems();
        if (items == null) return;

        foreach (Transform child in itemsContainer)
            Destroy(child.gameObject);
        _spawnedSlots.Clear();

        foreach (ItemData item in items)
        {
            if (item == null) continue;
            GameObject slotObj = Instantiate(itemSlotPrefab, itemsContainer);
            ShopItemSlotUI slot = slotObj.GetComponent<ShopItemSlotUI>();
            if (slot == null) continue;

            slot.Setup(item);
            slot.OnClicked += SelectItem;
            _spawnedSlots.Add(slot);
        }

        ApplyFilter(_activeCategory);
    }

    private void SelectItem(ItemData item)
    {
        _selectedItem = item;
        detailsPanel?.Show(item);
        RefreshBuyState();

        foreach (ShopItemSlotUI slot in _spawnedSlots)
            slot.SetSelected(slot.Item == item);
    }

    private void RefreshBuyState()
    {
        if (_selectedItem == null || detailsPanel == null || ShopManager.Instance == null) return;

        bool canBuy = ShopManager.Instance.CanPurchase(_goldComponent, _inventory, _selectedItem, out string reason);
        detailsPanel.SetBuyable(canBuy, reason);
    }

    private void OnBuyClicked(string itemId)
    {
        _shopComponent?.ServerRequestPurchase(itemId);
    }

    private void UpdateGold(int gold)
    {
        if (goldText != null)
            goldText.text = $"Gold: {gold}";

        RefreshBuyState();
    }

    private void HideStatus()
    {
        if (statusText != null)
            statusText.gameObject.SetActive(false);
    }
}
