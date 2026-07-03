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

        if (!IsServerInitialized)
        {
            if (IsOwner)
            {
                _navMeshAgent.updatePosition = false;
                _navMeshAgent.updateRotation = false;
            }
            else
            {
                _navMeshAgent.enabled = false;
            }
        }

        if (IsOwner)
        {
            Debug.Log($"[PlayerController] OnStartClient as owner — IsServerInitialized: {IsServerInitialized}");

            _cam = GetComponentInChildren<Camera>(true);
            if (_cam == null)
                Debug.LogError("[PlayerController] No Camera found in children!");
            else
            {
                _cam.enabled = true;
                Debug.Log($"[PlayerController] Camera found: {_cam.name}, depth: {_cam.depth}");
            }

            PlayerHUD playerHud = FindFirstObjectByType<PlayerHUD>();
            if (playerHud != null) playerHud.Initialize(gameObject);
            else Debug.LogWarning("[PlayerController] No PlayerHUD found in scene.");

            _gameOverCallback = _ => InputDisabled = true;
            NetworkGameManager.OnGameOver += _gameOverCallback;

            ShopUI shopUi = FindFirstObjectByType<ShopUI>();
            if (shopUi != null)
            {
                shopUi.Initialize(gameObject);
                Debug.Log("[PlayerController] ShopUI initialized.");
            }
            else
            {
                Debug.LogWarning("[PlayerController] No ShopUI found in scene.");
            }
        }
        else
        {
            Debug.Log($"[PlayerController] OnStartClient — not owner (IsServerInitialized: {IsServerInitialized})");
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

        GameObject nearestEnemy = FindNearestVisibleEnemy(hit.point);

        if (nearestEnemy != null && _basicAttack.IsOffCooldown())
        {
            NetworkObject targetNob = nearestEnemy.GetComponent<NetworkObject>();
            if (targetNob != null)
            {
                if (_basicAttack.IsInRange(nearestEnemy))
                    ServerRequestAttack(targetNob);
                else
                    ServerChaseForAttack(targetNob);
                return;
            }
        }

        // No attackable target — move to the ground position the player clicked.
        if (_stateMachine.CanMove)
            ServerSetDestination(hit.point);
    }

    // Returns the nearest alive enemy within attackMoveRadius that is currently
    // visible to the local player.  Skipping invisible enemies here avoids passing
    // an unreachable NavMesh destination when enemies are found but hidden by fog.
    private GameObject FindNearestVisibleEnemy(Vector3 center)
    {
        Collider[] cols = Physics.OverlapSphere(center, attackMoveRadius, enemyLayer);
        GameObject nearest  = null;
        float      nearestD = Mathf.Infinity;

        foreach (Collider col in cols)
        {
            if (col.gameObject == gameObject) continue;

            HealthComponent health = col.GetComponent<HealthComponent>();
            if (health == null || health.IsDead) continue;

            TeamComponent targetTeam = col.GetComponent<TeamComponent>();
            if (targetTeam == null || !_teamComponent.IsEnemy(targetTeam)) continue;

            // On the host, invisible enemies have active colliders — skip them.
            VisibilityTarget vt = col.GetComponent<VisibilityTarget>();
            if (vt != null && !vt.IsCurrentlyVisible) continue;

            float d = Vector3.Distance(center, col.transform.position);
            if (d < nearestD) { nearestD = d; nearest = col.gameObject; }
        }

        return nearest;
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
        // A stationary enemy's NavMeshObstacle carves a hole in the NavMesh.
        // Clicking inside that carved region makes SetDestination return an
        // invalid path and the agent never moves.  Snap to the nearest valid
        // point so the player always walks as close as possible to the click.
        if (NavMesh.SamplePosition(destination, out NavMeshHit navHit, 5f, NavMesh.AllAreas))
            destination = navHit.position;
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