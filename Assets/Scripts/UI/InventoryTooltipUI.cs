using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryTooltipUI : MonoBehaviour
{
    [SerializeField] private RectTransform panel;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI sellText;
    [SerializeField] private Transform statsContainer;
    [SerializeField] private GameObject statLinePrefab;

    private void Awake()
    {
        if (panel != null)
            panel.gameObject.SetActive(false);
    }

    public void Show(ItemData item, RectTransform anchor)
    {
        if (item == null || panel == null || anchor == null) return;

        if (nameText != null) nameText.text = item.itemName;
        if (costText != null) costText.text = $"{item.cost}g";
        if (sellText != null) sellText.text = ItemTooltipFormatting.FormatSellValue(item);

        if (statsContainer != null)
        {
            foreach (Transform child in statsContainer)
                Destroy(child.gameObject);

            if (statLinePrefab != null && item.modifiers != null)
            {
                foreach (ItemModifier modifier in item.modifiers)
                {
                    GameObject line = Instantiate(statLinePrefab, statsContainer);
                    TextMeshProUGUI text = line.GetComponent<TextMeshProUGUI>();
                    if (text != null) text.text = ItemTooltipFormatting.FormatModifier(modifier);
                }
            }
        }

        panel.gameObject.SetActive(true);
        panel.position = anchor.position + new Vector3(0, anchor.rect.height / 2f + 8f, 0);
    }

    public void Hide()
    {
        if (panel != null)
            panel.gameObject.SetActive(false);
    }
}
