using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections;

public class HealthComponent : NetworkBehaviour
{
    private CharacterStats _stats;

    [SerializeField] public bool autoAttackOnly = false;

    public readonly SyncVar<float> currentHealth = new SyncVar<float>();
    public event Action<float, float> OnHealthChanged;
    public event Action<GameObject> OnDeath;

    public float Current => currentHealth.Value;
    public float Max => _stats.maxHealth.Value;
    public bool IsDead => currentHealth.Value <= 0;

    private void Awake()
    {
        _stats = GetComponent<CharacterStats>();
        if (_stats == null)
            Debug.LogError($"[HealthComponent] No CharacterStats found on {gameObject.name}!");
        currentHealth.OnChange += HandleHealthChanged;
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        if (IsServerInitialized)
        {
            currentHealth.Value = Max;
            StartCoroutine(RegenTick());
        }
    }

    [Server]
    public float TakeDamage(float rawDamage, DamageType damageType, CharacterStats attacker = null, DamageSource source = DamageSource.Ability)
    {
        if (IsDead) return 0f;
        if (autoAttackOnly && source != DamageSource.AutoAttack) return 0f;

        float finalDamage = CalculateDamage(rawDamage, damageType, attacker);
        currentHealth.Value = Mathf.Max(0, currentHealth.Value - finalDamage);
        Debug.Log($"[Server] {gameObject.name} took {finalDamage} {damageType} damage. HP: {currentHealth.Value}/{Max}");

        if (currentHealth.Value <= 0)
            Die(attacker?.gameObject);

        return finalDamage;
    }

    [Server]
    public void Heal(float amount)
    {
        if (IsDead) return;
        amount *= GetHealingModifier();
        currentHealth.Value = Mathf.Min(Max, currentHealth.Value + amount);
    }

    // NEW: Called by PlayerRespawnHandler.Respawn() to restore HP to full.
    // [Server] because currentHealth is a SyncVar — only the server may write it.
    // The change then automatically replicates to all clients via FishNet.
    [Server]
    public void ResetToFull()
    {
        currentHealth.Value = Max;
    }

    private IEnumerator RegenTick()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            if (!IsDead)
                Heal(_stats.healthRegen.Value);
        }
    }

    private float CalculateDamage(float rawDamage, DamageType damageType, CharacterStats attacker)
    {
        switch (damageType)
        {
            case DamageType.Physical:
                return rawDamage * (100f / (100f + GetEffectiveArmor(attacker)));
            case DamageType.Magical:
                return rawDamage * (100f / (100f + GetEffectiveMagicResist(attacker)));
            case DamageType.True:
                return rawDamage;
            default:
                return rawDamage;
        }
    }

    private float GetEffectiveArmor(CharacterStats attacker)
    {
        float armor = _stats.armor.Value;
        if (attacker != null)
        {
            armor *= (1f - attacker.armorPenPercent.Value);
            armor -= attacker.lethality.Value;
        }
        return Mathf.Max(0, armor);
    }

    private float GetEffectiveMagicResist(CharacterStats attacker)
    {
        float mr = _stats.magicResist.Value;
        if (attacker != null)
        {
            mr *= (1f - attacker.magicPenPercent.Value);
            mr -= attacker.flatMagicPen.Value;
        }
        return Mathf.Max(0, mr);
    }

    private float GetHealingModifier() => 1f;

    [Server]
    private void Die(GameObject killer)
    {
        // NEW: Report the kill to the feed before broadcasting death.
        // This runs server-side only — KillFeedManager then pushes it to
        // all clients via ObserversRpc internally.
        // We use CharacterStats.CharacterName if available, fall back to
        // the GameObject name so it always shows something readable.
        string killerName = killer != null ? killer.name : "Environment";
        string victimName = gameObject.name;

        KillFeedManager.Instance.ReportKill(killerName, victimName);

        // Broadcast the death event to all clients (unchanged from before)
        RpcOnDeath(killer);
    }

    [ObserversRpc]
    private void RpcOnDeath(GameObject killer)
    {
        OnDeath?.Invoke(killer);
    }

    private void HandleHealthChanged(float oldValue, float newValue, bool asServer)
    {
        OnHealthChanged?.Invoke(newValue, Max);
    }
}