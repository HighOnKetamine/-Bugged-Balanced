using FishNet.Object;
using UnityEngine;

[RequireComponent(typeof(HealthComponent))]
[RequireComponent(typeof(TeamComponent))]
[RequireComponent(typeof(CharacterStats))]
public class InhibitorStateMachine : StateMachine<InhibitorStateMachine>
{
    [SerializeField] private sbyte _teamId;
    [SerializeField] public float respawnTime = 90f;

    public HealthComponent Health { get; private set; }
    public TeamComponent Team { get; private set; }
    public CharacterStats Stats { get; private set; }

    private void Awake()
    {
        Health = GetComponent<HealthComponent>();
        Team = GetComponent<TeamComponent>();
        Stats = GetComponent<CharacterStats>();
        if (Health == null) Debug.LogError($"[InhibitorStateMachine] Missing HealthComponent on {gameObject.name}");
        if (Team == null) Debug.LogError($"[InhibitorStateMachine] Missing TeamComponent on {gameObject.name}");
        if (Stats == null) Debug.LogError($"[InhibitorStateMachine] Missing CharacterStats on {gameObject.name}");
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Team.SetTeam(_teamId);
        Health.OnDeath += OnDied;
        TowerDeathState.OnTowerDied += OnTowerDied;
        ChangeState(new InhibitorInvulnerableState(this));
    }


    public override void OnStopServer()
    {
        base.OnStopServer();
        TowerDeathState.OnTowerDied -= OnTowerDied;
    }

    private void OnTowerDied(TowerStateMachine tower)
    {
        if (tower.Team.teamId.Value != _teamId) return;
        // Tower of same team died — inhibitor is now exposed
        ChangeState(new InhibitorVulnerableState(this));
    }
    private void OnDied(GameObject killer)
    {
        ChangeState(new InhibitorDeathState(this, killer));
    }

    public void Respawn()
    {
        Health.ResetToFull();
        ChangeState(new InhibitorInvulnerableState(this));
    }
}