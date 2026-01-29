using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemButton : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text itemCostText;
    [SerializeField] private TMP_Text itemDescriptionText;
    [SerializeField] private Button purchaseButton;

    private Item item;
    private ShopUI shopUI;

    public void Setup(Item itemData, ShopUI ui)
    {
        item = itemData;
        shopUI = ui;

        itemIcon.sprite = item.icon;
        itemNameText.text = item.itemName;
        itemCostText.text = $"{item.goldCost} Gold";
        itemDescriptionText.text = item.description;

        purchaseButton.onClick.AddListener(OnPurchaseClicked);
    }

    private void OnPurchaseClicked()
    {
        if (shopUI != null)
        {
            shopUI.TryPurchaseItem(item);
        }
    }
}