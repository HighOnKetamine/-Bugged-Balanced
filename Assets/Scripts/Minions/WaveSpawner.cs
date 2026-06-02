using System.Collections;
using FishNet;
using FishNet.Object;
using UnityEngine;

public class WaveSpawner : NetworkBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private NetworkObject minionPrefab;

    [Header("Team")]
    [SerializeField] private sbyte teamId;

    [Header("Lanes")]
    [SerializeField] private Lane[] lanes;

    [Header("Wave Settings")]
    [SerializeField] private int minionsPerWave = 6;
    [SerializeField] private float timeBetweenWaves = 30f;
    [SerializeField] private float timeBetweenSpawns = 0.5f;

    private int _waveNumber;

    // Called by NetworkGameManager.StartGameRoutine() — not auto-started.
    [Server]
    public void StartWaves()
    {
        StartCoroutine(WaveLoop());
    }

    private IEnumerator WaveLoop()
    {
        yield return new WaitForSeconds(5f);
        while (true)
        {
            _waveNumber++;
            yield return StartCoroutine(SpawnWave());
            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    private IEnumerator SpawnWave()
    {
        foreach (Lane lane in lanes)
        {
            for (int i = 0; i < minionsPerWave; i++)
            {
                SpawnMinion(lane);
                yield return new WaitForSeconds(timeBetweenSpawns);
            }
        }
    }

    [Server]
    private void SpawnMinion(Lane lane)
    {
        Transform spawnPoint = lane.GetWaypoint(0);
        if (spawnPoint == null)
        {
            Debug.LogError($"[WaveSpawner] Lane {lane.name} has null waypoint at index 0!");
            return;
        }
        NetworkObject nob = Instantiate(minionPrefab, spawnPoint.position, spawnPoint.rotation);
        InstanceFinder.ServerManager.Spawn(nob);
        nob.GetComponent<MinionStateMachine>().Initialize(lane, teamId);
    }
}