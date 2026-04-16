using System;
using UnityEngine;

/// <summary>
/// Applies a flat or percent stat modifier to a target for a duration.
/// The modifier is applied once on start and removed when the effect ends.
/// </summary>
public class StatModifierEffect : Effect
{
    public enum ModifierType { Flat, Percent }

    private readonly CharacterStats _stats;
    private readonly Func<CharacterStats, Stat> _statSelector;
    private readonly ModifierType _modifierType;

    /// <param name="target">The unit to modify.</param>
    /// <param name="duration">How long the modifier lasts in seconds.</param>
    /// <param name="statSelector">Selects which stat to modify, e.g. <c>stats => stats.moveSpeed</c>.</param>
    /// <param name="value">Amount to add. Use negative values for reductions (e.g. slows).</param>
    /// <param name="modifierType">Whether <paramref name="value"/> is a flat or percent modifier.</param>
    public StatModifierEffect(
        GameObject target,
        float duration,
        Func<CharacterStats, Stat> statSelector,
        float value,
        ModifierType modifierType = ModifierType.Flat
    ) : base(target, duration, value, maxStacks: 1, interval: duration)
    {
        _stats = target.GetComponent<CharacterStats>();
        _statSelector = statSelector;
        _modifierType = modifierType;

        if (_stats == null)
            throw new Exception($"StatModifierEffect: target {target.name} has no CharacterStats!");

        OnStart += ApplyModifier;
        OnEnd += RemoveModifier;
    }

    private void ApplyModifier()
    {
        var stat = _statSelector(_stats);
        if (_modifierType == ModifierType.Flat)
            stat.AddFlat(value);
        else
            stat.AddPercent(value);
    }

    private void RemoveModifier()
    {
        var stat = _statSelector(_stats);
        if (_modifierType == ModifierType.Flat)
            stat.RemoveFlat(value);
        else
            stat.RemovePercent(value);
    }

    protected override void Apply() { }
}