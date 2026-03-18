using UnityEngine;

public class TeamComponent : MonoBehaviour
{
    public const sbyte Neutral = -1;

    public sbyte teamId;

    public bool IsEnemy(TeamComponent other)
    {
        if (other == null) return false;
        if (teamId == Neutral || other.teamId == Neutral) return true;
        return teamId != other.teamId;
    }

    public bool IsAlly(TeamComponent other)
    {
        if (other == null) return false;
        if (teamId == Neutral || other.teamId == Neutral) return false;
        return teamId == other.teamId;
    }
}