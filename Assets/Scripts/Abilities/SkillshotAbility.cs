using UnityEngine;

public abstract class SkillshotAbility : AbilityBase
{
    [Header("Skillshot")]
    [SerializeField] protected float range = 15f;
    [SerializeField] protected float projectileSpeed = 10f;
    [SerializeField] protected Transform shootPoint;

    protected Vector3 ShootDirection { get; private set; }
    protected Vector3 ShootOrigin { get; private set; }

    protected override bool CanCast()
    {
        if (!base.CanCast()) return false;
        return true;
    }

    protected override void CastAbility()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Vector3 targetPoint = Physics.Raycast(ray, out RaycastHit hit, 200f)
            ? hit.point
            : ray.GetPoint(range);

        Vector3 origin = shootPoint != null ? shootPoint.position : transform.position;
        targetPoint.y = origin.y;

        ShootOrigin = origin;
        ShootDirection = (targetPoint - origin).normalized;

        Vector3 lookTarget = targetPoint;
        lookTarget.y = transform.position.y;
        transform.LookAt(lookTarget);

        ServerCast(ShootOrigin, ShootDirection);
    }

    protected abstract void ServerCast(Vector3 origin, Vector3 direction);
}