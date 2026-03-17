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

     public float CurrentHealth => currentHealth.Value;
     public float MaxHealth => maxHealth;
     public bool IsDead => currentHealth.Value <= 0;

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
     public void TakeDamage(float damage, GameObject attacker = null)
     {
          if (IsDead) return;
          
          Debug.Log($"[Server] {gameObject.name} took {damage} damage. Old Health: {currentHealth.Value}");

          currentHealth.Value = Mathf.Max(0, currentHealth.Value - damage);

          if (currentHealth.Value <= 0)
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
