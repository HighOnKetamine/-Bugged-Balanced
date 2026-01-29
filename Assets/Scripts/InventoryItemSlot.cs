using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryItemSlot : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private Button actionButton;
    [SerializeField] private TMP_Text actionButtonText;

    private Item item;
    private InventoryUI inventoryUI;
    private bool isEquipped;

    public void Setup(Item itemData, InventoryUI ui, bool equipped)
    {
        item = itemData;
        inventoryUI = ui;
        isEquipped = equipped;

        itemIcon.sprite = item.icon;
        itemNameText.text = item.itemName;

        if (isEquipped)
        {
            actionButtonText.text = "Unequip";
            actionButton.onClick.AddListener(OnUnequipClicked);
        }
        else
        {
            actionButtonText.text = "Equip";
            actionButton.onClick.AddListener(OnEquipClicked);
        }

        // Add upgrade button if applicable
        if (item.upgradedItem != null)
        {
            // You could add another button for upgrading here
        }
    }

    private void OnEquipClicked()
    {
        if (inventoryUI != null)
        {
            inventoryUI.EquipItem(item);
        }
    }

    private void OnUnequipClicked()
    {
        if (inventoryUI != null)
        {
            inventoryUI.UnequipItem(item);
        }
    }
}