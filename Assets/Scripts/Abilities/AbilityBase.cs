using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public abstract class AbilityBase : NetworkBehaviour
{
    [Header("Ability Settings")]
    [SerializeField] protected string abilityName;
    [SerializeField] protected KeyCode hotkey = KeyCode.Q;

    [Header("Per-Level Stats (index 0 = level 1)")]
    [SerializeField] private float[] damageLevels = { 60f, 90f, 120f, 150f, 180f };
    [SerializeField] private float[] cooldownLevels = { 10f, 9f, 8f, 7f, 6f };
    [SerializeField] private float[] manaCostLevels = { 50f, 55f, 60f, 65f, 70f };

    [Header("Leveling")]
    [SerializeField] private int maxAbilityLevel = 5;

    public readonly SyncVar<int> AbilityLevel = new SyncVar<int>(0); // 0 = not learned

    protected float lastCastTime = -999f;

    private ManaComponent _mana;

    public KeyCode Hotkey => hotkey;
    public string AbilityName => abilityName;
    public int MaxAbilityLevel => maxAbilityLevel;
    public bool IsLearned => AbilityLevel.Value > 0;
    public bool IsOnCooldown => RemainingCooldown > 0f;
    public float RemainingCooldown => IsLearned
        ? Mathf.Max(0, GetCurrentCooldown() - (Time.time - lastCastTime))
        : 0f;
    public float CooldownProgress => IsLearned
        ? Mathf.Clamp01(1f - (RemainingCooldown / GetCurrentCooldown()))
        : 0f;

    protected virtual void Awake()
    {
        _mana = GetComponent<ManaComponent>();
    }

    protected float GetCurrentDamage()
    {
        int idx = Mathf.Clamp(AbilityLevel.Value - 1, 0, damageLevels.Length - 1);
        return damageLevels[idx];
    }

    protected float GetCurrentCooldown()
    {
        int idx = Mathf.Clamp(AbilityLevel.Value - 1, 0, cooldownLevels.Length - 1);
        return cooldownLevels[idx];
    }

    protected float GetCurrentManaCost()
    {
        int idx = Mathf.Clamp(AbilityLevel.Value - 1, 0, manaCostLevels.Length - 1);
        return manaCostLevels[idx];
    }

    [Server]
    public bool TryLevelUp(ExperienceComponent exp)
    {
        if (AbilityLevel.Value >= maxAbilityLevel) return false;
        if (exp == null) return false;
        int spent = 0;
        foreach (AbilityBase ab in GetComponents<AbilityBase>())
            spent += ab.AbilityLevel.Value;
        if (spent >= exp.Level.Value) return false;
        AbilityLevel.Value++;
        Debug.Log($"[AbilityBase] {abilityName} leveled up to {AbilityLevel.Value}");
        return true;
    }

    public virtual bool TryCastAbility()
    {
        if (!IsLearned)
        {
            Debug.Log($"[{abilityName}] Not learned yet.");
            return false;
        }
        if (IsOnCooldown)
        {
            Debug.Log($"[{abilityName}] On cooldown — {RemainingCooldown:F1}s remaining");
            return false;
        }
        if (!CanCast()) return false;
        lastCastTime = Time.time;
        CastAbility();
        return true;
    }

    protected virtual bool CanCast()
    {
        if (_mana == null) return true;
        float cost = GetCurrentManaCost();
        if (_mana.Current < cost)
        {
            Debug.Log($"[{abilityName}] Not enough mana — need {cost}, have {_mana.Current:F0}");
            return false;
        }
        ConsumeMana();
        return true;
    }

    [ServerRpc]
    private void ConsumeMana() => _mana?.UseMana(GetCurrentManaCost());

    protected abstract void CastAbility();
}