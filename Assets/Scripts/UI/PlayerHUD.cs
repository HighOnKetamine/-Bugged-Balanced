using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private Image hpBarFill;
    [SerializeField] private TextMeshProUGUI hpText;

    [Header("Mana")]
    [SerializeField] private Image manaBarFill;
    [SerializeField] private TextMeshProUGUI manaText;

    [Header("Experience")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI xpText;
    [SerializeField] private Image xpBarFill;

    [Header("Stats")]
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI killsText;
    [SerializeField] private TextMeshProUGUI deathsText;

    [Header("Panel")]
    [SerializeField] private GameObject hudPanel;

    private HealthComponent _health;
    private ManaComponent _mana;
    private ExperienceComponent _experience;
    private PlayerScoreComponent _score;
    private GoldComponent _gold;

    private void Awake()
    {
        if (hudPanel != null) hudPanel.SetActive(false);
        else gameObject.SetActive(false);
    }

    public void Initialize(GameObject playerObject)
    {
        if (hudPanel != null) hudPanel.SetActive(true);
        else gameObject.SetActive(true);

        _health = playerObject.GetComponent<HealthComponent>();
        _mana = playerObject.GetComponent<ManaComponent>();
        _experience = playerObject.GetComponent<ExperienceComponent>();
        _score = playerObject.GetComponent<PlayerScoreComponent>();
        _gold = playerObject.GetComponent<GoldComponent>();

        if (_health != null)
        {
            _health.OnHealthChanged += RefreshHealth;
            RefreshHealth(_health.Current, _health.Max);
        }

        if (_mana != null)
        {
            _mana.OnManaChanged += RefreshMana;
            RefreshMana(_mana.Current, _mana.Max);
        }

        if (_experience != null)
        {
            _experience.OnXPChanged += RefreshExperience;
            _experience.OnLevelChanged += RefreshLevel;
            RefreshLevel(_experience.Level.Value);
            RefreshExperience(_experience.CurrentXP.Value, _experience.XPToNextLevel.Value, _experience.Level.Value);
        }

        if (_score != null)
        {
            _score.OnScoreChanged += RefreshScore;
            RefreshScore(_score.Kills.Value, _score.Deaths.Value);
        }

        if (_gold != null)
        {
            _gold.OnGoldChanged += RefreshGold;
            RefreshGold(_gold.Gold.Value);
        }
    }

    private void OnDestroy()
    {
        if (_health != null) _health.OnHealthChanged -= RefreshHealth;
        if (_mana != null) _mana.OnManaChanged -= RefreshMana;
        if (_experience != null)
        {
            _experience.OnXPChanged -= RefreshExperience;
            _experience.OnLevelChanged -= RefreshLevel;
        }
        if (_score != null) _score.OnScoreChanged -= RefreshScore;
        if (_gold != null) _gold.OnGoldChanged -= RefreshGold;
    }

    private void RefreshHealth(float current, float max)
    {
        if (hpBarFill != null)
            hpBarFill.fillAmount = max <= 0 ? 0f : Mathf.Clamp01(current / max);
        if (hpText != null)
            hpText.text = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
    }

    private void RefreshMana(float current, float max)
    {
        if (manaBarFill != null)
            manaBarFill.fillAmount = max <= 0 ? 0f : Mathf.Clamp01(current / max);
        if (manaText != null)
            manaText.text = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
    }

    private void RefreshLevel(int level)
    {
        if (levelText != null)
            levelText.text = $"Lvl {level}";
    }

    private void RefreshExperience(int currentXp, int xpToNext, int level)
    {
        if (xpText != null)
            xpText.text = $"{currentXp}/{xpToNext}";
        if (xpBarFill != null)
            xpBarFill.fillAmount = xpToNext <= 0 ? 0f : Mathf.Clamp01((float)currentXp / xpToNext);
        if (levelText != null)
            levelText.text = $"Lvl {level}";
    }

    private void RefreshGold(int gold)
    {
        if (goldText != null)
            goldText.text = $"{gold}";
    }

    private void RefreshScore(int kills, int deaths)
    {
        if (killsText != null)
            killsText.text = $"{kills}";
        if (deathsText != null)
            deathsText.text = $"{deaths}";
    }
}