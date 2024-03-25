using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/*
 * ShopSlot, InventorySlot이 상속받는 부모 class
 * 각 Slot은 itemData를 가지고
 * itemData를 통해 UI를 Initialize한다.
 * Slot은 버튼이고 버튼이 선택되면 
 * 해당 ItemMenu(Shop.cs, Inventory.cs)의 focusedItemData를 갱신시킨다 
 */
public class ItemMenuSlot : MonoBehaviour
{
    [SerializeField] protected ItemData itemdata;
    [SerializeField] TextMeshProUGUI itemName;
    [SerializeField] Image icon;
    [SerializeField] Button btn;

    public ItemData itemData => itemdata;

    //아이템 슬롯에 들어갈 이미지, 이름 Init
    //Slot의 Click 이벤트에 itemMenu의 메서드를 등록
    public virtual void Initialize(ItemMenu itemMenu)
    {
        itemName.text = itemdata.ItemName;
        icon.sprite = itemdata.Icon;
        TryGetComponent(out btn);
        btn.onClick.AddListener(delegate { itemMenu.OnSlotSelected(itemdata); });
    }
}

/*
 * ItemMenuSlot을 상속받고 ShopSlot에만 들어가는 Price를 추가로 Initialize
 */
public class ShopSlot : ItemMenuSlot
{
    [SerializeField] TextMeshProUGUI price;             //Price Text

    public override void Initialize(ItemMenu itemMenu)
    {
        base.Initialize(itemMenu);
        price.text = itemData.Price.ToString();        
    }
}
