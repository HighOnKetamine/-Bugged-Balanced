using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class RespawnManager : NetworkBehaviour
{
    public static RespawnManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    [Header("Respawn Settings")]
    [SerializeField] private float defaultRespawnTime = 5f;

    [Header("Spawn Points")]
    [Tooltip("One entry per team. Set Team Id and drag in the spawn point Transforms.")]
    [SerializeField] private TeamSpawnConfig[] teamSpawnConfigs;

    // Serializable wrapper so it shows up nicely in the Inspector
    [System.Serializable]
    private class TeamSpawnConfig
    {
        public sbyte teamId;
        public Transform[] spawnPoints;
    }

    // Built at startup for O(1) lookup by team id
    private Dictionary<sbyte, Transform[]> _spawnMap;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        _spawnMap = new Dictionary<sbyte, Transform[]>();
        foreach (var config in teamSpawnConfigs)
            _spawnMap[config.teamId] = config.spawnPoints;
    }

    private struct PendingRespawn
    {
        public PlayerRespawnHandler Handler;
        public float RespawnAt;
        public sbyte TeamId;
    }

    private readonly List<PendingRespawn> _pending = new();

    [Server]
    public void RegisterDeath(PlayerRespawnHandler handler, float overrideTime = -1f)
    {
        float delay = overrideTime > 0f ? overrideTime : defaultRespawnTime;
        sbyte teamId = handler.GetComponent<TeamComponent>()?.teamId.Value
                       ?? TeamComponent.Neutral;

        _pending.Add(new PendingRespawn
        {
            Handler = handler,
            RespawnAt = Time.time + delay,
            TeamId = teamId
        });

        handler.OnDeathRegistered(delay);
    }

    private void Update()
    {
        if (!IsServerInitialized) return;

        for (int i = _pending.Count - 1; i >= 0; i--)
        {
            if (Time.time >= _pending[i].RespawnAt)
            {
                _pending[i].Handler.Respawn(GetSpawnPoint(_pending[i].TeamId));
                _pending.RemoveAt(i);
            }
        }
    }

    private Vector3 GetSpawnPoint(sbyte teamId)
    {
        // Neutral nebo neznámý tým — fallback na origin
        if (teamId == TeamComponent.Neutral || !_spawnMap.TryGetValue(teamId, out var points))
        {
            Debug.LogWarning($"[RespawnManager] No spawn config for team {teamId}.");
            return Vector3.zero;
        }

        return points[Random.Range(0, points.Length)].position;
    }
}