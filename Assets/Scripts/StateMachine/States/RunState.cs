public class RunState : State<PlayerStateMachine>
{
    public RunState(PlayerStateMachine machine) : base(machine) { }

    public override void Enter()
    {
        Machine.Animator.SetBool("IsMoving", true);
    }

    public override void Update()
    {
        var agent = Machine.NavMeshAgent;
        if (agent != null && Machine.Stats != null)
            agent.speed = Machine.Stats.moveSpeed.Value;

        // If agent is not ready (disabled / not on NavMesh / null), skip nav-specific checks.
        if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh)
            return;

        if (agent.pathPending) return;

        if (Machine.AttackMoveTarget != null)
        {
            HealthComponent health = Machine.AttackMoveTarget.GetComponent<HealthComponent>();
            if (health == null || health.IsDead)
            {
                Machine.AttackMoveTarget = null;
                Machine.ChangeState(new IdleState(Machine));
                return;
            }

            // Stop chasing if the target leaves vision
            TeamComponent myTeam = Machine.GetComponent<TeamComponent>();
            if (myTeam != null && myTeam.teamId.Value != TeamComponent.Neutral &&
                ServerVisionTracker.Instance != null &&
                !ServerVisionTracker.Instance.CanSee(myTeam.teamId.Value, Machine.AttackMoveTarget.transform.position))
            {
                Machine.AttackMoveTarget = null;
                Machine.ChangeState(new IdleState(Machine));
                return;
            }

            if (!Machine.BasicAttack.IsOffCooldown())
            {
                Machine.AttackMoveTarget = null;
                Machine.ChangeState(new IdleState(Machine));
                return;
            }
            if (Machine.BasicAttack.IsInRange(Machine.AttackMoveTarget))
            {
                var target = Machine.AttackMoveTarget;
                Machine.AttackMoveTarget = null;
                Machine.BasicAttack.Attack(target);
            }
            return;
        }

        if (agent.remainingDistance <= agent.stoppingDistance)
            Machine.ChangeState(new IdleState(Machine));
    }

    public override void Exit() { }
}