using FishNet.Connection;
using FishNet.Object;
using FishNet.Observing;
using UnityEngine;

/// <summary>
/// FishNet observer condition for fog-of-war.
///
/// Add this condition to the NetworkObserver on player and minion prefabs.
/// Towers should NOT use it (towers are always visible).
///
/// Create via: Assets → Create → FishNet → Observers → FogOfWar Condition
/// Then drag the created asset into the NetworkObserver's Conditions list on
/// the prefab.
///
/// FishNet re-evaluates Timed conditions on its internal observer tick (default
/// ~0.25 s via ObserverManager).  Units that leave vision are despawned from
/// the enemy client entirely — no AI, no position updates, no rendering.
/// </summary>
[CreateAssetMenu(menuName = "FishNet/Observers/FogOfWar Condition", fileName = "FogOfWarCondition")]
public class FogOfWarCondition : ObserverCondition
{
    public override bool ConditionMet(NetworkConnection connection, bool currentlyAdded, out bool notProcessed)
    {
        notProcessed = false;

        // The owner of an object always sees it (e.g. the player sees their own character).
        if (NetworkObject.Owner == connection)
            return true;

        // Look up which team this connection belongs to.
        sbyte connTeam = NetworkGameManager.Instance != null
            ? NetworkGameManager.Instance.GetClientTeam(connection.ClientId)
            : TeamComponent.Neutral;

        // Team unknown (pre-game / spectator) → show everything.
        if (connTeam == TeamComponent.Neutral)
            return true;

        // Get the team of the observed object.
        var tc = NetworkObject.GetComponent<TeamComponent>();
        sbyte objectTeam = tc != null ? tc.teamId.Value : TeamComponent.Neutral;

        // Same team or neutral object → always visible.
        if (objectTeam == connTeam || objectTeam == TeamComponent.Neutral)
            return true;

        // Enemy: only visible if the client's team has a vision source in range.
        if (ServerVisionTracker.Instance == null)
            return true;  // tracker not ready — fail open

        return ServerVisionTracker.Instance.CanSee(connTeam, NetworkObject.transform.position);
    }

    /// <summary>Timed so FishNet re-evaluates periodically as units move.</summary>
    public override ObserverConditionType GetConditionType() => ObserverConditionType.Timed;
}
