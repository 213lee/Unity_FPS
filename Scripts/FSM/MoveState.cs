using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Move State", menuName = "ScriptableObject/FPS State/Move", order = 2)]
public class MoveState : ScriptableObject, IState
{
    public void Enter(Enemy owner)
    {
        owner.SetPatrolMove();
    }
    public void Excute(Enemy owner)
    {
        if (owner.FindTarget()) owner.OnAttackState();
        else
        {
            if (owner.wallCheck()) owner.OnRotateState();
            else
            {
                if (owner.TimeCheck())
                { 
                    owner.Move();
                    return;
                }
                else owner.OnStopState();
            }
        }
    }
    public void Exit(Enemy owner)
    {
        owner.SetPatroMoveFinish();
    }
}