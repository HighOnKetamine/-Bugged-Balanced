using UnityEngine;

public class TowerAttackState : State<TowerStateMachine>
{
    public TowerAttackState(TowerStateMachine machine) : base(machine) { }

    public override void Enter() { }

    public override void Update()
    {
        // Re-prioritize before anything else — swap to a player if one enters range
        TryUpgradeToMinion();

        if (Machine.CurrentTarget == null ||
            Machine.CurrentTarget.GetComponent<HealthComponent>()?.IsDead == true)
        {
            Machine.CurrentTarget = null;
            Machine.ChangeState(new TowerIdleState(Machine));
            return;
        }

        float dist = Vector3.Distance(Machine.transform.position, Machine.CurrentTarget.transform.position);
        if (dist > Machine.aggroRange)
        {
            Machine.CurrentTarget = null;
            Machine.ChangeState(new TowerIdleState(Machine));
            return;
        }

        float cooldown = 1f / Machine.Stats.attackSpeed.Value;
        if (Time.time - Machine.LastAttackTime >= cooldown)
        {
            Machine.LastAttackTime = Time.time;
            Machine.CurrentTarget.GetComponent<HealthComponent>()?
                .TakeDamage(Machine.Stats.attackDamage.Value, DamageType.Physical, Machine.Stats);
        }
    }

    public override void Exit() { }

    private void TryUpgradeToMinion()
    {
        if (Machine.CurrentTarget != null &&
            Machine.CurrentTarget.GetComponent<MinionStateMachine>() != null) return;

        Collider[] hits = Physics.OverlapSphere(
            Machine.transform.position,
            Machine.aggroRange,
            LayerMask.GetMask("Targetable")
        );

        foreach (Collider col in hits)
        {
            if (col.GetComponent<MinionStateMachine>() == null) continue;

            TeamComponent tc = col.GetComponent<TeamComponent>();
            HealthComponent hc = col.GetComponent<HealthComponent>();

            if (tc == null || hc == null || hc.IsDead) continue;
            if (!Machine.Team.IsEnemy(tc)) continue;

            Machine.CurrentTarget = col.gameObject;
            return;
        }
    }
}