using FishNet.Object;
using UnityEngine;

/// <summary>
/// Receives kill events from HealthComponent and broadcasts them to all clients.
///
/// WHY a separate manager and not directly in HealthComponent?
/// HealthComponent knows about HP — it shouldn't also know about UI layout,
/// font sizes, or how long to display a kill banner. Separation of concerns.
///
/// Hook: HealthComponent.OnDeath → KillFeedManager.ReportKill()
/// </summary>
public class KillFeedManager : NetworkBehaviour
{
    public static KillFeedManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// Call this from wherever you detect a kill (HealthComponent, DeathState, etc.).
    /// killerName / victimName — use the champion name or player display name.
    /// </summary>
    [Server]
    public void ReportKill(string killerName, string victimName)
    {
        // Push to all clients — they each have a KillFeedUI listening
        BroadcastKillObservers(killerName, victimName);
    }

    [ObserversRpc]
    private void BroadcastKillObservers(string killerName, string victimName)
    {
        KillFeedUI.Instance?.ShowKill(killerName, victimName);
    }
}