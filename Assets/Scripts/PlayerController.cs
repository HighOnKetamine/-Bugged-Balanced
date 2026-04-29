using UnityEngine;
using UnityEngine.AI;
using FishNet.Object;

public class PlayerController : NetworkBehaviour
{
    #region Settings
    [SerializeField] private float attackMoveRadius = 5f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask groundLayer;
    #endregion

    #region References
    private Camera _cam;
    private NavMeshAgent _navMeshAgent;
    private BasicAttack _basicAttack;
    private PlayerStateMachine _stateMachine;
    private TeamComponent _teamComponent;
    private AbilityBase[] _abilities;
    #endregion

    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _basicAttack = GetComponent<BasicAttack>();
        _stateMachine = GetComponent<PlayerStateMachine>();
        _teamComponent = GetComponent<TeamComponent>();
        _abilities = GetComponents<AbilityBase>();

        if (_navMeshAgent != null)
            _navMeshAgent.angularSpeed = 0f;

        if (_navMeshAgent == null) Debug.LogError("[PlayerController] No NavMeshAgent found!");
        if (_basicAttack == null) Debug.LogError("[PlayerController] No BasicAttack found!");
        if (_stateMachine == null) Debug.LogError("[PlayerController] No PlayerStateMachine found!");
        if (_teamComponent == null) Debug.LogError("[PlayerController] No TeamComponent found!");
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!IsOwner)
            _navMeshAgent.enabled = false;

        if (IsOwner)
        {
            _cam = GetComponentInChildren<Camera>();
            if (_cam == null)
                Debug.LogError("[PlayerController] No Camera found!");
            else
                _cam.enabled = true;

            AbilityHUD hud = FindFirstObjectByType<AbilityHUD>();
            hud?.Initialize(gameObject);
        }
    }

    private void Update()
    {
        if (IsServerInitialized)
            RotateTowardMovement();

        if (!IsOwner || _cam == null || _navMeshAgent == null) return;

        HandleMovement();
        HandleAttackMove();
        HandleAbilities();
    }

    private void RotateTowardMovement()
    {
        if (_navMeshAgent.velocity.sqrMagnitude > 0.01f)
            transform.forward = _navMeshAgent.velocity.normalized;
    }

    private void HandleMovement()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            ServerStop();
            return;
        }

        if (!Input.GetMouseButtonDown(1)) return;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) return;
        if (!_stateMachine.CanMove) return;

        Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
            ServerSetDestination(hit.point);
    }

    private void HandleAttackMove()
    {
        if (!Input.GetMouseButtonDown(1)) return;
        if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift)) return;
        if (!_stateMachine.CanAttack) return;

        Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer)) return;

        Collider[] cols = Physics.OverlapSphere(hit.point, attackMoveRadius, enemyLayer);
        GameObject nearestEnemy = null;
        float nearestDistance = Mathf.Infinity;

        foreach (Collider col in cols)
        {
            if (col.gameObject == gameObject) continue;

            HealthComponent health = col.GetComponent<HealthComponent>();
            if (health == null || health.IsDead) continue;

            TeamComponent targetTeam = col.GetComponent<TeamComponent>();
            if (targetTeam == null || !_teamComponent.IsEnemy(targetTeam)) continue;

            float distance = Vector3.Distance(hit.point, col.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestEnemy = col.gameObject;
            }
        }

        if (nearestEnemy != null)
        {
            NetworkObject targetNob = nearestEnemy.GetComponent<NetworkObject>();
            if (targetNob == null)
            {
                Debug.LogWarning("[PlayerController] Attack-move target has no NetworkObject!");
                return;
            }

            if (!_basicAttack.IsOffCooldown())
            {
                if (_stateMachine.CanMove)
                    ServerSetDestination(hit.point);
            }
            else if (_basicAttack.IsInRange(nearestEnemy))
            {
                ServerRequestAttack(targetNob);
            }
            else
            {
                ServerChaseForAttack(targetNob);
            }
        }
        else
        {
            if (_stateMachine.CanMove)
                ServerSetDestination(hit.point);
        }
    }

    private void HandleAbilities()
    {
        // Temp test keys — remove when game manager exists
        if (Input.GetKeyDown(KeyCode.T)) ServerSetTeam(0);
        if (Input.GetKeyDown(KeyCode.Y)) ServerSetTeam(1);

        if (!_stateMachine.CanCast) return;

        foreach (AbilityBase ability in _abilities)
        {
            if (Input.GetKeyDown(ability.Hotkey))
            {
                ability.TryCastAbility();
                break; // one ability per frame
            }
        }
    }

    [ServerRpc]
    private void ServerSetTeam(sbyte teamId) => GetComponent<TeamComponent>().SetTeam(teamId);

    #region Server Methods

    [ServerRpc]
    private void ServerStop()
    {
        StopMovement();
        _stateMachine.ChangeState(new IdleState(_stateMachine));
    }

    private void StopMovement()
    {
        _navMeshAgent.isStopped = true;
        _navMeshAgent.ResetPath();
        _navMeshAgent.velocity = Vector3.zero;
    }

    [ServerRpc]
    private void ServerSetDestination(Vector3 destination)
    {
        _stateMachine.AttackMoveTarget = null;
        StopMovement();
        _navMeshAgent.SetDestination(destination);
        _navMeshAgent.isStopped = false;
        _stateMachine.ChangeState(new RunState(_stateMachine));
    }

    [ServerRpc]
    private void ServerChaseForAttack(NetworkObject target)
    {
        _stateMachine.AttackMoveTarget = target.gameObject;
        StopMovement();
        _navMeshAgent.SetDestination(target.transform.position);
        _navMeshAgent.isStopped = false;
        _stateMachine.ChangeState(new RunState(_stateMachine));
    }

    [ServerRpc]
    private void ServerRequestAttack(NetworkObject target)
    {
        if (target == null) return;
        _basicAttack.Attack(target.gameObject);
    }

    #endregion
}