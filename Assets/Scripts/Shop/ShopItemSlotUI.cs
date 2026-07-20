using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemSlotUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private GameObject selectedHighlight;
    [SerializeField] private Button selectButton;

    public ItemData Item { get; private set; }
    public event Action<ItemData> OnClicked;

    private void Awake()
    {
        if (selectButton != null)
            selectButton.onClick.AddListener(() => OnClicked?.Invoke(Item));
    }

    public void Setup(ItemData item)
    {
        Item = item;
        if (icon != null) icon.sprite = item.icon;
        if (nameText != null) nameText.text = item.itemName;
        if (costText != null) costText.text = $"{item.cost}g";
        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        if (selectedHighlight != null)
            selectedHighlight.SetActive(selected);
    }
}
