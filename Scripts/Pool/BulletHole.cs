using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletHole : PoolableObject
{
    public void OnEnable()
    {
        StartCoroutine(PushTimer());
    }

    //Ȱ��ȭ �ǰ� ������ �ð��� ������ Push
    IEnumerator PushTimer()
    {
        yield return new WaitForSeconds(3.0f);
        GameMgr.Instance.PoolMgr.Push(this);
    }

    public void SetPosition(Vector3 pos)
    {
        transform.position = pos;
    }
}
