using UnityEngine;
using UnityEngine.UI;
using FishNet.Object;
using FishNet.Connection;
using TMPro;
using System.Collections.Generic;

public class InventoryUI : NetworkBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform inventoryContainer;
    [SerializeField] private Transform equippedContainer;
    [SerializeField] private GameObject itemSlotPrefab;

    private NetworkConnection localConnection;

    private void Start()
    {
        if (IsOwner)
        {
            localConnection = LocalConnection;
            inventoryPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (IsOwner && Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }

        if (IsOwner && inventoryPanel.activeSelf)
        {
            RefreshInventory();
        }
    }

    private void ToggleInventory()
    {
        inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        if (inventoryPanel.activeSelf)
        {
            RefreshInventory();
        }
    }

    private void RefreshInventory()
    {
        if (InventoryManager.Instance == null) return;

        // Clear existing items
        foreach (Transform child in inventoryContainer)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in equippedContainer)
        {
            Destroy(child.gameObject);
        }

        // Populate inventory
        List<Item> inventory = InventoryManager.Instance.GetPlayerInventory(localConnection);
        foreach (Item item in inventory)
        {
            GameObject slotObj = Instantiate(itemSlotPrefab, inventoryContainer);
            InventoryItemSlot slot = slotObj.GetComponent<InventoryItemSlot>();
            if (slot != null)
            {
                slot.Setup(item, this, false);
            }
        }

        // Populate equipped items
        List<Item> equipped = InventoryManager.Instance.GetPlayerEquippedItems(localConnection);
        foreach (Item item in equipped)
        {
            GameObject slotObj = Instantiate(itemSlotPrefab, equippedContainer);
            InventoryItemSlot slot = slotObj.GetComponent<InventoryItemSlot>();
            if (slot != null)
            {
                slot.Setup(item, this, true);
            }
        }
    }

    public void EquipItem(Item item)
    {
        if (localConnection != null)
        {
            ServerEquipItem(localConnection, item);
        }
    }

    public void UnequipItem(Item item)
    {
        if (localConnection != null)
        {
            ServerUnequipItem(localConnection, item);
        }
    }

    public void TryUpgradeItem(Item item)
    {
        if (localConnection != null && item.upgradedItem != null)
        {
            ServerTryUpgradeItem(localConnection, item, item.upgradedItem);
        }
    }

    [ServerRpc]
    private void ServerEquipItem(NetworkConnection conn, Item item)
    {
        InventoryManager.Instance.EquipItem(conn, item);
    }

    [ServerRpc]
    private void ServerUnequipItem(NetworkConnection conn, Item item)
    {
        InventoryManager.Instance.UnequipItem(conn, item);
    }

    [ServerRpc]
    private void ServerTryUpgradeItem(NetworkConnection conn, Item baseItem, Item upgradeItem)
    {
        InventoryManager.Instance.TryUpgradeItem(conn, baseItem, upgradeItem);
    }
}