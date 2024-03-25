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
 * Player�� ��� ���, �������� �̷���� �� UI�� �����ϴµ��� �߰����� �۾��� ���� 
 * Inventory�� ��ӹ޾� �ۼ��� PlayerInventory
 */
public class PlayerInventory : Inventory
{
    public IPlayObserver playObserver = null;             //PlayUI
    public IInventoryObserver inventoryObserver = null;   //InventoryUI

    public int Money;   //�÷��̾��� �κ��丮�� �ִ� ���� �Ӵ�

    public void StartSet(ItemData startItemData)
    {
        StartSet();
        inventoryObserver?.StartSet();
        Money = 1000;
        AddItem(startItemData);
        EquipItem(startItemData);
    }

    /*
     * �÷��̾� �κ��丮�� �ʿ��� UI(Play, Inventory) ������ ���
     */
    public void SetObserver(IInventoryObserver _inventoryObserver)
    {
        base.SetObserver(_inventoryObserver);
        inventoryObserver = _inventoryObserver;
    }

    /*
     * Play UI�� ���� �Ӵϸ� ������Ʈ
     */
    public void UpdateMoney(int money = 0)
    {
        Money += money;
        playObserver.UpdateMoneyText(Money);
    }

    /*
     * Player Inventory�� Money�� �߰��ϴ� �޼���
     * ex) Enemy Die -> OnDieEvent<int>�� ȣ��
     */
    public void EarnByMoney(int money)
    {
        playObserver.KillEnemyMessage(money);
        UpdateMoney(money);
    }

    /*
     * �κ��丮�� �������� �߰��ϴ� �޽��
     * _itemData���� �κ��丮���� ������ �������� �˻��ϰ�
     * ���� �������� ����ų� ������ �߰�
     */
    public override bool AddItem(ItemData _itemData, int _amount = -1)
    {
        int findIndex = itemDic[_itemData.ItemType].FindIndex(it => it.Data.ItemCode == _itemData.ItemCode);

        //_itemData�� ���� �������� ���ٸ� ���� �߰�
        if (findIndex == -1)
        {
            Item itemComponent = Instantiate(_itemData.Prefab, transform).GetComponent<Item>();
            itemComponent.Initialize();
            itemComponent.name = _itemData.name;
            itemComponent.gameObject.SetActive(false);
            inventoryObserver.Add(itemComponent, this);

            //Dictionary�� Item�� ����
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
        //�̹� ���� �Ѵٸ�
        else
        {
            //�� �� �ִ� �������϶� ������ ������Ű��
            if (itemDic[_itemData.ItemType][findIndex] is CountableItem countableItem)
            {
                countableItem.Add(_amount);

                //ź�̶�� AmmoRemain�� ���Ž�Ų��.
                if (countableItem is Ammo ammo && ammo.Ammodata.AmmoType == equipGun.GunData.AmmoType)
                {
                    playObserver.UpdateAmmoRemainText(ammo.Amount);
                }

                //Countable Item�� ��Ÿ���� Inventory Slot������ Amount ���� ����
                inventoryObserver.UpdateAmount(countableItem.Data.ItemCode, countableItem.Amount);
            }
            //�ϳ��� ������ �������̶�� false
            else return false;
        }

        inventoryObserver.Show(_itemData);

        return true;
    }

    public override void EquipItem()
    {
        Item item = itemDic[focusedItemData.ItemType].Find(it => it.Data.ItemCode == focusedItemData.ItemCode);
        //���õ� �������� gun�̶�� ���� �޼��� ����
        if (item is Gun gun && gun != equipGun)
        {
            equipGun = gun;
            onEquipEvent(gun);
            inventoryObserver.EquipMessage(EQUIPCASE.SUCCESS, gun.Data.ItemName);
            playObserver.UpdateWeaponText(gun.GunData.ItemName);
            playObserver.UpdateAmmoRemainText(GetAmountByAmmoType(gun.GunData.AmmoType));
        }
        //�ƴ϶�� ������ �� ���ٴ� ���� ��� UI
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

            //���� ź�� ���� UIMgr���� AmmoRemainText, InventoryUI���� UpdateSlotAmount
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
     * ����(��� ������)�� ��������� ������ ȿ���� �÷��̾ �����ϱ� ���� ���
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
