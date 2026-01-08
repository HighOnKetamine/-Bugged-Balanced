using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;

public class HealthSystem : NetworkBehaviour
{
     [Header("Health Settings")]
     [SerializeField] private float maxHealth = 100f;

     [SyncVar(OnChange = nameof(OnHealthChanged))]
     private float currentHealth;

     public event Action<float, float> OnHealthChangedEvent; // current, max
     public event Action OnDeath;

     public float CurrentHealth => currentHealth;
     public float MaxHealth => maxHealth;
     public bool IsDead => currentHealth <= 0;

     private void Awake()
     {
          currentHealth = maxHealth;
     }

     public override void OnStartNetwork()
     {
          base.OnStartNetwork();
          if (IsServer)
          {
               currentHealth = maxHealth;
          }
     }

     [Server]
     public void TakeDamage(float damage, GameObject attacker = null)
     {
          if (IsDead) return;

          currentHealth = Mathf.Max(0, currentHealth - damage);

          if (currentHealth <= 0)
          {
               Die();
          }
     }

     [Server]
     public void Heal(float amount)
     {
          if (IsDead) return;
          currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
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
