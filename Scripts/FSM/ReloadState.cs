using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Reload State", menuName = "ScriptableObject/FPS State/Reload", order = 4)]
public class ReloadState : ScriptableObject, IState
{
    
    public void Enter(Enemy owner)
    {
        owner.SetReload();
    }
    public void Excute(Enemy owner)
    {
        //딜레이 타이머
        if (owner.TimeCheck()) return;
        else owner.OnStopState();
    }
    public void Exit(Enemy owner)
    {
        
    }
}
