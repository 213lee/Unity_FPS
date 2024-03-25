using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletHole : PoolableObject
{
    public void OnEnable()
    {
        StartCoroutine(PushTimer());
    }

    //활성화 되고 지정한 시간이 지나면 Push
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
