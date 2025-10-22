using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float rotationSpeed = 10f;
    public LayerMask groundMask;

    [Header("References")]
    public Camera mainCamera;

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void Update()
    {
        HandleMouseClick();
        RotateTowardsMovementDirection();
    }

    void HandleMouseClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundMask))
            {
                agent.SetDestination(hit.point);
            }
        }
    }

    void RotateTowardsMovementDirection()
    {
        if (agent.velocity.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(agent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        if (agent != null)
            Gizmos.DrawSphere(agent.destination, 0.1f);
    }
}
