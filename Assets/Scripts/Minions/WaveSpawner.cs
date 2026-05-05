using System.Collections;
using UnityEngine;
using FishNet.Object;
using FishNet;

public class WaveSpawner : NetworkBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private NetworkObject minionPrefab;

    [Header("Lanes")]
    [SerializeField] private Lane[] lanes; // přiřaď všechny lane objekty ze scény

    [Header("Wave Settings")]
    [SerializeField] private int minionsPerWave = 6;
    [SerializeField] private float timeBetweenWaves = 30f;
    [SerializeField] private float timeBetweenSpawns = 0.5f; // interval mezi spawnem jednotlivých minionů v jedné vlně

    private int _waveNumber = 0;

    public override void OnStartServer()
    {
        base.OnStartServer();
        StartCoroutine(WaveLoop());
    }

    private IEnumerator WaveLoop()
    {
        yield return new WaitForSeconds(5f); // krátká pauza na začátku hry

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
            Debug.LogError($"[WaveSpawner] Lane {lane.name} má null waypoint na indexu 0!");
            return;
        }

        NetworkObject nob = Instantiate(minionPrefab, spawnPoint.position, spawnPoint.rotation);
        InstanceFinder.ServerManager.Spawn(nob);
        nob.GetComponent<MinionStateMachine>().Initialize(lane);
    }
}