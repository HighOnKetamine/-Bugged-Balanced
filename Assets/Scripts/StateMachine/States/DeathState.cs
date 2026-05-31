using UnityEngine;

public class DeathState : State<PlayerStateMachine>
{
    public DeathState(PlayerStateMachine machine) : base(machine) { }

    public override void Enter()
    {
        Debug.Log("[DeathState] Enter");
        Machine.NavMeshAgent.ResetPath();
        Machine.NavMeshAgent.velocity = Vector3.zero;
        Machine.CanAttack = false;
        Machine.CanCast = false;
        Machine.CanMove = false;
        Machine.Animator.SetTrigger("Death");

        // NEW: Register this player for a respawn timer.
        // We reach the handler through the Machine (PlayerStateMachine is a MonoBehaviour,
        // so GetComponent is available on it). This keeps DeathState decoupled — it doesn't
        // need to know anything about respawn logic, it just hands off responsibility.
        var respawnHandler = Machine.GetComponent<PlayerRespawnHandler>();
        if (respawnHandler != null)
        {
            RespawnManager.Instance.RegisterDeath(respawnHandler);
        }
        else
        {
            Debug.LogWarning("[DeathState] PlayerRespawnHandler not found on player prefab!");
        }
    }

    public override void Update() { }

    public override void Exit()
    {
        // Exit is called by PlayerRespawnHandler.Respawn() when the timer fires.
        // At this point CanAttack/CanCast/CanMove being restored here is correct —
        // the state machine transition handles the timing, not a manual flag flip.
        Machine.Animator.Rebind();
        Machine.CanAttack = true;
        Machine.CanCast = true;
        Machine.CanMove = true;
    }
}