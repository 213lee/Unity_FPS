using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/*
 * ItemMenuSlot을 상속받고 InventorySlot에만 들어가는
 * Countable 아이템이라면 Amount를 추가로 설정
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
