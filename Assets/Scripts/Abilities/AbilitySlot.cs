using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilitySlot : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image cooldownOverlay;  // dark radial fill
    [SerializeField] private TextMeshProUGUI hotkeyText;
    [SerializeField] private TextMeshProUGUI cooldownText;

    private AbilityBase _ability;

    public void Initialize(AbilityBase ability, Sprite icon)
    {
        _ability = ability;

        if (iconImage != null && icon != null)
            iconImage.sprite = icon;

        if (hotkeyText != null)
            hotkeyText.text = ability.Hotkey.ToString();

        if (cooldownOverlay != null)
        {
            cooldownOverlay.fillMethod = Image.FillMethod.Radial360;
            cooldownOverlay.fillOrigin = (int)Image.Origin360.Top;
            cooldownOverlay.fillClockwise = false;
            cooldownOverlay.fillAmount = 0f;
        }
    }

    private void Update()
    {
        if (_ability == null) return;

        bool onCooldown = _ability.IsOnCooldown;

        if (cooldownOverlay != null)
            cooldownOverlay.fillAmount = onCooldown
                ? 1f - _ability.CooldownProgress
                : 0f;

        if (cooldownText != null)
        {
            cooldownText.gameObject.SetActive(onCooldown);
            if (onCooldown)
                cooldownText.text = Mathf.CeilToInt(_ability.RemainingCooldown).ToString();
        }
    }
}