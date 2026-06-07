using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public abstract class AbilityBase : NetworkBehaviour
{
    [Header("Ability Settings")]
    [SerializeField] protected string abilityName;
    [SerializeField] protected KeyCode hotkey = KeyCode.Q;
    [SerializeField] private Sprite abilityIcon;

    [Header("Per-Level Stats (index 0 = level 1)")]
    [SerializeField] private float[] damageLevels = { 60f, 90f, 120f, 150f, 180f };
    [SerializeField] private float[] cooldownLevels = { 10f, 9f, 8f, 7f, 6f };
    [SerializeField] private float[] manaCostLevels = { 50f, 55f, 60f, 65f, 70f };

    [Header("Scaling")]
    [SerializeField] private float apRatio = 0.5f;
    [SerializeField] private float adRatio = 0f;

    [Header("Leveling")]
    [SerializeField] private int maxAbilityLevel = 5;

    [Header("Audio / Visual")]
    [SerializeField] private AudioClip castSound;
    [SerializeField] private GameObject castVfxPrefab;

    public readonly SyncVar<int> AbilityLevel = new SyncVar<int>(0);

    protected float lastCastTime = -999f;

    private ManaComponent _mana;

    public KeyCode Hotkey => hotkey;
    public string AbilityName => abilityName;
    public Sprite AbilityIcon => abilityIcon;
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

    protected float GetScaledDamage()
    {
        CharacterStats stats = GetComponent<CharacterStats>();
        float baseDamage = GetCurrentDamage();
        if (stats == null) return baseDamage;
        return baseDamage
            + (stats.abilityPower.Value * apRatio)
            + (stats.attackDamage.Value * adRatio);
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
        PlayCastEffects();
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

    private void PlayCastEffects()
    {
        if (castSound != null)
            AudioSource.PlayClipAtPoint(castSound, transform.position);
        if (castVfxPrefab != null)
        {
            GameObject vfx = Instantiate(castVfxPrefab, transform.position, Quaternion.identity);
            Destroy(vfx, 2f);
        }
    }

    protected virtual GameObject hitVfxPrefab => null;

    [ObserversRpc]
    protected void RpcSpawnHitVfx(Vector3 position)
    {
        if (hitVfxPrefab == null) return;
        GameObject vfx = Instantiate(hitVfxPrefab, position, Quaternion.identity);
        Destroy(vfx, 2f);
    }

    protected abstract void CastAbility();
}