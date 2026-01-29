using UnityEngine;

/// <summary>
/// Marker component for team spawn points.
/// Used by RespawnManager to determine where players respawn.
/// </summary>
public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private TeamId team;
    
    public TeamId Team => team;

    private void OnDrawGizmos()
    {
        Gizmos.color = team == TeamId.Blue ? Color.blue : Color.red;
        Gizmos.DrawWireSphere(transform.position, 1f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 2f);
    }
}
