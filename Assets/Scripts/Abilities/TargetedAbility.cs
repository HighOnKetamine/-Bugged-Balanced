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
        return CurrentTarget != null;
    }

    protected override void CastAbility()
    {
        if (CurrentTarget == null) return;
        transform.LookAt(new Vector3(
            CurrentTarget.transform.position.x,
            transform.position.y,
            CurrentTarget.transform.position.z));
        ServerCast(CurrentTarget);
    }

    private GameObject GetTargetUnderMouse()
    {
        Camera cam = Camera.main;
        if (cam == null) return null;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, castRange * 2f, targetMask))
            return null;

        if (Vector3.Distance(transform.position, hit.point) > castRange)
            return null;

        return hit.collider.gameObject;
    }

    [FishNet.Object.ServerRpc]
    protected abstract void ServerCast(GameObject target);
}