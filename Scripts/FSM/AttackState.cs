using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Attack State", menuName = "ScriptableObject/FPS State/Attack", order = 3)]
public class AttackState : ScriptableObject, IState
{
    //Enter �������� Ÿ�̸� SET
    public void Enter(Enemy owner)
    {
        owner.SetAttack();
    }

    //Excute���� �������� Ÿ�̸� ����Ǹ� AttackToPlayer
    //AttakToPlayer ȣ��Ǹ鼭 �ĵ����� Ÿ�̸� ����, isAttackFinished �÷��� ����
    //�ĵ� �����鼭 StopState�� Change
    public void Excute(Enemy owner)
    {
        //������ Ÿ�̸�
        if (owner.TimeCheck()) return;
        else
        {
            //������ ���� ��(�ĵ�) Ÿ�̸Ӹ� ����ϸ� �ĵ����� ����
            if (owner.isAttackFinished)
            {
                //�ĵ����� ���� �� ���� źâ�� ź�� ���� �� 
                if (owner.AmmoCheck()) owner.OnReloadState();
                else if (owner.FindTarget()) owner.OnAttackState();
                else owner.OnStopState();
            }
            //����
            else owner.AttackToPlayer();
        }
    }
    public void Exit(Enemy owner)
    {
        owner.AttackFinished();
    }
}
