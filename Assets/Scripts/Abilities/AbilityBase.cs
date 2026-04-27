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

    // Cooldown queries
    public float Cooldown => cooldown;
    public float RemainingCooldown => Mathf.Max(0, cooldown - (Time.time - lastCastTime));
    public float CooldownProgress => Mathf.Clamp01(1f - (RemainingCooldown / cooldown)); // 0=fresh cooldown, 1=ready
    public bool IsOnCooldown => RemainingCooldown > 0f;

    // Info
    public KeyCode Hotkey => hotkey;
    public string AbilityName => abilityName;
    public float ManaCost => manaCost;

    protected virtual void Awake()
    {
        _mana = GetComponent<ManaComponent>();
        _stateMachine = GetComponent<PlayerStateMachine>();
    }

    protected virtual void Update()
    {
        if (!IsOwner) return;
        if (_stateMachine != null && !_stateMachine.CanCast) return;
        if (Input.GetKeyDown(hotkey))
            TryCastAbility();
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

    // Base check: mana. Subclasses override this and call base.CanCast()
    // so mana is always validated in the chain.
    protected virtual bool CanCast()
    {
        if (_mana == null) return true; // no mana system on this unit — allow cast

        if (_mana.Current < manaCost)
        {
            Debug.Log($"[{abilityName}] Not enough mana — need {manaCost}, have {_mana.Current:F0}");
            return false;
        }

        // Consume here on the owner so the cooldown doesn't start
        // on a failed cast. Server-side consumption happens in ServerCast.
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