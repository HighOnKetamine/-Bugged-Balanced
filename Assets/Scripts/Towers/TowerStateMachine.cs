using UnityEngine;
using FishNet.Object;

[RequireComponent(typeof(HealthComponent))]
[RequireComponent(typeof(TeamComponent))]
[RequireComponent(typeof(CharacterStats))]
[RequireComponent(typeof(NetworkObject))]
public class TowerStateMachine : StateMachine<TowerStateMachine>
{
    [Header("AI")]
    public float aggroRange = 12f;

    public HealthComponent Health { get; private set; }
    public TeamComponent Team { get; private set; }
    public CharacterStats Stats { get; private set; }

    public GameObject CurrentTarget { get; set; }
    public float LastAttackTime { get; set; }

    [SerializeField] private sbyte _teamId;

    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log($"[TowerStateMachine] OnStartServer fired on {gameObject.name}");
        Initialize(_teamId);
    }

    private void Awake()
    {
        Health = GetComponent<HealthComponent>();
        Team = GetComponent<TeamComponent>();
        Stats = GetComponent<CharacterStats>();

        if (Health == null) Debug.LogError($"[TowerStateMachine] Missing HealthComponent on {gameObject.name}");
        if (Team == null) Debug.LogError($"[TowerStateMachine] Missing TeamComponent on {gameObject.name}");
        if (Stats == null) Debug.LogError($"[TowerStateMachine] Missing CharacterStats on {gameObject.name}");
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!IsServerInitialized)
            enabled = false;             // state machine runs server-only
    }

    [Server]
    public void Initialize(sbyte teamId)
    {
        Team.SetTeam(teamId);
        Health.OnDeath += killer => ChangeState(new TowerDeathState(this, killer));
        ChangeState(new TowerIdleState(this));
    }
}