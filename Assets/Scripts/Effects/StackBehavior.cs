/// <summary>
/// Defines how stacks behave over time on an <see cref="Effect"/>.
/// </summary>
public enum StackBehavior
{
    /// <summary>Adding a stack refreshes the full duration. All stacks expire together.</summary>
    RefreshDuration,
    /// <summary>Each stack has its own independent duration and decays one by one.</summary>
    IndependentDecay
}