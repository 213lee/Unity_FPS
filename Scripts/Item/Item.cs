using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : MonoBehaviour
{
    protected ItemData data;

    public ItemData Data => data;

    public abstract void Initialize();
}
