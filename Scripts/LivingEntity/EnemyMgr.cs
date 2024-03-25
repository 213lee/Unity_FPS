using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Enemy(subject)�� �����ϴ� EnemyManager(Observer)
public interface IEnemyDieObserver
{
    public void DestroyEnemy(Enemy enemy);
}

public class EnemyMgr : MonoBehaviour, IEnemyDieObserver
{
    [SerializeField] EnemyData[] enemyData = new EnemyData[3];     //Enemy�� ������ ���� �Ӽ� ������
    [SerializeField] StateData stateData;       //Enemy�� ������ FSM state ������
    [SerializeField] List<Enemy> enemyList;     //Enemy�� List�� ����

    public event System.Action OnClearEvent = null;

    public void Initialize(Player player)
    {
        enemyList = new();
        SetEnemy(player);
    }

    public void SetEnemy(Player player, int stage = 0)
    {
        enemyList.Clear();
        //Scene�� �ִ� Enemy tag�� ���� gameobject�� ��� Find
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject obj in objs)
        {
            if (obj.transform.TryGetComponent<Enemy>(out Enemy enemy))
            {
                enemy.Initialize(enemyData[stage], stateData, this, player);
                enemyList.Add(enemy);
            }
        }
    }

    //Enemy�� �׾��� �� List���� ����
    public void DestroyEnemy(Enemy enemy)
    {
        enemyList.Remove(enemy);
        //Ŭ���� ���� ���� ��� óġ���� ��
        if(enemyList.Count <= 0)
        {
            OnClearEvent?.Invoke();
        }
    }

    //Enemy�� �ϳ��� ����� ġƮŰ
    public void LeaveOneEnemy()
    {
        if (enemyList.Count <= 1) return;

        for (int i = enemyList.Count - 1; i > 0; i--) 
        {
            enemyList[i].SetActive(false);
            enemyList.RemoveAt(i);
        }
    }

}
