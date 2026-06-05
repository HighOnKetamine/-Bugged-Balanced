using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class ExperienceComponent : NetworkBehaviour
{
    [Header("XP Settings")]
    [SerializeField] private int baseXpToLevel = 100;
    [SerializeField] private int xpPerLevel = 50;
    [SerializeField] private float levelScalingMultiplier = 1.08f;

    public readonly SyncVar<int> Level = new SyncVar<int>(1);
    public readonly SyncVar<int> CurrentXP = new SyncVar<int>(0);
    public readonly SyncVar<int> XPToNextLevel = new SyncVar<int>(100);

    public event Action<int, int, int> OnXPChanged;  // xp, xpToNext, level
    public event Action<int> OnLevelChanged;

    private CharacterStats _stats;

    private void Awake()
    {
        _stats = GetComponent<CharacterStats>();
        if (_stats == null)
            Debug.LogError($"[ExperienceComponent] No CharacterStats found on {gameObject.name}!");
        CurrentXP.OnChange += HandleXPChanged;
        Level.OnChange += HandleLevelChanged;
        XPToNextLevel.OnChange += HandleXPToNextLevelChanged;
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        if (!IsServerInitialized) return;
        if (Level.Value <= 0) Level.Value = 1;
        if (XPToNextLevel.Value <= 0) XPToNextLevel.Value = CalculateXPForLevel(Level.Value);
        if (_stats != null) _stats.SetLevel(Level.Value);
    }

    public void AwardExperience(int amount)
    {
        if (amount <= 0) return;
        CurrentXP.Value += amount;
        Debug.Log($"[ExperienceComponent] {gameObject.name} gained {amount} XP. ({CurrentXP.Value}/{XPToNextLevel.Value})");
        TryLevelUp();
    }

    private void TryLevelUp()
    {
        while (CurrentXP.Value >= XPToNextLevel.Value)
        {
            CurrentXP.Value -= XPToNextLevel.Value;
            Level.Value++;
            XPToNextLevel.Value = CalculateXPForLevel(Level.Value);
            if (_stats != null) _stats.SetLevel(Level.Value);
            Debug.Log($"[ExperienceComponent] {gameObject.name} reached level {Level.Value}.");
        }
    }

    private int CalculateXPForLevel(int level)
    {
        int xp = baseXpToLevel + xpPerLevel * (level - 1);
        return Mathf.Max(1, Mathf.RoundToInt(xp * Mathf.Pow(levelScalingMultiplier, level - 1)));
    }

    private void HandleXPChanged(int oldValue, int newValue, bool asServer)
        => OnXPChanged?.Invoke(newValue, XPToNextLevel.Value, Level.Value);

    private void HandleLevelChanged(int oldValue, int newValue, bool asServer)
    {
        OnLevelChanged?.Invoke(newValue);
        if (!asServer && _stats != null) _stats.SetLevel(newValue);
        OnXPChanged?.Invoke(CurrentXP.Value, XPToNextLevel.Value, newValue);
    }

    private void HandleXPToNextLevelChanged(int oldValue, int newValue, bool asServer)
        => OnXPChanged?.Invoke(CurrentXP.Value, newValue, Level.Value);
}