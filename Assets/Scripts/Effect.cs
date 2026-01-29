using System;

public abstract class Effect
{
    // Duration in seconds
    protected float duration;

    protected float interval;

    protected float started;

    protected uint stacks;

    protected uint maxStacks;

    // Variable to store damage, healing or buff
    protected float value;

    public bool CanStack => maxStacks > 1;

    public bool IsActive => started > 0;

    public uint Stacks => stacks;


    // Sends param of duration left and stacks left
    public event Action<float, uint> OnTick;
    public event Action OnStart;
    public event Action OnEnd;

    public Effect(float duration, float value, uint maxStacks = 1)
    {
        if (duration < 0) throw new Exception("Duration cannot be negative!");
        if (maxStacks <= 0) throw new Exception("MaxStacks cannot be zero or negative value!");

        this.duration = duration;
        this.value = value;
        this.maxStacks = maxStacks;
    }

    public abstract void Tick();
}

