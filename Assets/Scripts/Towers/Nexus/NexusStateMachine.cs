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

        Health.OnDeath += killer => ChangeState(new NexusDeathState(this));
        TowerDeathState.OnTowerDied += OnTowerDied;

        ChangeState(new NexusInvulnerableState(this));
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        TowerDeathState.OnTowerDied -= OnTowerDied;
    }

    private void OnTowerDied(TowerStateMachine tower)
    {
        if (tower.Team.teamId.Value != _teamId) return;

        _towersRemaining--;
        Debug.Log($"[NexusStateMachine] Team {_teamId} towers remaining: {_towersRemaining}");

        if (_towersRemaining <= 0)
            ChangeState(new NexusVulnerableState(this));
    }
}