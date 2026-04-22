using UnityEngine;
using FishNet.Component.Animating;
using UnityEngine.AI;

public class PlayerStateMachine : StateMachine<PlayerStateMachine>
{
    public Animator Animator { get; private set; }
    public NetworkAnimator NetworkAnimator { get; private set; }
    public NavMeshAgent NavMeshAgent { get; private set; }
    public CharacterStats Stats { get; private set; }
    public BasicAttack BasicAttack { get; private set; }
    public HealthComponent Health { get; private set; }
    public GameObject CurrentAttackTarget { get; set; }
    public GameObject AttackMoveTarget { get; set; }

    public bool CanMove { get; set; } = true;
    public bool CanAttack { get; set; } = true;
    public bool CanCast { get; set; } = true;

    private void Awake()
    {
        Animator = GetComponent<Animator>();
        NetworkAnimator = GetComponent<NetworkAnimator>();
        NavMeshAgent = GetComponent<NavMeshAgent>();
        Stats = GetComponent<CharacterStats>();
        BasicAttack = GetComponent<BasicAttack>();
        Health = GetComponent<HealthComponent>();

        if (Animator == null) Debug.LogError($"[PlayerStateMachine] Missing Animator on {gameObject.name}");
        if (NetworkAnimator == null) Debug.LogError($"[PlayerStateMachine] Missing NetworkAnimator on {gameObject.name}");
        if (NavMeshAgent == null) Debug.LogError($"[PlayerStateMachine] Missing NavMeshAgent on {gameObject.name}");
        if (Stats == null) Debug.LogError($"[PlayerStateMachine] Missing CharacterStats on {gameObject.name}");
        if (BasicAttack == null) Debug.LogError($"[PlayerStateMachine] Missing BasicAttack on {gameObject.name}");
        if (Health == null) Debug.LogError($"[PlayerStateMachine] Missing HealthComponent on {gameObject.name}");
    }

    private void Start()
    {
        Health.OnDeath += _ => ChangeState(new DeathState(this));
        BasicAttack.OnPreAttack += target =>
        {
            CurrentAttackTarget = target;
            ChangeState(new BasicAttackState(this));
        };

        ChangeState(new IdleState(this));
    }
}