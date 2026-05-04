using UnityEngine;

public class Lane : MonoBehaviour
{
    [Header("Waypoints — seřadit od základny k nepřátelské základně")]
    public Transform[] waypoints;

    public sbyte teamId;

    public Transform GetWaypoint(int index)
    {
        if (index < 0 || index >= waypoints.Length) return null;
        return waypoints[index];
    }

    public bool IsLastWaypoint(int index) => index >= waypoints.Length - 1;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;
        Gizmos.color = teamId == 0 ? Color.blue : Color.red;
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            if (waypoints[i] != null && waypoints[i + 1] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
        }
    }
#endif
}