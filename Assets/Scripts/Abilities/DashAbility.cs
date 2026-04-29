using UnityEngine;
using UnityEngine.AI;
using FishNet.Object;

public class DashAbility : SkillshotAbility
{
    [Header("Dash")]
    [SerializeField] private float dashDistance = 5f;

    [ServerRpc]
    protected override void ServerCast(Vector3 origin, Vector3 direction)
    {
        Vector3 destination = transform.position + direction * dashDistance;

        if (NavMesh.SamplePosition(destination, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            GetComponent<NavMeshAgent>().Warp(hit.position);
        else
            GetComponent<NavMeshAgent>().Warp(destination);
    }
}