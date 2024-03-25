using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/*
 * ShopSlot, InventorySlot�� ��ӹ޴� �θ� class
 * �� Slot�� itemData�� ������
 * itemData�� ���� UI�� Initialize�Ѵ�.
 * Slot�� ��ư�̰� ��ư�� ���õǸ� 
 * �ش� ItemMenu(Shop.cs, Inventory.cs)�� focusedItemData�� ���Ž�Ų�� 
 */
public class ItemMenuSlot : MonoBehaviour
{
    [SerializeField] protected ItemData itemdata;
    [SerializeField] TextMeshProUGUI itemName;
    [SerializeField] Image icon;
    [SerializeField] Button btn;

    public ItemData itemData => itemdata;

    //������ ���Կ� �� �̹���, �̸� Init
    //Slot�� Click �̺�Ʈ�� itemMenu�� �޼��带 ���
    public virtual void Initialize(ItemMenu itemMenu)
    {
        itemName.text = itemdata.ItemName;
        icon.sprite = itemdata.Icon;
        TryGetComponent(out btn);
        btn.onClick.AddListener(delegate { itemMenu.OnSlotSelected(itemdata); });
    }
}

/*
 * ItemMenuSlot�� ��ӹް� ShopSlot���� ���� Price�� �߰��� Initialize
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
