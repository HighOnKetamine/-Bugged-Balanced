using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;

public class HealthSystem : NetworkBehaviour
{
     [Header("Health Settings")]
     [SerializeField] private float maxHealth = 100f;

     [SerializeField]
     public readonly SyncVar<float> currentHealth = new SyncVar<float>();

     public event Action<float, float> OnHealthChangedEvent; // current, max
     public event Action OnDeath;

     private GameObject lastAttacker;

     public float CurrentHealth => currentHealth.Value;
     public float MaxHealth => maxHealth;
     public bool IsDead => currentHealth.Value <= 0;
     public GameObject GetLastAttacker() => lastAttacker;

     private void Awake()
     {
          currentHealth.Value = maxHealth;
          currentHealth.OnChange += OnHealthChanged;
     }

     public override void OnStartNetwork()
     {
          base.OnStartNetwork();
          if (IsServerInitialized)
          {
               currentHealth.Value = maxHealth;
          }
     }

     [Server]
    public void TakeDamage(float damage, GameObject attacker)
    {
        if (IsDead) return;

        // Track last attacker for economy/kill attribution
        lastAttacker = attacker;

        // If we're neutral and got attacked, become hostile to attacker's team
        TeamComponent myTeam = GetComponent<TeamComponent>();
        if (myTeam != null && myTeam.Team == TeamId.Neutral && attacker != null)
        {
            TeamComponent attackerTeam = attacker.GetComponent<TeamComponent>();
            if (attackerTeam != null)
            {
                myTeam.ProvokeByTeam(attackerTeam.Team);
            }
        }

        currentHealth.Value = Mathf.Max(0, currentHealth.Value - damage);
        Debug.Log($"[HealthSystem] {gameObject.name} took {damage} damage. Current health: {currentHealth.Value}/{MaxHealth}");

        if (currentHealth.Value <= 0 && !IsDead)
        {
            Die();
        }
    }

     [Server]
     public void Heal(float amount)
     {
          if (IsDead) return;
          currentHealth.Value = Mathf.Min(maxHealth, currentHealth.Value + amount);
     }

     [Server]
     private void Die()
     {
          RpcOnDeath();
     }

     [ObserversRpc]
     private void RpcOnDeath()
     {
          OnDeath?.Invoke();
     }

     private void OnHealthChanged(float oldValue, float newValue, bool asServer)
     {
          OnHealthChangedEvent?.Invoke(newValue, maxHealth);
     }
}
