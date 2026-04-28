using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

/// <summary>
/// Manages all active <see cref="Effect"/> instances on a unit.
/// Apply, tick, and removal are server-side only — clients never run effect logic directly.
/// </summary>
public class EffectComponent : NetworkBehaviour
{
     private readonly List<Effect> _activeEffects = new List<Effect>();
     private readonly List<Effect> _toRemove = new List<Effect>();

     /// <summary>
     /// Applies an effect to this unit. If the same effect type is already active,
     /// adds a stack instead of creating a duplicate. Server only.
     /// </summary>
     [Server]
     public void ApplyEffect(Effect effect)
     {
          var existing = _activeEffects.Find(e => e.GetType() == effect.GetType());

          if (existing != null)
          {
               if (existing.CanStack)
                    existing.AddStack();
               return;
          }

          effect.OnEnd += () => _toRemove.Add(effect);
          effect.Start();
          _activeEffects.Add(effect);
     }

     private void Update()
     {
          if (!IsServerInitialized) return;

          foreach (var effect in _activeEffects)
               effect.Tick(Time.deltaTime);

          foreach (var expired in _toRemove)
               _activeEffects.Remove(expired);

          _toRemove.Clear();
     }
}