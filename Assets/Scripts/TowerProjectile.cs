using UnityEngine;

/// <summary>
/// Visual-only projectile for tower attacks.
/// Server handles damage; this is purely cosmetic animation.
/// </summary>
public class TowerProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private float arcHeight = 2f;

    [Header("Visuals")]
    [SerializeField] private GameObject impactEffect;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float journeyLength;
    private float startTime;
    private bool initialized = false;

    public void Initialize(Vector3 start, Vector3 target)
    {
        startPosition = start;
        targetPosition = target;
        journeyLength = Vector3.Distance(start, target);
        startTime = Time.time;
        initialized = true;
    }

    private void Update()
    {
        if (!initialized) return;

        float distanceCovered = (Time.time - startTime) * speed;
        float fractionOfJourney = distanceCovered / journeyLength;

        if (fractionOfJourney >= 1f)
        {
            // Reached target
            OnReachTarget();
            return;
        }

        // Linear interpolation between start and target
        Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);

        // Add arc (parabola)
        float arcProgress = Mathf.Sin(fractionOfJourney * Mathf.PI);
        currentPos.y += arcHeight * arcProgress;

        transform.position = currentPos;

        // Face direction of travel
        if (fractionOfJourney < 0.99f)
        {
            Vector3 nextPos = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney + 0.01f);
            nextPos.y += arcHeight * Mathf.Sin((fractionOfJourney + 0.01f) * Mathf.PI);
            transform.LookAt(nextPos);
        }
    }

    private void OnReachTarget()
    {
        // Spawn impact effect
        if (impactEffect != null)
        {
            Instantiate(impactEffect, targetPosition, Quaternion.identity);
        }

        // Destroy projectile
        Destroy(gameObject);
    }
}
