using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Shop, Invnetory가 공통으로 가지는 부모 class
public class ItemMenu : MonoBehaviour
{
    //해당 ItemMenu에 등록되는 UI Observer
    protected IItemMenuObserver itemMenuObserver;

    //현재 선택되어 있는 아이템
    protected ItemData focusedItemData = null;

    //Slot이 선택되었을때 해당 아이템을 ItemMenu에서 가지고, UI에 표시
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
 * ItemMenu에서 UI로 표시할 ErroType
 * 1(상점에서 구매 버튼을 눌렀을 때)
 * 2(인벤토리에서 사용 버튼을 눌렀을 때)
 * Error Type
 * 1 - a 플레이어가 구매하려는 아이템이 보유한 게임머니보다 비쌀 때 (ex : 게임머니가 부족합니다.)
 * 1 - b 플레이어 인벤토리에 중복되지 않는 템이 이미 존재할 때 (ex : 해당 아이템을 더이상 가질 수 없습니다.)
 */


public enum BUYCASE
{
    FAILURE_MONEY,      // 1 - a
    FAILURE_EXIST,      // 1 - b
    SUCCESS,            
    COUNT
}

/*
 * ShopObserver의 subject
 * 플레이어의 inventory를 가진다.
 */
public class Shop : ItemMenu
{
    [SerializeField] PlayerInventory playerInventory;      //Player의 Inventory

    public IShopObserver shopObserver;
    
    public void SetObserver(IShopObserver _shopObserver)
    {
        base.SetObserver(_shopObserver);
        shopObserver = _shopObserver;
    }

    //Player의 inventory에서 아이템의 개수를 받아오는 메서드
    public int GetItemAmount()
    {
        return playerInventory.GetAmount(focusedItemData);
    }

    //Buy
    public void Buy()
    {
        //인벤토리에 소지한 게임머니보다 선택한 아이템의 가격이 비싸다면
        if (focusedItemData.Price > playerInventory.Money)
        {
            //보유한 게임머니가 부족할 때 (1-a)
            shopObserver.BuyMessage(BUYCASE.FAILURE_MONEY);
        }
        else
        {
            //인벤토리에 아이템을 추가하고
            //focusedItem이 셀 수 없는 아이템이고 이미 존재한다면 false
            if (!playerInventory.AddItem(focusedItemData))
            {
                //중복되는 아이템일 때 (1-b)
                shopObserver.BuyMessage(BUYCASE.FAILURE_EXIST);
            }
            //아이템 구매 성공
            else
            {
                //아이템의 가격만큼 인벤토리에서 게임머니 차감 -> UI Update.
                playerInventory.UpdateMoney(-focusedItemData.Price);
             
                //업데이트된 정보로 아이템 출력
                itemMenuObserver.Show(focusedItemData);

                //아이템 구매완료 UI
                shopObserver.BuyMessage(BUYCASE.SUCCESS, focusedItemData.ItemName);
            }
        }
    }

}
