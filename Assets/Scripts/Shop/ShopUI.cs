using System.Collections.Generic;
using FishNet.Object;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Transform itemsContainer;
    [SerializeField] private GameObject itemSlotPrefab;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private float shopRadius = 8f;

    private ShopComponent _shopComponent;
    private GoldComponent _goldComponent;
    private Transform _baseTransform;
    private bool _isOpen = false;

    public void Initialize(GameObject playerObject, Transform baseTransform)
    {
        _shopComponent = playerObject.GetComponent<ShopComponent>();
        _goldComponent = playerObject.GetComponent<GoldComponent>();
        _baseTransform = baseTransform;

        if (_goldComponent != null)
            _goldComponent.OnGoldChanged += UpdateGold;

        shopPanel.SetActive(false);
        PopulateItems();
    }

    private void OnDestroy()
    {
        if (_goldComponent != null)
            _goldComponent.OnGoldChanged -= UpdateGold;
    }

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

    private bool IsNearBase()
    {
        if (_baseTransform == null || _shopComponent == null) return false;
        return Vector3.Distance(
            _shopComponent.transform.position,
            _baseTransform.position) <= shopRadius;
    }

    private void OpenShop()
    {
        _isOpen = true;
        shopPanel.SetActive(true);
        UpdateGold(_goldComponent?.Gold.Value ?? 0);
        PlayerController.InputDisabled = true;
    }

    private void CloseShop()
    {
        _isOpen = false;
        shopPanel.SetActive(false);
        PlayerController.InputDisabled = false;
    }

    private void PopulateItems()
    {
        if (ShopManager.Instance == null) return;
        ItemData[] items = ShopManager.Instance.GetAvailableItems();
        if (items == null) return;

        foreach (Transform child in itemsContainer)
            Destroy(child.gameObject);

        foreach (ItemData item in items)
        {
            if (item == null) continue;
            GameObject slot = Instantiate(itemSlotPrefab, itemsContainer);
            SetupItemSlot(slot, item);
        }
    }

    private void SetupItemSlot(GameObject slot, ItemData item)
    {
        // Icon
        Image icon = slot.transform.Find("Icon")?.GetComponent<Image>();
        if (icon != null && item.icon != null)
            icon.sprite = item.icon;

        // Name (Fixed sub-child path)
        TextMeshProUGUI nameText = slot.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
        if (nameText != null)
            nameText.text = item.itemName;

        // Cost (Fixed sub-child path)
        TextMeshProUGUI costText = slot.transform.Find("Cost")?.GetComponent<TextMeshProUGUI>();
        if (costText != null)
            costText.text = $"{item.cost}g";

        // Description (Fixed sub-child path)
        TextMeshProUGUI descText = slot.transform.Find("Description")?.GetComponent<TextMeshProUGUI>();
        if (descText != null)
            descText.text = item.description;

        // Buy button
        Button buyButton = slot.transform.Find("BuyButton")?.GetComponent<Button>();
        if (buyButton != null)
        {
            string itemId = item.GetId();
            buyButton.onClick.AddListener(() => OnBuyClicked(itemId));
        }
    }

    private void OnBuyClicked(string itemId)
    {
        _shopComponent?.ServerRequestPurchase(itemId);
    }

    private void UpdateGold(int gold)
    {
        if (goldText != null)
            goldText.text = $"Gold: {gold}";
    }

    private void HideStatus()
    {
        if (statusText != null)
            statusText.gameObject.SetActive(false);
    }
}