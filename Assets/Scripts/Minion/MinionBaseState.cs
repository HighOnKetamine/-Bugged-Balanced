
using UnityEngine;

public abstract class MinionBaseState
{
    public abstract void EnterState(MinionStateManager minion);

    public abstract void UpdateState(MinionStateManager minion);

    public abstract void OnCollisionEnter(MinionStateManager minion, Collision collision);

}