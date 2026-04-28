using UnityEngine;
using System.Collections.Generic;

public abstract class AoeAbility : AbilityBase
{
    public enum AoeShape { Circle, Fan }

    [Header("AOE")]
    [SerializeField] protected AoeShape shape = AoeShape.Circle;
    [SerializeField] protected float radius = 4f;
    [SerializeField] protected float fanAngle = 90f;

    protected Vector3 CastPosition { get; private set; }

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
        CastPosition = Physics.Raycast(ray, out RaycastHit hit, 200f)
            ? hit.point
            : ray.GetPoint(10f);

        ServerCast(CastPosition);
    }

    protected Collider[] GetTargetsInZone(Vector3 center, LayerMask mask)
    {
        if (shape == AoeShape.Circle)
            return Physics.OverlapSphere(center, radius, mask);

        Collider[] inSphere = Physics.OverlapSphere(center, radius, mask);
        List<Collider> inFan = new();
        foreach (var col in inSphere)
        {
            Vector3 dir = (col.transform.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dir) <= fanAngle * 0.5f)
                inFan.Add(col);
        }
        return inFan.ToArray();
    }

    protected abstract void ServerCast(Vector3 position);
}