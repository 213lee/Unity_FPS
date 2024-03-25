using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Die State", menuName = "ScriptableObject/FPS State/Die", order = 4)]
public class DieState : ScriptableObject, IState
{
    public void Enter(Enemy owner)
    {
        owner.SetDie();
    }

    public void Excute(Enemy owner)
    {
        if (owner.TimeCheck()) return;
        owner.Die();
    }

    public void Exit(Enemy owner)
    {
    }
}
