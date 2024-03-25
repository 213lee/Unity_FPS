using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Attack State", menuName = "ScriptableObject/FPS State/Attack", order = 3)]
public class AttackState : ScriptableObject, IState
{
    //Enter 선딜레이 타이머 SET
    public void Enter(Enemy owner)
    {
        owner.SetAttack();
    }

    //Excute에서 선딜레이 타이머 종료되면 AttackToPlayer
    //AttakToPlayer 호출되면서 후딜레이 타이머 세팅, isAttackFinished 플래그 설정
    //후딜 끝나면서 StopState로 Change
    public void Excute(Enemy owner)
    {
        //딜레이 타이머
        if (owner.TimeCheck()) return;
        else
        {
            //공격이 끝난 후(후딜) 타이머를 통과하면 후딜레이 종료
            if (owner.isAttackFinished)
            {
                //후딜레이 종료 후 남은 탄창에 탄이 없을 때 
                if (owner.AmmoCheck()) owner.OnReloadState();
                else if (owner.FindTarget()) owner.OnAttackState();
                else owner.OnStopState();
            }
            //공격
            else owner.AttackToPlayer();
        }
    }
    public void Exit(Enemy owner)
    {
        owner.AttackFinished();
    }
}
