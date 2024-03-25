using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Shop, Invnetory�� �������� ������ �θ� class
public class ItemMenu : MonoBehaviour
{
    //�ش� ItemMenu�� ��ϵǴ� UI Observer
    protected IItemMenuObserver itemMenuObserver;

    //���� ���õǾ� �ִ� ������
    protected ItemData focusedItemData = null;

    //Slot�� ���õǾ����� �ش� �������� ItemMenu���� ������, UI�� ǥ��
    public virtual void OnSlotSelected(ItemData itemData)
    {
        focusedItemData = itemData;
        ShowDetail();
    }

    public void ShowDetail()
    {
        if (!focusedItemData) return;
        itemMenuObserver?.Show(focusedItemData);
    }

    public virtual void SetObserver(IItemMenuObserver _itemMenuObserver)
    {
        itemMenuObserver = _itemMenuObserver;
    }
}

/*
 * ItemMenu���� UI�� ǥ���� ErroType
 * 1(�������� ���� ��ư�� ������ ��)
 * 2(�κ��丮���� ��� ��ư�� ������ ��)
 * Error Type
 * 1 - a �÷��̾ �����Ϸ��� �������� ������ ���ӸӴϺ��� ��� �� (ex : ���ӸӴϰ� �����մϴ�.)
 * 1 - b �÷��̾� �κ��丮�� �ߺ����� �ʴ� ���� �̹� ������ �� (ex : �ش� �������� ���̻� ���� �� �����ϴ�.)
 */


public enum BUYCASE
{
    FAILURE_MONEY,      // 1 - a
    FAILURE_EXIST,      // 1 - b
    SUCCESS,            
    COUNT
}

/*
 * ShopObserver�� subject
 * �÷��̾��� inventory�� ������.
 */
public class Shop : ItemMenu
{
    [SerializeField] PlayerInventory playerInventory;      //Player�� Inventory

    public IShopObserver shopObserver;
    
    public void SetObserver(IShopObserver _shopObserver)
    {
        base.SetObserver(_shopObserver);
        shopObserver = _shopObserver;
    }

    //Player�� inventory���� �������� ������ �޾ƿ��� �޼���
    public int GetItemAmount()
    {
        return playerInventory.GetAmount(focusedItemData);
    }

    //Buy
    public void Buy()
    {
        //�κ��丮�� ������ ���ӸӴϺ��� ������ �������� ������ ��δٸ�
        if (focusedItemData.Price > playerInventory.Money)
        {
            //������ ���ӸӴϰ� ������ �� (1-a)
            shopObserver.BuyMessage(BUYCASE.FAILURE_MONEY);
        }
        else
        {
            //�κ��丮�� �������� �߰��ϰ�
            //focusedItem�� �� �� ���� �������̰� �̹� �����Ѵٸ� false
            if (!playerInventory.AddItem(focusedItemData))
            {
                //�ߺ��Ǵ� �������� �� (1-b)
                shopObserver.BuyMessage(BUYCASE.FAILURE_EXIST);
            }
            //������ ���� ����
            else
            {
                //�������� ���ݸ�ŭ �κ��丮���� ���ӸӴ� ���� -> UI Update.
                playerInventory.UpdateMoney(-focusedItemData.Price);
             
                //������Ʈ�� ������ ������ ���
                itemMenuObserver.Show(focusedItemData);

                //������ ���ſϷ� UI
                shopObserver.BuyMessage(BUYCASE.SUCCESS, focusedItemData.ItemName);
            }
        }
    }

}
