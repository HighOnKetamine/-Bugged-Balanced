using UnityEngine;
using UnityEngine.UI;
using FishNet.Object;
using FishNet.Connection;
using TMPro;
using System.Collections.Generic;

public class ShopUI : NetworkBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Transform itemContainer;
    [SerializeField] private GameObject itemButtonPrefab;
    [SerializeField] private TMP_Text goldText;

    private NetworkConnection localConnection;
    private Item[] availableItems;

    private void Start()
    {
        if (IsOwner)
        {
            localConnection = LocalConnection;
            shopPanel.SetActive(false);
            UpdateGoldDisplay();
        }
    }

    private void Update()
    {
        if (IsOwner && Input.GetKeyDown(KeyCode.B))
        {
            ToggleShop();
        }

        if (IsOwner && shopPanel.activeSelf)
        {
            UpdateGoldDisplay();
        }
    }

    private void ToggleShop()
    {
        shopPanel.SetActive(!shopPanel.activeSelf);
        if (shopPanel.activeSelf)
        {
            RefreshShopItems();
        }
    }

    private void RefreshShopItems()
    {
        // Clear existing items
        foreach (Transform child in itemContainer)
        {
            Destroy(child.gameObject);
        }

        availableItems = ShopManager.Instance.GetAvailableItems();

        foreach (Item item in availableItems)
        {
            GameObject buttonObj = Instantiate(itemButtonPrefab, itemContainer);
            ShopItemButton button = buttonObj.GetComponent<ShopItemButton>();
            if (button != null)
            {
                button.Setup(item, this);
            }
        }
    }

    public void TryPurchaseItem(Item item)
    {
        if (localConnection != null)
        {
            ServerTryPurchaseItem(localConnection, item);
        }
    }

    [ServerRpc]
    private void ServerTryPurchaseItem(NetworkConnection conn, Item item)
    {
        ShopManager.Instance.TryPurchaseItem(conn, item);
    }

    private void UpdateGoldDisplay()
    {
        if (EconomyManager.Instance != null && localConnection != null)
        {
            int gold = EconomyManager.Instance.GetPlayerGold(localConnection);
            goldText.text = $"Gold: {gold}";
        }
    }
}