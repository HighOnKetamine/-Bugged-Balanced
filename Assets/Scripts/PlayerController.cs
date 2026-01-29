using UnityEngine;
using UnityEngine.AI;
using FishNet.Object;

public class PlayerController : NetworkBehaviour
{
    Camera cam;
    NavMeshAgent navMeshAgent;
    private bool isInputEnabled = true;

    public bool IsInputEnabled => isInputEnabled;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();

        if (navMeshAgent == null)
            Debug.LogError("NavMeshAgent component not found on this GameObject!");
    }

    /// <summary>
    /// Enable or disable player input. Used by GameStateManager and RespawnManager.
    /// </summary>
    public void SetInputEnabled(bool enabled)
    {
        isInputEnabled = enabled;
        Debug.Log($"[PlayerController] Input {(enabled ? "enabled" : "disabled")} for {gameObject.name}");
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (IsOwner)
        {
            cam = GetComponentInChildren<Camera>();
            cam.enabled = true;
            if (cam == null)
                Debug.LogError("Main Camera not found in the scene!");
        }
    }

    void Update()
    {
        if (!IsOwner || cam == null || navMeshAgent == null || !isInputEnabled)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                navMeshAgent.SetDestination(hit.point);
            }
        }
    }
}
