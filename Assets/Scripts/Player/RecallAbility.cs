using System.Collections;
using FishNet.Object;
using UnityEngine;
using UnityEngine.AI;

public class RecallAbility : NetworkBehaviour
{
    [SerializeField] private float recallDuration = 3f;
    [SerializeField] private GameObject recallVfxPrefab;

    private bool _isRecalling = false;
    private Vector3 _recallStartPosition;
    private Coroutine _recallCoroutine;
    private PlayerStateMachine _stateMachine;
    private NavMeshAgent _agent;

    private void Awake()
    {
        _stateMachine = GetComponent<PlayerStateMachine>();
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.B))
        {
            if (_isRecalling)
                CancelRecall();
            else
                StartRecall();
            return;
        }

        if (_isRecalling && Vector3.Distance(transform.position, _recallStartPosition) > 0.1f)
            CancelRecall();
    }

    private void StartRecall()
    {
        _isRecalling = true;
        _recallStartPosition = transform.position;
        ServerStopMovement();
        _recallCoroutine = StartCoroutine(RecallRoutine());
        Debug.Log("[RecallAbility] Recalling...");

        if (recallVfxPrefab != null)
        {
            GameObject vfx = Instantiate(recallVfxPrefab, transform.position, Quaternion.identity);
            Destroy(vfx, recallDuration);
        }
    }

    private void CancelRecall()
    {
        _isRecalling = false;
        if (_recallCoroutine != null)
            StopCoroutine(_recallCoroutine);
        Debug.Log("[RecallAbility] Recall cancelled.");
    }

    private IEnumerator RecallRoutine()
    {
        yield return new WaitForSeconds(recallDuration);
        if (_isRecalling)
            ServerRecall();
        _isRecalling = false;
    }

    [ServerRpc]
    private void ServerStopMovement()
    {
        _agent.isStopped = true;
        _agent.ResetPath();
        _agent.velocity = Vector3.zero;
        _stateMachine.ChangeState(new IdleState(_stateMachine));
    }

    [ServerRpc]
    private void ServerRecall()
    {
        TeamComponent team = GetComponent<TeamComponent>();
        if (team == null) return;
        Vector3 spawnPoint = RespawnManager.Instance.GetSpawnPoint(team.teamId.Value);
        _agent.Warp(spawnPoint);
        GetComponent<HealthComponent>()?.ResetToFull();
        GetComponent<ManaComponent>()?.ResetToFull();
        Debug.Log($"[RecallAbility] {gameObject.name} recalled to base.");
    }
}