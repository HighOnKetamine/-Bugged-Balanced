using UnityEngine;

public abstract class TargetedAbility : AbilityBase
{
    [Header("Targeted")]
    [SerializeField] protected float castRange = 10f;
    [SerializeField] protected LayerMask targetMask;

    protected GameObject CurrentTarget { get; private set; }

    protected override bool CanCast()
    {
        if (!base.CanCast()) return false;
        CurrentTarget = GetTargetUnderMouse();
        if (CurrentTarget == null) return false;

        // On the host, hidden enemies still have active colliders so a raycast
        // can land on them.  Block targeting anything not currently visible.
        VisibilityTarget vt = CurrentTarget.GetComponent<VisibilityTarget>();
        if (vt != null && !vt.IsCurrentlyVisible)
            return false;

        return true;
    }

    protected override void CastAbility()
    {
        if (CurrentTarget == null) return;
        transform.LookAt(new Vector3(
            CurrentTarget.transform.position.x,
            transform.position.y,
            CurrentTarget.transform.position.z));
        ConsumeMana();
        ServerCast(CurrentTarget);
    }

    private GameObject GetTargetUnderMouse()
    {
        Camera cam = _cam;
        if (cam == null) return null;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, castRange * 2f, targetMask))
            return null;

        if (Vector3.Distance(transform.position, hit.point) > castRange)
            return null;

        return hit.collider.gameObject;
    }

    protected abstract void ServerCast(GameObject target);
}