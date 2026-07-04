using UnityEngine;

public class MinionChaseAttackState : State<MinionStateMachine>
{
    private bool       _isStopped;
    private Vector3    _leashPoint;
    private bool       _isStructure;
    private Collider[] _structureColliders; // cached on Enter for surface-point calculation
    private const float LeashRadius = 10f;

    public MinionChaseAttackState(MinionStateMachine machine) : base(machine) { }

    public override void Enter()
    {
        _isStopped   = false;
        _leashPoint  = Machine.transform.position;
        _isStructure = Machine.CurrentTarget != null && MinionRunState.IsStructure(Machine.CurrentTarget);

        // Cache colliders once so we can measure distance to surface each frame
        // without repeated GetComponentsInChildren calls.
        _structureColliders = _isStructure
            ? Machine.CurrentTarget.GetComponentsInChildren<Collider>()
            : null;

        Machine.SetMoving(true);
    }

    public override void Update()
    {
        if (Machine.CurrentTarget == null ||
            Machine.CurrentTarget.GetComponent<HealthComponent>()?.IsDead == true)
        {
            Machine.CurrentTarget = null;
            Machine.ChangeState(new MinionRunState(Machine));
            return;
        }

        // Units can be abandoned via leash / kite checks.
        // Structures are stationary — stay committed until they die.
        if (!_isStructure)
        {
            if (Vector3.Distance(Machine.transform.position, _leashPoint) > LeashRadius)
            {
                Machine.CurrentTarget = null;
                Machine.ChangeState(new MinionRunState(Machine));
                return;
            }

            float distToTarget = Vector3.Distance(
                Machine.transform.position, Machine.CurrentTarget.transform.position);
            if (distToTarget > Machine.aggroRange * 1.5f)
            {
                Machine.CurrentTarget = null;
                Machine.ChangeState(new MinionRunState(Machine));
                return;
            }
        }

        // For structures measure distance to the nearest surface point, not the
        // center.  This lets minions attack from the collider edge regardless of
        // how large the structure mesh is.
        float   dist;
        Vector3 navTarget;
        if (_isStructure)
        {
            navTarget = NearestSurfacePoint();
            dist      = Vector3.Distance(Machine.transform.position, navTarget);
        }
        else
        {
            navTarget = Machine.CurrentTarget.transform.position;
            dist      = Vector3.Distance(Machine.transform.position, navTarget);
        }

        if (dist <= Machine.Stats.attackRange.Value)
        {
            if (!_isStopped)
            {
                Machine.SetMoving(false);
                _isStopped = true;
            }

            float cooldown = 1f / Machine.Stats.attackSpeed.Value;
            if (Time.time - Machine.LastAttackTime >= cooldown)
            {
                Machine.LastAttackTime = Time.time;
                Machine.CurrentTarget.GetComponent<HealthComponent>()?
                    .TakeDamage(Machine.Stats.attackDamage.Value, DamageType.Physical,
                                Machine.Stats, DamageSource.AutoAttack);
            }
        }
        else
        {
            if (_isStopped)
            {
                Machine.SetMoving(true);
                _isStopped = false;
            }
            Machine.NavMeshAgent.SetDestination(navTarget);
        }
    }

    public override void Exit()
    {
        _isStopped = false;
        Machine.SetMoving(true);
    }

    // Closest point on any of the structure's bounding boxes to the minion.
    // Using ClosestPointOnBounds (AABB) is fast and keeps the target on or just
    // outside the mesh surface rather than in the unreachable center.
    private Vector3 NearestSurfacePoint()
    {
        Vector3 best  = Machine.CurrentTarget.transform.position;
        float   bestD = float.MaxValue;

        foreach (Collider col in _structureColliders)
        {
            if (col == null) continue;
            Vector3 p = col.ClosestPointOnBounds(Machine.transform.position);
            float   d = Vector3.Distance(Machine.transform.position, p);
            if (d < bestD) { bestD = d; best = p; }
        }

        return best;
    }
}
