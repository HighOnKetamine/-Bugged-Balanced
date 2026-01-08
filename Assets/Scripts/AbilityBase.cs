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

    public float Cooldown => cooldown;
    public float RemainingCooldown => Mathf.Max(0, cooldown - (Time.time - lastCastTime));
    public bool IsOnCooldown => Time.time - lastCastTime < cooldown;
    public KeyCode Hotkey => hotkey;
    public string AbilityName => abilityName;

    protected virtual void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(hotkey))
        {
            TryCastAbility();
        }
    }

    public virtual bool TryCastAbility()
    {
        if (IsOnCooldown)
        {
            Debug.Log($"{abilityName} is on cooldown! {RemainingCooldown:F1}s remaining");
            return false;
        }

        if (CanCast())
        {
            lastCastTime = Time.time;
            CastAbility();
            return true;
        }

        return false;
    }

    protected virtual bool CanCast()
    {
        return true;
    }

    protected abstract void CastAbility();
}

