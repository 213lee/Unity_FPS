using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Enemy(subject)를 구독하는 EnemyManager(Observer)
public interface IEnemyDieObserver
{
    public void DestroyEnemy(Enemy enemy);
}

public class EnemyMgr : MonoBehaviour, IEnemyDieObserver
{
    [SerializeField] EnemyData[] enemyData = new EnemyData[3];     //Enemy가 가지는 공통 속성 데이터
    [SerializeField] StateData stateData;       //Enemy가 가지는 FSM state 데이터
    [SerializeField] List<Enemy> enemyList;     //Enemy를 List로 관리

    public event System.Action OnClearEvent = null;

    public void Initialize(Player player)
    {
        enemyList = new();
        SetEnemy(player);
    }

    public void SetEnemy(Player player, int stage = 0)
    {
        enemyList.Clear();
        //Scene에 있는 Enemy tag를 가진 gameobject를 모두 Find
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

    //Enemy가 죽었을 때 List에서 제거
    public void DestroyEnemy(Enemy enemy)
    {
        enemyList.Remove(enemy);
        //클리어 조건 적을 모두 처치했을 때
        if(enemyList.Count <= 0)
        {
            OnClearEvent?.Invoke();
        }
    }

    //Enemy를 하나만 남기는 치트키
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
