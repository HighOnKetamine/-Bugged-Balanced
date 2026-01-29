using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public enum TeamId
{
    Neutral = 0,
    Blue = 1,
    Red = 2
}

/// <summary>
/// Component to assign entities to teams. Can be attached to players, minions, towers, and bases.
/// </summary>
public class TeamComponent : NetworkBehaviour
{
    [Header("Team Settings")]
    [Tooltip("Set the initial team in the Inspector. This will be applied when the object spawns.")]
    [SerializeField] private TeamId initialTeam = TeamId.Neutral;

    [SerializeField]
    private readonly SyncVar<TeamId> team = new SyncVar<TeamId>(new SyncTypeSettings(WritePermission.ServerOnly, ReadPermission.Observers));

    public TeamId Team
    {
        get => team.Value;
        set
        {
            if (IsServerInitialized)
                team.Value = value;
        }
    }

    // Track which teams have attacked this neutral entity (server-only)
    private bool provokedByBlue = false;
    private bool provokedByRed = false;

    /// <summary>
    /// Call this when a neutral entity is attacked to make it hostile toward that team
    /// </summary>
    [Server]
    public void ProvokeByTeam(TeamId attackerTeam)
    {
        if (team.Value != TeamId.Neutral) return;

        if (attackerTeam == TeamId.Blue)
        {
            provokedByBlue = true;
            Debug.Log($"[TeamComponent] {gameObject.name} provoked by Blue team");
        }
        else if (attackerTeam == TeamId.Red)
        {
            provokedByRed = true;
            Debug.Log($"[TeamComponent] {gameObject.name} provoked by Red team");
        }
    }

    /// <summary>
    /// Check if this entity is an enemy of another entity.
    /// Neutral entities can be attacked by anyone, but only attack back if provoked.
    /// </summary>
    public bool IsEnemyOf(TeamComponent other)
    {
        if (other == null) return false;
        
        // If OTHER is Neutral, we can always attack them (neutrals are attackable by all)
        if (other.team.Value == TeamId.Neutral)
        {
            Debug.Log($"[TeamComponent] {gameObject.name}({team.Value}) vs {other.gameObject.name}({other.team.Value}) = true (Neutral can be attacked)");
            return true;
        }

        // If WE are Neutral, only attack if provoked by that team
        if (team.Value == TeamId.Neutral)
        {
            bool isProvoked = (other.team.Value == TeamId.Blue && provokedByBlue) ||
                             (other.team.Value == TeamId.Red && provokedByRed);
            
            Debug.Log($"[TeamComponent] {gameObject.name}({team.Value}) vs {other.gameObject.name}({other.team.Value}) = {isProvoked} (Neutral provoked={isProvoked})");
            return isProvoked;
        }
        
        // Non-neutral teams: enemies if different teams
        bool result = team.Value != other.team.Value;
        Debug.Log($"[TeamComponent] {gameObject.name}({team.Value}) vs {other.gameObject.name}({other.team.Value}) = {result}");
        return result;
    }

    /// <summary>
    /// Check if this entity is an ally of another entity.
    /// Neutral entities are NOT allies of anyone.
    /// </summary>
    public bool IsAllyOf(TeamComponent other)
    {
        if (other == null) return false;
        
        // Neutrals are never allies
        if (team.Value == TeamId.Neutral || other.team.Value == TeamId.Neutral) return false;
        
        return team.Value == other.team.Value;
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        
        // Apply the initial team value set in the Inspector
        if (IsServerInitialized)
        {
            team.Value = initialTeam;
            Debug.Log($"[TeamComponent] {gameObject.name} initialized with team: {initialTeam}");
        }
    }
}
