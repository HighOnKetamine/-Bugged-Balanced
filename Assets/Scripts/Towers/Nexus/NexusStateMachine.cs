using FishNet.Object;
using UnityEngine;

[RequireComponent(typeof(HealthComponent))]
[RequireComponent(typeof(TeamComponent))]
[RequireComponent(typeof(CharacterStats))]
public class NexusStateMachine : StateMachine<NexusStateMachine>
{
    [SerializeField] private sbyte _teamId;
    [SerializeField] private int _towerCount = 1;

    public HealthComponent Health { get; private set; }
    public TeamComponent Team { get; private set; }
    public CharacterStats Stats { get; private set; }

    private int _towersRemaining;
    private int _inhibitorsDead;

    private void Awake()
    {
        Health = GetComponent<HealthComponent>();
        Team = GetComponent<TeamComponent>();
        Stats = GetComponent<CharacterStats>();
        if (Health == null) Debug.LogError($"[NexusStateMachine] Missing HealthComponent on {gameObject.name}");
        if (Team == null) Debug.LogError($"[NexusStateMachine] Missing TeamComponent on {gameObject.name}");
        if (Stats == null) Debug.LogError($"[NexusStateMachine] Missing CharacterStats on {gameObject.name}");
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Team.SetTeam(_teamId);
        _towersRemaining = _towerCount;
        _inhibitorsDead = 0;
        Health.OnDeath += _ => ChangeState(new NexusDeathState(this));
        TowerDeathState.OnTowerDied += OnTowerDied;
        InhibitorDeathState.OnInhibitorDied += OnInhibitorDied;
        InhibitorDeathState.OnInhibitorRespawned += OnInhibitorRespawned;
        ChangeState(new NexusInvulnerableState(this));
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        TowerDeathState.OnTowerDied -= OnTowerDied;
        InhibitorDeathState.OnInhibitorDied -= OnInhibitorDied;
        InhibitorDeathState.OnInhibitorRespawned -= OnInhibitorRespawned;
    }

    private void OnTowerDied(TowerStateMachine tower)
    {
        if (tower.Team.teamId.Value != _teamId) return;
        _towersRemaining--;
        Debug.Log($"[NexusStateMachine] Team {_teamId} towers remaining: {_towersRemaining}");
        // Tower death makes inhibitor vulnerable — handled by InhibitorStateMachine
    }

    private void OnInhibitorDied(InhibitorStateMachine inhibitor)
    {
        if (inhibitor.Team.teamId.Value != _teamId) return;
        _inhibitorsDead++;
        Debug.Log($"[NexusStateMachine] Team {_teamId} inhibitors down: {_inhibitorsDead}");
        if (_inhibitorsDead > 0)
            ChangeState(new NexusVulnerableState(this));
    }

    private void OnInhibitorRespawned(InhibitorStateMachine inhibitor)
    {
        if (inhibitor.Team.teamId.Value != _teamId) return;
        _inhibitorsDead = Mathf.Max(0, _inhibitorsDead - 1);
        Debug.Log($"[NexusStateMachine] Team {_teamId} inhibitor respawned. Down: {_inhibitorsDead}");
        if (_inhibitorsDead <= 0)
            ChangeState(new NexusInvulnerableState(this));
    }
}