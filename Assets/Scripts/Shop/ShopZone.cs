using FishNet.Object;
using UnityEngine;

/// <summary>
/// Place on any GameObject with a Collider to define a shop area.
/// When the LOCAL player walks in, ShopUI unlocks; walking out locks it again.
///
/// Setup in the Inspector:
///   • Add a BoxCollider (or any Collider) — isTrigger is forced on in Awake.
///   • Set allowedTeamIds to restrict which teams can shop here.
///     Leave the array empty to allow ALL teams.
///   • A kinematic Rigidbody is added automatically so trigger events fire
///     against the player's CapsuleCollider (which has no Rigidbody).
/// </summary>
[RequireComponent(typeof(Collider))]
public class ShopZone : MonoBehaviour
{
    [Tooltip("Team IDs that may use this zone. Leave empty to allow all teams.")]
    [SerializeField] private sbyte[] allowedTeamIds;

    // How many ShopZone triggers the local player is currently inside.
    // Using a counter instead of a bool handles overlapping zones correctly.
    private static int _localInsideCount;
    public static bool LocalPlayerInShop => _localInsideCount > 0;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;

        // Unity only fires trigger events when at least one participant has a
        // Rigidbody. Players use NavMeshAgent (no Rigidbody), so we add one here.
        if (!TryGetComponent<Rigidbody>(out _))
        {
            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    private void OnDestroy()
    {
        // Reset on scene unload so stale counts don't persist.
        _localInsideCount = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!GetLocalOwner(other, out NetworkObject nob)) return;
        if (!IsTeamAllowed(nob)) return;
        _localInsideCount++;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!GetLocalOwner(other, out NetworkObject nob)) return;
        if (!IsTeamAllowed(nob)) return;
        _localInsideCount = Mathf.Max(0, _localInsideCount - 1);
    }

    // Returns true only if the collider belongs to the LOCAL player's object.
    private static bool GetLocalOwner(Collider col, out NetworkObject nob)
    {
        nob = col.GetComponentInParent<NetworkObject>();
        return nob != null && nob.IsOwner;
    }

    private bool IsTeamAllowed(NetworkObject nob)
    {
        if (allowedTeamIds == null || allowedTeamIds.Length == 0) return true;
        TeamComponent team = nob.GetComponent<TeamComponent>();
        if (team == null) return true;
        foreach (sbyte id in allowedTeamIds)
            if (id == team.teamId.Value) return true;
        return false;
    }
}
