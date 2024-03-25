using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//제네릭을 사용하지 않는 오브젝트풀링 방식
//나중에 내 방식에 따라 맞는 방법 선택

public abstract class PoolableObject : MonoBehaviour
{
    public virtual bool IsUsing
    {
        get => gameObject.activeSelf;
        set => gameObject.SetActive(value);
    }

    public Transform Parent
    {
        get => transform.parent;
        set => transform.SetParent(value);
    }

}
    

//제네릭을 사용하지 않으면 모든 풀을 풀 매니져에서 관리가 가능.
public class PoolMgr : MonoBehaviour
{
    class ObjectPool
    {
        public PoolableObject Original { get; private set; }
        public Transform Container { get; private set; }
        Queue<PoolableObject> pool = new();

        public bool Initialize(PoolableObject original, uint count = 1u)
        {
            if (!original) return false;

            Original = original;
            Container = new GameObject($"{original.name} Container").transform;
            Container.SetParent(rootContainer);

            if (0u == count) count = 1u;
            for (uint i = 0; i < count; i++) Push(CreateObject());

            return true;
        }

        PoolableObject CreateObject()
        {
            PoolableObject poolObj = Object.Instantiate(Original);
            poolObj.name = Original.name;

            return poolObj;
        }

        public bool Push(PoolableObject poolableObject)
        {
            if (!poolableObject || (Original.name != poolableObject.name)) return false;

            poolableObject.Parent = Container;
            poolableObject.IsUsing = false;

            pool.Enqueue(poolableObject);

            return true;
        }

        public PoolableObject Pop(Transform parent = null)
        {
            PoolableObject poolableObject = null;

            if (0 < pool.Count) poolableObject = pool.Dequeue();
            else poolableObject = CreateObject();

            poolableObject.Parent = parent;
            poolableObject.IsUsing = true;

            return poolableObject;
        }

        public void ReturnObjectsToPool()
        {
            foreach (Transform child in Container)
            {
                if (child.gameObject.activeSelf)
                {
                    Push(child.GetComponent<PoolableObject>());
                }
            }
        }

        public void ReturnObjectsToPool(Transform popRoot = null)
        {
            Transform root = popRoot != null ? popRoot : Container;
            foreach (Transform child in popRoot)
            {
                if (child.gameObject.activeSelf)
                {
                    Push(child.GetComponent<PoolableObject>());
                }
            }
        }
    }


    static Transform rootContainer;
    Dictionary<string, ObjectPool> objectPools = new();

    public void Initialize()
    {
        if (rootContainer) throw new UnityException("Already initialized");
        rootContainer = new GameObject("Object Pool Root Container").transform;
        rootContainer.SetParent(transform);
    }

    public Transform CreatePool(PoolableObject original, uint count = 1u)
    {
        if (!rootContainer) throw new UnityException("Not initialized");
        if (objectPools.ContainsKey(original.name)) throw new UnityException($"Object pool already created : {original.name}");

        ObjectPool pool = new();

        pool.Initialize(original, count);

        objectPools.Add(original.name, pool);

        return pool.Container;
    }

    public bool Push(PoolableObject poolableObject)
    {
        if (objectPools.ContainsKey(poolableObject.name))
        {
            objectPools[poolableObject.name].Push(poolableObject);
            return true;
        }
        return false;
    }

    public PoolableObject Pop(string name, Transform parent = null)
    {
        if (objectPools.ContainsKey(name)) return objectPools[name].Pop(parent);
        return null;
    }

    //name(objectPools.Key)을 받아 해당 Pool만 return
    public void ReturnToPool(string name, Transform popRoot = null)
    {
        if (objectPools.TryGetValue(name, out ObjectPool pool))
        {
            pool.ReturnObjectsToPool(popRoot);
        }
    }

    public void ReturnAllToPool()
    {
        foreach (ObjectPool pool in objectPools.Values)
        {
            pool.ReturnObjectsToPool();
        }
    }

    public void Clear()
    {
        Object.Destroy(rootContainer.gameObject);
        rootContainer = null;

        objectPools.Clear();

        Initialize();
    }
}
