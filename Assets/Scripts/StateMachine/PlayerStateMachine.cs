using UnityEngine;
using UnityEngine.AI;
using FishNet.Object;
using FishNet.Component.Animating;

public class PlayerStateMachine : NetworkBehaviour
{
    public Animator Animator { get; private set; }
    public NetworkAnimator NetworkAnimator { get; private set; }
    public NavMeshAgent NavMeshAgent { get; private set; }
    public CharacterStats Stats { get; private set; }
    public BasicAttack BasicAttack { get; private set; }
    public HealthComponent Health { get; private set; }
    public GameObject CurrentAttackTarget { get; private set; }

    private PlayerState _currentState;

    private void Awake()
    {
        Animator = GetComponent<Animator>();
        NetworkAnimator = GetComponent<NetworkAnimator>();
        NavMeshAgent = GetComponent<NavMeshAgent>();
        Stats = GetComponent<CharacterStats>();
        BasicAttack = GetComponent<BasicAttack>();
        Health = GetComponent<HealthComponent>();

        if (Animator == null)
            Debug.LogError($"[PlayerStateMachine] No Animator found on {gameObject.name}!");
        if (NetworkAnimator == null)
            Debug.LogError($"[PlayerStateMachine] No NetworkAnimator found on {gameObject.name}!");
        if (NavMeshAgent == null)
            Debug.LogError($"[PlayerStateMachine] No NavMeshAgent found on {gameObject.name}!");
        if (Stats == null)
            Debug.LogError($"[PlayerStateMachine] No CharacterStats found on {gameObject.name}!");
        if (BasicAttack == null)
            Debug.LogError($"[PlayerStateMachine] No BasicAttack found on {gameObject.name}!");
        if (Health == null)
            Debug.LogError($"[PlayerStateMachine] No HealthComponent found on {gameObject.name}!");
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

    private void Update()
    {
        _currentState?.Update();
    }

    public void ChangeState(PlayerState newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter();
    }
}