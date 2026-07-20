using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopDetailsPanelUI : MonoBehaviour
{
    [Header("State Roots")]
    [SerializeField] private GameObject emptyState;
    [SerializeField] private GameObject content;

    [Header("Content")]
    [SerializeField] private TextMeshProUGUI nameHeader;
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI flavorText;
    [SerializeField] private Transform statsStackContainer;
    [SerializeField] private GameObject statLinePrefab;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI sellText;
    [SerializeField] private Button buyButton;
    [SerializeField] private TextMeshProUGUI buyButtonLabel;

    public event Action<string> OnBuyClicked;

    private string _currentItemId;

    private void Awake()
    {
        if (buyButton != null)
            buyButton.onClick.AddListener(() => OnBuyClicked?.Invoke(_currentItemId));
    }

    public void ShowEmpty()
    {
        _currentItemId = null;
        if (emptyState != null) emptyState.SetActive(true);
        if (content != null) content.SetActive(false);
    }

    public void Show(ItemData item)
    {
        if (item == null) { ShowEmpty(); return; }

        _currentItemId = item.GetId();
        if (emptyState != null) emptyState.SetActive(false);
        if (content != null) content.SetActive(true);

        if (nameHeader != null) nameHeader.text = item.itemName;
        if (itemImage != null) itemImage.sprite = item.icon;
        if (flavorText != null) flavorText.text = item.flavorText;
        if (priceText != null) priceText.text = $"Price: {item.cost}g";
        if (sellText != null) sellText.text = ItemTooltipFormatting.FormatSellValue(item);

        RebuildStats(item);
    }

    public void SetBuyable(bool canBuy, string reason)
    {
        if (buyButton != null) buyButton.interactable = canBuy;
        if (buyButtonLabel != null) buyButtonLabel.text = canBuy ? "BUY" : (reason ?? "UNAVAILABLE").ToUpperInvariant();
    }

    private void RebuildStats(ItemData item)
    {
        if (statsStackContainer == null) return;

        foreach (Transform child in statsStackContainer)
            Destroy(child.gameObject);

        if (statLinePrefab == null || item.modifiers == null) return;

        foreach (ItemModifier modifier in item.modifiers)
        {
            GameObject line = Instantiate(statLinePrefab, statsStackContainer);
            TextMeshProUGUI text = line.GetComponent<TextMeshProUGUI>();
            if (text != null) text.text = ItemTooltipFormatting.FormatModifier(modifier);
        }
    }
}
