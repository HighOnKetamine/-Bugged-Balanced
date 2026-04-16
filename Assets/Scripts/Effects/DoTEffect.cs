using System;
using UnityEngine;

/// <summary>
/// A damage-over-time effect. Deals <c>value</c> damage to the target's <see cref="HealthComponent"/>
/// on each interval tick. Supports stacking — each stack adds a full <c>value</c> instance of damage per tick.
/// </summary>
public class DoTEffect : Effect
{
    private readonly HealthComponent _health;
    private readonly DamageType _damageType;

    /// <param name="target">The unit to damage.</param>
    /// <param name="duration">How long the DoT lasts in seconds.</param>
    /// <param name="damagePerTick">Damage dealt per interval tick, per stack.</param>
    /// <param name="damageType">Physical, Magical, or True damage.</param>
    /// <param name="maxStacks">Maximum stacks allowed on one target.</param>
    /// <param name="interval">How often damage is applied, in seconds.</param>
    /// <param name="stackBehavior">Whether stacks refresh the shared duration or decay independently.</param>
    public DoTEffect(
        GameObject target,
        float duration,
        float damagePerTick,
        DamageType damageType = DamageType.Physical,
        uint maxStacks = 1,
        float interval = 1f,
        StackBehavior stackBehavior = StackBehavior.RefreshDuration
    ) : base(target, duration, damagePerTick, maxStacks, interval, stackBehavior)
    {
        _health = target.GetComponent<HealthComponent>();
        _damageType = damageType;

        if (_health == null)
            throw new Exception($"DoTEffect: target {target.name} has no HealthComponent!");
    }

    protected override void Apply()
    {
        if (_health.IsDead) return;
        _health.TakeDamage(this.StackBehavior == StackBehavior.RefreshDuration ? value * stacks : value, _damageType);
    }
}