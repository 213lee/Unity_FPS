using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Item�� ��ӹ޴� ������ ������ ������
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

    //_amount : �����Ǵ� ��. (default. AmmoData�� OneTimeSupply��ŭ ������Ŵ)
    public virtual void Add(int _amount = -1)
    {
        if (_amount == -1) amount += countableItemData.OneTimeSupply;
        else amount += _amount;
    }

    //Countable Item�� ��������� ȣ��Ǿ� ������ ����
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
