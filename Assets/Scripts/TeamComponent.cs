using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class TeamComponent : NetworkBehaviour
{
    public const sbyte Neutral = -1;

    public readonly SyncVar<sbyte> teamId = new SyncVar<sbyte>();

    public bool IsEnemy(TeamComponent other)
    {
        if (other == null) return false;
        if (teamId.Value == Neutral || other.teamId.Value == Neutral) return true;
        return teamId.Value != other.teamId.Value;
    }

    public bool IsAlly(TeamComponent other)
    {
        if (other == null) return false;
        if (teamId.Value == Neutral || other.teamId.Value == Neutral) return false;
        return teamId.Value == other.teamId.Value;
    }
}