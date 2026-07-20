using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image icon;

    public ItemData Item { get; private set; }
    public event Action<ItemData> OnClicked;
    public event Action<ItemData, InventorySlotUI> OnHoverEnter;
    public event Action<InventorySlotUI> OnHoverExit;

    public void SetItem(ItemData item)
    {
        Item = item;
        if (icon == null) return;
        icon.sprite = item.icon;
        icon.enabled = true;
    }

    public void SetEmpty()
    {
        Item = null;
        if (icon == null) return;
        icon.sprite = null;
        icon.enabled = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Item != null)
            OnClicked?.Invoke(Item);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Item != null)
            OnHoverEnter?.Invoke(Item, this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnHoverExit?.Invoke(this);
    }
}
