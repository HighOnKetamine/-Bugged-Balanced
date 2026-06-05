using TMPro;
using UnityEngine;

public class PlayerHUD : MonoBehaviour
{
    [Header("Player HUD")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI xpText;
    [SerializeField] private RectTransform xpBarFill;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI killsText;
    [SerializeField] private TextMeshProUGUI deathsText;

    private ExperienceComponent _experience;
    private PlayerScoreComponent _score;
    private GoldComponent _gold;

    public void Initialize(GameObject playerObject)
    {
        _experience = playerObject.GetComponent<ExperienceComponent>();
        _score = playerObject.GetComponent<PlayerScoreComponent>();
        _gold = playerObject.GetComponent<GoldComponent>();

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
        if (_experience != null)
        {
            _experience.OnXPChanged -= RefreshExperience;
            _experience.OnLevelChanged -= RefreshLevel;
        }

        if (_score != null)
            _score.OnScoreChanged -= RefreshScore;

        if (_gold != null)
            _gold.OnGoldChanged -= RefreshGold;
    }

    private void RefreshLevel(int level)
    {
        if (levelText != null)
            levelText.text = $"Lvl {level}";
    }

    private void RefreshExperience(int currentXp, int xpToNext, int level)
    {
        if (xpText != null)
            xpText.text = $"XP: {currentXp}/{xpToNext}";

        if (xpBarFill != null)
            xpBarFill.localScale = new Vector3(xpToNext <= 0 ? 0f : Mathf.Clamp01((float)currentXp / xpToNext), 1f, 1f);

        if (levelText != null)
            levelText.text = $"Lvl {level}";
    }

    private void RefreshGold(int gold)
    {
        if (goldText != null)
            goldText.text = $"Gold: {gold}";
    }

    private void RefreshScore(int kills, int deaths)
    {
        if (killsText != null)
            killsText.text = $"Kills: {kills}";

        if (deathsText != null)
            deathsText.text = $"Deaths: {deaths}";
    }
}
