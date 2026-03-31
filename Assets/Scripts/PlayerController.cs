using UnityEngine;
using UnityEngine.AI;
using FishNet.Object;

public class PlayerController : NetworkBehaviour
{
    #region Settings
    // World-space radius for attack-move target search (tune to match your NavMesh scale)
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
    #endregion

    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _basicAttack = GetComponent<BasicAttack>();
        _stateMachine = GetComponent<PlayerStateMachine>();
        _teamComponent = GetComponent<TeamComponent>();

        // Disable built-in agent rotation — we handle it manually
        // so direction changes are instant instead of lerped.
        if (_navMeshAgent != null)
            _navMeshAgent.angularSpeed = 0f;

        if (_navMeshAgent == null)
            Debug.LogError("[PlayerController] No NavMeshAgent found!");
        if (_basicAttack == null)
            Debug.LogError("[PlayerController] No BasicAttack found!");
        if (_stateMachine == null)
            Debug.LogError("[PlayerController] No PlayerStateMachine found!");
        if (_teamComponent == null)
            Debug.LogError("[PlayerController] No TeamComponent found!");
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        // Disable NavMeshAgent on non-owner clients.
        // The agent only runs meaningfully on the server; leaving it enabled
        // on remote clients causes it to fight FishNet's position sync,
        // producing jitter and ghost movement.
        if (!IsOwner)
            _navMeshAgent.enabled = false;

        if (IsOwner)
        {
            // Camera starts disabled on the prefab intentionally —
            // only the owning client should enable it to avoid duplicate views.
            _cam = GetComponentInChildren<Camera>();
            if (_cam == null)
                Debug.LogError("[PlayerController] No Camera found!");
            else
                _cam.enabled = true;
        }
    }

    private void Update()
    {
        // Rotation runs on the server where NavMeshAgent has real velocity.
        // NetworkTransform will sync the resulting rotation to all clients.
        if (IsServerInitialized)
            RotateTowardMovement();

        if (!IsOwner || _cam == null || _navMeshAgent == null) return;

        HandleMovement();
        HandleAttackMove();
        HandleAbilities();
    }

    // Snaps transform.forward to the agent's current velocity direction.
    // We set angularSpeed = 0 in Awake so the agent never competes with this.
    // Why snap instead of Slerp? In a MOBA the character should always face
    // where it's going immediately — a slow turn feels sluggish and wrong.
    private void RotateTowardMovement()
    {
        if (_navMeshAgent.velocity.sqrMagnitude > 0.01f)
            transform.forward = _navMeshAgent.velocity.normalized;
    }

    //! Handles right click movement
    private void HandleMovement()
    {
        if (!Input.GetMouseButtonDown(1)) return;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) return;
        if (!_stateMachine.CanMove) return;

        Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
            ServerSetDestination(hit.point);
    }

    //! Handles shift + right click attack move
    private void HandleAttackMove()
    {
        if (!Input.GetMouseButtonDown(1)) return;
        if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift)) return;
        if (!_stateMachine.CanAttack) return;

        Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer)) return;

        //! find nearest enemy around cursor point
        Collider[] cols = Physics.OverlapSphere(hit.point, attackMoveRadius, enemyLayer);
        GameObject nearestEnemy = null;
        float nearestDistance = Mathf.Infinity;

        foreach (Collider col in cols)
        {
            if (col.gameObject == gameObject) continue;

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

            if (_basicAttack.CanAttack(nearestEnemy))
                ServerRequestAttack(targetNob);
            else
                ServerSetDestination(nearestEnemy.transform.position);
        }
        else
        {
            // No target found — fall back to moving toward the clicked point
            if (_stateMachine.CanMove)
                ServerSetDestination(hit.point);
        }
    }

    // TODO: wire up ability system
    private void HandleAbilities()
    {
        if (!_stateMachine.CanCast) return;

        if (Input.GetKeyDown(KeyCode.Q)) { /* cast Q */ }
        if (Input.GetKeyDown(KeyCode.W)) { /* cast W */ }
        if (Input.GetKeyDown(KeyCode.E)) { /* cast E */ }
        if (Input.GetKeyDown(KeyCode.R)) { /* cast R */ }
    }

    #region ServerRpcs

    /// <summary>
    /// Sends a move order to the server. Only the owning client may call this.
    /// </summary>
    [ServerRpc]
    private void ServerSetDestination(Vector3 destination)
    {
        _navMeshAgent.isStopped = true;
        _navMeshAgent.ResetPath();
        _navMeshAgent.velocity = Vector3.zero;
        _navMeshAgent.SetDestination(destination);
        _navMeshAgent.isStopped = false;
    }

    /// <summary>
    /// Sends an attack order to the server. Target is passed as NetworkObject
    /// so the reference survives the server round-trip.
    /// Only the owning client may call this.
    /// </summary>
    [ServerRpc]
    private void ServerRequestAttack(NetworkObject target)
    {
        if (target == null) return;
        _basicAttack.Attack(target.gameObject);
    }

    #endregion
}