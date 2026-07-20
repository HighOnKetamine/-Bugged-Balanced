using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopFilterButtonUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private Button button;
    [SerializeField] private GameObject selectedHighlight;

    public event Action OnClicked;

    private void Awake()
    {
        if (button != null)
            button.onClick.AddListener(() => OnClicked?.Invoke());
    }

    public void Setup(string text)
    {
        if (label != null) label.text = text;
        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        if (selectedHighlight != null)
            selectedHighlight.SetActive(selected);
    }
}
