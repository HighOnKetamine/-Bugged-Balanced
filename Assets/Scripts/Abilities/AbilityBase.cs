using FishNet.Object;
using UnityEngine;

public abstract class AbilityBase : NetworkBehaviour
{
    [Header("Ability Settings")]
    [SerializeField] protected string abilityName;
    [SerializeField] protected float cooldown = 5f;
    [SerializeField] protected float manaCost = 20f;
    [SerializeField] protected KeyCode hotkey = KeyCode.Q;

    protected float lastCastTime = -999f;
    private ManaComponent _mana;
    private PlayerStateMachine _stateMachine;

    public float Cooldown => cooldown;
    public float RemainingCooldown => Mathf.Max(0, cooldown - (Time.time - lastCastTime));
    public float CooldownProgress => Mathf.Clamp01(1f - (RemainingCooldown / cooldown));
    public bool IsOnCooldown => RemainingCooldown > 0f;
    public KeyCode Hotkey => hotkey;
    public string AbilityName => abilityName;
    public float ManaCost => manaCost;

    protected virtual void Awake()
    {
        _mana = GetComponent<ManaComponent>();
        _stateMachine = GetComponent<PlayerStateMachine>();
    }

    public virtual bool TryCastAbility()
    {
        if (IsOnCooldown)
        {
            Debug.Log($"[{abilityName}] On cooldown — {RemainingCooldown:F1}s remaining");
            return false;
        }

        if (!CanCast())
            return false;

        lastCastTime = Time.time;
        CastAbility();
        return true;
    }

    protected virtual bool CanCast()
    {
        if (_mana == null) return true;

        if (_mana.Current < manaCost)
        {
            Debug.Log($"[{abilityName}] Not enough mana — need {manaCost}, have {_mana.Current:F0}");
            return false;
        }

        ConsumeMana();
        return true;
    }

    [ServerRpc]
    private void ConsumeMana()
    {
        _mana?.UseMana(manaCost);
    }

    protected abstract void CastAbility();
}