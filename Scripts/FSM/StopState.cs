using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Stop State", menuName = "ScriptableObject/FPS State/Stop", order = 1)]
public class StopState : ScriptableObject, IState
{
    public void Enter(Enemy owner)
    {
        owner.SetPatrolStop();
    }
    public void Excute(Enemy owner)
    {
        if (owner.FindTarget())
        {
            owner.OnAttackState();
        }
        else
        {
            //타이머 동안 멈춘상태로
            if (owner.TimeCheck()) return;
            else owner.OnRotateState();
        }
        
    }
    public void Exit(Enemy owner)
    {
    }
}