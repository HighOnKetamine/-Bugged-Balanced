using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract base class for all status effects. Handles timing, stacking, and interval-based ticking.
/// Subclasses implement <see cref="Apply"/> to define the actual effect logic.
/// </summary>
public abstract class Effect
{
    protected float duration;
    protected float interval;
    protected float elapsed;
    protected float intervalElapsed;
    protected uint stacks;
    protected uint maxStacks;
    protected float value;
    protected GameObject target;

    private readonly List<float> _stackElapsed = new List<float>();
    private readonly StackBehavior _stackBehavior;
    protected StackBehavior StackBehavior => _stackBehavior;

    /// <summary>True if the effect can be applied more than once on the same target.</summary>
    public bool CanStack => maxStacks > 1;

    /// <summary>True if the effect has been started and has not yet expired.</summary>
    public bool IsActive => elapsed >= 0f && elapsed < duration;

    /// <summary>True if the effect duration has fully elapsed.</summary>
    public bool IsExpired => elapsed >= duration;

    /// <summary>Current number of stacks on this effect instance.</summary>
    public uint Stacks => stacks;

    /// <summary>Fired on each interval tick. Params: remaining duration, current stacks.</summary>
    public event Action<float, uint> OnTick;

    /// <summary>Fired once when the effect is first started via <see cref="Start"/>.</summary>
    public event Action OnStart;

    /// <summary>Fired once when the effect duration expires.</summary>
    public event Action OnEnd;

    /// <param name="target">The unit this effect is applied to.</param>
    /// <param name="duration">How long the effect lasts in seconds.</param>
    /// <param name="value">Generic magnitude.</param>
    /// <param name="maxStacks">Maximum number of times this effect can stack.</param>
    /// <param name="interval">How often <see cref="Apply"/> is called, in seconds.</param>
    /// <param name="stackBehavior">Whether stacks refresh the shared duration or decay independently.</param>
    public Effect(
        GameObject target,
        float duration,
        float value,
        uint maxStacks = 1,
        float interval = 1f,
        StackBehavior stackBehavior = StackBehavior.RefreshDuration
    )
    {
        if (target == null) throw new Exception("Target cannot be null!");
        if (duration < 0) throw new Exception("Duration cannot be negative!");
        if (maxStacks <= 0) throw new Exception("MaxStacks cannot be zero or negative!");

        this.target = target;
        this.duration = duration;
        this.value = value;
        this.maxStacks = maxStacks;
        this.interval = interval;
        this.elapsed = -1f;
        _stackBehavior = stackBehavior;
    }

    /// <summary>Activates the effect.</summary>
    public void Start()
    {
        elapsed = 0f;
        stacks = 1;
        _stackElapsed.Clear();
        _stackElapsed.Add(0f);
        OnStart?.Invoke();
    }

    /// <summary>Adds a stack.</summary>
    public void AddStack()
    {
        if (!CanStack || stacks >= maxStacks) return;
        stacks++;

        if (_stackBehavior == StackBehavior.RefreshDuration)
            elapsed = 0f;
        else
            _stackElapsed.Add(0f);
    }

    /// <summary>Advances the effect by deltaTime seconds.</summary>
    public void Tick(float deltaTime)
    {
        if (!IsActive && _stackBehavior == StackBehavior.RefreshDuration) return;
        if (stacks == 0) return;

        if (_stackBehavior == StackBehavior.IndependentDecay)
        {
            for (int i = _stackElapsed.Count - 1; i >= 0; i--)
            {
                _stackElapsed[i] += deltaTime;
                if (_stackElapsed[i] >= duration)
                {
                    _stackElapsed.RemoveAt(i);
                    stacks--;
                }
            }

            intervalElapsed += deltaTime;
            if (intervalElapsed >= interval)
            {
                intervalElapsed -= interval;
                float remaining = _stackElapsed.Count > 0 ? duration - _stackElapsed[0] : 0f;
                OnTick?.Invoke(remaining, stacks);
                Apply();
            }

            if (stacks == 0)
            {
                OnEnd?.Invoke();
                stacks = uint.MaxValue; // prevent re-firing
            }
        }
        else
        {
            elapsed += deltaTime;
            intervalElapsed += deltaTime;

            if (intervalElapsed >= interval)
            {
                intervalElapsed -= interval;
                OnTick?.Invoke(duration - elapsed, stacks);
                Apply();
            }

            if (IsExpired)
            {
                OnEnd?.Invoke();
                elapsed = float.MaxValue; // prevent re-firing
            }
        }
    }

    /// <summary>Defines the actual effect behavior, called on each interval tick.</summary>
    protected abstract void Apply();
}