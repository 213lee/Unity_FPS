using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Rotate State", menuName = "ScriptableObject/FPS State/Rotate", order = 3)]
public class RotateState : ScriptableObject, IState
{
    public void Enter(Enemy owner)
    {
        owner.SetPatrolRotate();
    }
    public void Excute(Enemy owner)
    {
        if (owner.FindTarget())
        {
            owner.OnAttackState();
        }
        else
        {            
            if (owner.isRotating) return;
            else owner.OnMoveState();
        }
    }
    public void Exit(Enemy owner)
    {
        owner.StopRotate();
    }
}