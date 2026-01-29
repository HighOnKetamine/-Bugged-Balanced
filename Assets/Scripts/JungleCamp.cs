using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages a jungle camp with neutral monsters that respawn after being killed.
/// Handles spawn timing, leash behavior, and camp-specific rewards.
/// </summary>
public class JungleCamp : NetworkBehaviour
{
    public enum CampType
    {
        Wolves,
        Golems,
        Dragons,
        Baron
    }

    [Header("Camp Settings")]
    [SerializeField] private CampType campType;
    [SerializeField] private float respawnTime = 30f;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject[] neutralPrefabs; // Monsters to spawn

    [Header("Rewards")]
    [SerializeField] private int goldReward = 50;
    [SerializeField] private int xpReward = 100;
    [SerializeField] private GameObject buffPrefab; // Optional buff to apply on kill

    // Server-only tracking
    private List<GameObject> activeNeutrals = new List<GameObject>();
    private bool isRespawning = false;
    private Coroutine respawnCoroutine;

    public override void OnStartServer()
    {
        base.OnStartServer();

        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }

        // Initial spawn
        SpawnCamp();
    }

    [Server]
    private void SpawnCamp()
    {
        Debug.Log($"[JungleCamp] Spawning {campType} camp at {spawnPoint.position}");

        foreach (GameObject prefab in neutralPrefabs)
        {
            GameObject neutral = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
            ServerManager.Spawn(neutral);

            // Set up neutral
            HealthSystem health = neutral.GetComponent<HealthSystem>();
            if (health != null)
            {
                health.OnDeath += () => OnNeutralDeath(neutral);
            }

            // Set team to Neutral
            TeamComponent team = neutral.GetComponent<TeamComponent>();
            if (team != null)
            {
                team.SetTeam(TeamId.Neutral);
            }

            activeNeutrals.Add(neutral);
        }
    }

    [Server]
    private void OnNeutralDeath(GameObject neutral)
    {
        if (activeNeutrals.Contains(neutral))
        {
            activeNeutrals.Remove(neutral);

            // Award rewards
            AwardCampRewards(neutral);

            // Check if camp is cleared
            if (activeNeutrals.Count == 0 && !isRespawning)
            {
                StartRespawnTimer();
            }
        }
    }

    [Server]
    private void AwardCampRewards(GameObject neutral)
    {
        // Find killer
        HealthSystem health = neutral.GetComponent<HealthSystem>();
        GameObject killer = health?.GetLastAttacker();

        if (killer != null)
        {
            // Notify EconomyManager
            EconomyManager.Instance?.AwardNeutralKill(killer, goldReward, xpReward, campType);

            // Apply buff if any
            if (buffPrefab != null)
            {
                ApplyBuffToKiller(killer);
            }
        }
    }

    [Server]
    private void ApplyBuffToKiller(GameObject killer)
    {
        // Instantiate buff effect on killer
        GameObject buffObj = Instantiate(buffPrefab, killer.transform.position, Quaternion.identity);
        ServerManager.Spawn(buffObj);

        // Attach to killer (assuming buff has a component that applies effect)
        BuffApplier buffApplier = buffObj.GetComponent<BuffApplier>();
        if (buffApplier != null)
        {
            buffApplier.ApplyTo(killer);
        }
    }

    [Server]
    private void StartRespawnTimer()
    {
        isRespawning = true;
        respawnCoroutine = StartCoroutine(RespawnTimer());
    }

    [Server]
    private IEnumerator RespawnTimer()
    {
        yield return new WaitForSeconds(respawnTime);

        // Respawn camp
        SpawnCamp();
        isRespawning = false;
        respawnCoroutine = null;

        Debug.Log($"[JungleCamp] {campType} camp respawned");
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        if (respawnCoroutine != null)
        {
            StopCoroutine(respawnCoroutine);
        }
    }
}