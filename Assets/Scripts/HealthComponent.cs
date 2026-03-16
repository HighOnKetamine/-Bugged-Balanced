using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class HealthComponent : NetworkBehaviour
{
    private CharacterStats _stats;

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
            currentHealth.Value = Max;
    }

    [Server]
    public void TakeDamage(float rawDamage, DamageType damageType, CharacterStats attacker)
    {
        if (IsDead) return;

        float finalDamage = CalculateDamage(rawDamage, damageType, attacker);
        currentHealth.Value = Mathf.Max(0, currentHealth.Value - finalDamage);

        Debug.Log($"[Server] {gameObject.name} took {finalDamage} {damageType} damage. HP: {currentHealth.Value}/{Max}");

        if (currentHealth.Value <= 0)
            Die(attacker?.gameObject);
    }

    [Server]
    public void Heal(float amount)
    {
        if (IsDead) return;
        currentHealth.Value = Mathf.Min(Max, currentHealth.Value + amount);
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
            mr *= (1f - attacker.magicPenPercent.Value);   // % pen first
            mr -= attacker.flatMagicPen.Value;              // flat pen second
        }

        return Mathf.Max(0, mr);
    }

    [Server]
    private void Die(GameObject killer)
    {
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