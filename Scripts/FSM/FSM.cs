using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSM
{
    Enemy owner;
    public IState currState { get; private set; }

    FSM() { }
    public FSM(Enemy owner)
    {
        currState = null;
        this.owner = owner;
    }

    public void Update() { currState.Excute(owner); }
    public bool SetCurrState(IState state)
    {
        if (state == null) return false;
        currState = state;
        //Debug.Log("SetCurrState : " + state);
        return true;
    }

    public bool ChangeState(IState state)
    {
        if (state == null) return false;
        currState.Exit(owner);
        currState = state;
        currState.Enter(owner);
        return true;
    }
}
