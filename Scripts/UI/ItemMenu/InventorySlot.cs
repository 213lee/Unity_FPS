using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/*
 * ItemMenuSlot�� ��ӹް� InventorySlot���� ����
 * Countable �������̶�� Amount�� �߰��� ����
 */
public class InventorySlot : ItemMenuSlot
{
    [SerializeField] TextMeshProUGUI amount;            //Amount Text

    public void Initialize(Item _item, ItemMenu _itemMenu)
    {
        itemdata = _item.Data;
        base.Initialize(_itemMenu);
        
        if (_item is Ammo ammo) amount.text = ammo.Amount.ToString();
    }

    public void UpdateAmount(int updateAmount)
    {
        amount.text = updateAmount.ToString();
    }
}
