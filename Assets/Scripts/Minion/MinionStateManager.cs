using FishNet.Object;
using UnityEngine;

public class MinionStateManager : NetworkBehaviour
{
    public uint goldValue;
    public uint experienceValue;


    MinionBaseState currentState;


    void Start()
    {

    }

    void Update()
    {

    }

    public void SwitchState(MinionBaseState state)
    {
        this.currentState = state;

        this.currentState.EnterState(this);
    }
}