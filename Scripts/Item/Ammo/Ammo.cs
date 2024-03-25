using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Item을 상속받는 개수를 가지는 아이템
 */
public abstract class CountableItem : Item
{
    protected CountableItemData countableItemData;
    [SerializeField] protected int amount;

    public CountableItemData CountableItemData => countableItemData;
    public int Amount => amount;

    public override void Initialize()
    {
        data = CountableItemData;
        amount = CountableItemData.OneTimeSupply;
    }

    //_amount : 증가되는 값. (default. AmmoData의 OneTimeSupply만큼 증가시킴)
    public virtual void Add(int _amount = -1)
    {
        if (_amount == -1) amount += countableItemData.OneTimeSupply;
        else amount += _amount;
    }

    //Countable Item을 사용했을때 호출되어 개수를 갱신
    public virtual bool Use(int _amount = 1)
    {
        if (amount < _amount) return false;
        amount -= _amount;
        return true;
    }
}

public class Ammo : CountableItem
{
    [SerializeField] AmmoData ammodata;

    public AmmoData Ammodata => ammodata;

    public override void Initialize()
    {
        countableItemData = ammodata;
        base.Initialize();
    }
}
