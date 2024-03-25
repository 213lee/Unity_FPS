using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EQUIPCASE
{
    SUCCESS,
    FAILURE,
    COUNT
}

/*
 * Player의 경우 사용, 장착등이 이루어질 때 UI를 갱신하는등의 추가적인 작업을 위해 
 * Inventory를 상속받아 작성된 PlayerInventory
 */
public class PlayerInventory : Inventory
{
    public IPlayObserver playObserver = null;             //PlayUI
    public IInventoryObserver inventoryObserver = null;   //InventoryUI

    public int Money;   //플레이어의 인벤토리에 있는 게임 머니

    public void StartSet(ItemData startItemData)
    {
        StartSet();
        inventoryObserver?.StartSet();
        Money = 1000;
        AddItem(startItemData);
        EquipItem(startItemData);
    }

    /*
     * 플레이어 인벤토리에 필요한 UI(Play, Inventory) 옵저버 등록
     */
    public void SetObserver(IInventoryObserver _inventoryObserver)
    {
        base.SetObserver(_inventoryObserver);
        inventoryObserver = _inventoryObserver;
    }

    /*
     * Play UI에 게임 머니를 업데이트
     */
    public void UpdateMoney(int money = 0)
    {
        Money += money;
        playObserver.UpdateMoneyText(Money);
    }

    /*
     * Player Inventory에 Money를 추가하는 메서드
     * ex) Enemy Die -> OnDieEvent<int>로 호출
     */
    public void EarnByMoney(int money)
    {
        playObserver.KillEnemyMessage(money);
        UpdateMoney(money);
    }

    /*
     * 인벤토리에 아이템을 추가하는 메쏘드
     * _itemData으로 인벤토리에서 가지는 아이템을 검색하고
     * 새로 아이템을 만들거나 개수를 추가
     */
    public override bool AddItem(ItemData _itemData, int _amount = -1)
    {
        int findIndex = itemDic[_itemData.ItemType].FindIndex(it => it.Data.ItemCode == _itemData.ItemCode);

        //_itemData와 같은 아이템이 없다면 새로 추가
        if (findIndex == -1)
        {
            Item itemComponent = Instantiate(_itemData.Prefab, transform).GetComponent<Item>();
            itemComponent.Initialize();
            itemComponent.name = _itemData.name;
            itemComponent.gameObject.SetActive(false);
            inventoryObserver.Add(itemComponent, this);

            //Dictionary로 Item을 관리
            itemDic[_itemData.ItemType].Add(itemComponent);

            if (itemComponent is PlayerGun gun)
            {
                gun.playObserver = playObserver;
            }
            else if (itemComponent is Ammo ammo && ammo.Ammodata.AmmoType == equipGun.GunData.AmmoType)
            {
                playObserver.UpdateAmmoRemainText(ammo.Amount);
            }
        }
        //이미 존재 한다면
        else
        {
            //셀 수 있는 아이템일때 개수를 증가시키고
            if (itemDic[_itemData.ItemType][findIndex] is CountableItem countableItem)
            {
                countableItem.Add(_amount);

                //탄이라면 AmmoRemain을 갱신시킨다.
                if (countableItem is Ammo ammo && ammo.Ammodata.AmmoType == equipGun.GunData.AmmoType)
                {
                    playObserver.UpdateAmmoRemainText(ammo.Amount);
                }

                //Countable Item을 나타내는 Inventory Slot에서도 Amount 양을 갱신
                inventoryObserver.UpdateAmount(countableItem.Data.ItemCode, countableItem.Amount);
            }
            //하나만 가지는 아이템이라면 false
            else return false;
        }

        inventoryObserver.Show(_itemData);

        return true;
    }

    public override void EquipItem()
    {
        Item item = itemDic[focusedItemData.ItemType].Find(it => it.Data.ItemCode == focusedItemData.ItemCode);
        //선택된 아이템이 gun이라면 장착 메서드 수행
        if (item is Gun gun && gun != equipGun)
        {
            equipGun = gun;
            onEquipEvent(gun);
            inventoryObserver.EquipMessage(EQUIPCASE.SUCCESS, gun.Data.ItemName);
            playObserver.UpdateWeaponText(gun.GunData.ItemName);
            playObserver.UpdateAmmoRemainText(GetAmountByAmmoType(gun.GunData.AmmoType));
        }
        //아니라면 장착할 수 없다는 오류 출력 UI
        else
        {
            inventoryObserver.EquipMessage(EQUIPCASE.FAILURE);
        }
    }

    public override void UseAmmo(AMMOTYPE ammoType, int useAmount)
    {
        Ammo ammo = FindAmmo(ammoType);
        if (ammo)
        {
            ammo.Use(useAmount);

            //남은 탄을 갱신 UIMgr에서 AmmoRemainText, InventoryUI에서 UpdateSlotAmount
            playObserver.UpdateAmmoRemainText(ammo.Amount);
            inventoryObserver.UpdateAmount(ammo.Data.ItemCode, ammo.Amount);
            inventoryObserver.Show(focusedItemData);

            if (ammo.Amount <= 0)
            {
                Destroy(ammo.gameObject);
                itemDic[ITEMTYPE.AMMO].Remove(ammo);
                inventoryObserver.Remove(ammo.Data.ItemCode);
                if (ammo.Data.ItemCode == focusedItemData.ItemCode) inventoryObserver.ClearDetail();
            }
        }
    }

    /*
     * 포션(사용 가능한)을 사용했을때 아이템 효과를 플레이어에 적용하기 위해 사용
    */
    public override void UsePotion()
    {
        Item item = itemDic[focusedItemData.ItemType].Find(it => it.Data.ItemCode == focusedItemData.ItemCode);
        if (item is Potion potion)
        {
            inventoryObserver.UseMessage(potion.PotionData.PotionType, potion.Use(onUsePotionEvent));
            inventoryObserver.UpdateAmount(potion.Data.ItemCode, potion.Amount);
            inventoryObserver.Show(focusedItemData);

            if (potion.Amount <= 0)
            {
                Destroy(potion.gameObject);
                itemDic[ITEMTYPE.POTION].Remove(potion);
                inventoryObserver.Remove(potion.Data.ItemCode);
                inventoryObserver.Show(focusedItemData);

                if (potion.Data.ItemCode == focusedItemData.ItemCode) inventoryObserver.ClearDetail();
            }
        }
    }

}
