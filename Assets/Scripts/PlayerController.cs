using UnityEngine;
using UnityEngine.AI;
using FishNet.Object;

public class PlayerController : NetworkBehaviour
{
    #region Settings
    [SerializeField] private float attackMoveRadius = 300f;
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
        if (IsOwner)
        {
            _cam = GetComponentInChildren<Camera>();
            if (_cam == null)
                Debug.LogError("[PlayerController] No Camera found!");
            else
                _cam.enabled = true;
        }
    }

    private void Update()
    {
        if (!IsOwner || _cam == null || _navMeshAgent == null) return;

        HandleMovement();
        HandleAttackMove();
        HandleAbilities();
    }

    //! Handles right click movement
    private void HandleMovement()
    {
        if (!Input.GetMouseButtonDown(1)) return;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) return;
        if (!_stateMachine.CanMove) return;

        Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
            _navMeshAgent.SetDestination(hit.point);
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
            if (_basicAttack.CanAttack(nearestEnemy))
                _basicAttack.Attack(nearestEnemy);
            else
                _navMeshAgent.SetDestination(nearestEnemy.transform.position);
        }
        else
        {
            if (_stateMachine.CanMove)
                _navMeshAgent.SetDestination(hit.point);
        }
    }

    // Handles ability input - placeholders for ability system
    private void HandleAbilities()
    {
        if (!_stateMachine.CanCast) return;

        if (Input.GetKeyDown(KeyCode.Q)) { /* cast Q */ }
        if (Input.GetKeyDown(KeyCode.W)) { /* cast W */ }
        if (Input.GetKeyDown(KeyCode.E)) { /* cast E */ }
        if (Input.GetKeyDown(KeyCode.R)) { /* cast R */ }
    }
}