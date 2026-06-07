using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilitySlot : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image cooldownOverlay;
    [SerializeField] private Image notLearnedOverlay;
    [SerializeField] private TextMeshProUGUI hotkeyText;
    [SerializeField] private TextMeshProUGUI cooldownText;
    [SerializeField] private TextMeshProUGUI levelText;

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

        bool learned = _ability.IsLearned;
        bool onCooldown = _ability.IsOnCooldown;

        if (notLearnedOverlay != null)
            notLearnedOverlay.gameObject.SetActive(!learned);

        if (levelText != null)
            levelText.text = learned
                ? $"{_ability.AbilityLevel.Value}/{_ability.MaxAbilityLevel}"
                : "-";

        if (cooldownOverlay != null)
            cooldownOverlay.fillAmount = (learned && onCooldown)
                ? 1f - _ability.CooldownProgress
                : 0f;

        if (cooldownText != null)
        {
            cooldownText.gameObject.SetActive(learned && onCooldown);
            if (learned && onCooldown)
                cooldownText.text = Mathf.CeilToInt(_ability.RemainingCooldown).ToString();
        }
    }
}