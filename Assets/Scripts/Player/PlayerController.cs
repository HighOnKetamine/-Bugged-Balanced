using System;
using UnityEngine;
using UnityEngine.AI;
using FishNet.Object;

public class PlayerController : NetworkBehaviour
{
    public static bool InputDisabled = false;

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
    private Action<sbyte> _gameOverCallback;
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
        // Ensure camera reference is always retrieved. Enable the camera only for the owner
        // so other clients (and server instances) do not render another player's view.
        _cam = GetComponentInChildren<Camera>();
        if (_cam != null)
            _cam.enabled = IsOwner;
        else if (IsOwner)
            Debug.LogError("[PlayerController] No Camera found!");

        // Only disable the NavMeshAgent on non-owner CLIENT instances.
        // OnStartClient runs on both client and server (host), so ensure we don't
        // disable the server-side NavMeshAgent which is responsible for authoritative
        // movement. Leave the agent enabled on the server.
        if (!IsOwner && !IsServer)
            _navMeshAgent.enabled = false;

        if (IsOwner)
        {
            PlayerHUD playerHud = FindFirstObjectByType<PlayerHUD>();
            playerHud?.Initialize(gameObject);

            _gameOverCallback = _ => InputDisabled = true;
            NetworkGameManager.OnGameOver += _gameOverCallback;

            ShopUI shopUi = FindFirstObjectByType<ShopUI>();
            if (shopUi != null)
            {
                TeamComponent team = GetComponent<TeamComponent>();
                Transform baseTransform = null;
                if (team != null && RespawnManager.Instance != null)
                {
                    Vector3 basePos = RespawnManager.Instance.GetSpawnPoint(team.teamId.Value);
                    GameObject baseMarker = new GameObject("BaseMarker");
                    baseMarker.transform.position = basePos;
                    baseTransform = baseMarker.transform;
                }
                shopUi.Initialize(gameObject, baseTransform);
            }

            // StartCoroutine(InitializeShopDelayed());
        }
    }

    // private System.Collections.IEnumerator InitializeShopDelayed()
    // {
    //     // Wait for team assignment and RespawnManager to be ready
    //     yield return new WaitForSeconds(0.5f);

    //     ShopUI shopUi = FindFirstObjectByType<ShopUI>();
    //     if (shopUi != null)
    //     {
    //         TeamComponent team = GetComponent<TeamComponent>();
    //         Transform baseTransform = null;
    //         if (team != null && RespawnManager.Instance != null)
    //         {
    //             Vector3 basePos = RespawnManager.Instance.GetSpawnPoint(team.teamId.Value);
    //             GameObject baseMarker = new GameObject("BaseMarker");
    //             baseMarker.transform.position = basePos;
    //             baseTransform = baseMarker.transform;
    //         }
    //         shopUi.Initialize(gameObject, baseTransform);
    //     }
    // }

    public override void OnStopClient()
    {
        base.OnStopClient();
        if (IsOwner && _gameOverCallback != null)
            NetworkGameManager.OnGameOver -= _gameOverCallback;
    }

    private void Update()
    {
        if (IsServerInitialized)
            RotateTowardMovement();

        if (!IsOwner || _cam == null || _navMeshAgent == null) return;
        if (InputDisabled) return;

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
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.K) && IsOwner)
            GetComponent<HealthComponent>().TakeDamage(99999f, DamageType.True);
#endif

        if (!_stateMachine.CanCast) return;

        bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        for (int i = 0; i < _abilities.Length; i++)
        {
            if (!Input.GetKeyDown(_abilities[i].Hotkey)) continue;
            if (ctrl)
                ServerLevelUpAbility(i);
            else
                _abilities[i].TryCastAbility();
            break;
        }
    }

    [ServerRpc]
    private void ServerLevelUpAbility(int abilityIndex)
    {
        if (abilityIndex < 0 || abilityIndex >= _abilities.Length) return;
        ExperienceComponent exp = GetComponent<ExperienceComponent>();
        _abilities[abilityIndex].TryLevelUp(exp);
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