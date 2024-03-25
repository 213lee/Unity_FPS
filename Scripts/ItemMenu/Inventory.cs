using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void OnEquipEvent(Gun equip);                                                 //�κ��丮���� ������ �̷������ ���Ǵ� delegate
public delegate bool OneUsePotionEvent(POTIONTYPE type, float duration, float percentage);    //�κ��丮���� ������ ��������� ���Ǵ� delegate

public class Inventory : ItemMenu
{   
    [SerializeField] protected Dictionary<ITEMTYPE, List<Item>> itemDic;        //���� Ÿ���� �������� ������ ����Ʈ�� ��� Dictionary

    protected OnEquipEvent onEquipEvent;                       //�÷��̾�� ������ �����ϴ� �Լ��� ������ delegate
    protected OneUsePotionEvent onUsePotionEvent;              //�÷��̾�� ������ ����� �����ϴ� �Լ��� ������ delegate
    protected Gun equipGun = null;                             //���� �������� ��

    public virtual void Initialize(LivingEntity entity)
    {
        itemDic = new();
        itemDic[ITEMTYPE.WEAPON] = new List<Item>();
        itemDic[ITEMTYPE.POTION] = new List<Item>();
        itemDic[ITEMTYPE.AMMO] = new List<Item>();

        //Player�� ������ EquipWeapon �޽�带 Inventory���� �����ϱ� ����.
        onEquipEvent += entity.EquipWeapon;
        onUsePotionEvent += entity.UsePotion;
    }

    public virtual void StartSet()
    {
        foreach(var list in itemDic)
        {
            foreach(Item item in list.Value)
            {
                Destroy(item.gameObject);
            }
            list.Value.Clear();
        }
    }

    /*
     * Ammo List�� ��ȸ�ϸ鼭 ammoType�� ��ġ�ϴ� Ammo�� ��´�.
     */
    public Ammo FindAmmo(AMMOTYPE ammoType)
    {
        for (int i = 0; i < itemDic[ITEMTYPE.AMMO].Count; i++)
        {
            Ammo ammo = itemDic[ITEMTYPE.AMMO][i] as Ammo;
            if (ammo.Ammodata.AmmoType == ammoType)
                return ammo;
        }
        return null;
    }

    /*
     * Gun���� Reload�� ����ϴ� ammoType�� �޾Ƽ�
     * �ش� Ammo�� Amount�� Return
     */
    public int GetAmountByAmmoType(AMMOTYPE ammoType)
    {
        int amount = 0;
        Ammo ammo = FindAmmo(ammoType);
        if (ammo)
        {
            amount = ammo.Amount;
        }
        return amount;
    }


    /*
     * Shop���� ItemData�� �޾�
     * �ش� �������� ������ return�ϴ� �Լ�
     */
    public int GetAmount(ItemData itemData)
    {
        //Countable�������̶��
        if(itemData is CountableItemData)
        {
            //�ش� Dictionary���� �������� ã��
            CountableItem countableItem = itemDic[itemData.ItemType].Find(it => it.Data.ItemCode == itemData.ItemCode) as CountableItem;

            //�������� �ִٸ� ������ ��ȯ�Ѵ�.
            if(countableItem)
            {
                return countableItem.Amount;
            }

        }
        return 0;
    }
      

    /*
     * inventory���� ���� focusedItemData�� ��ġ�ϴ� �������� ������ return
     * GetAmount(ItemData itemData) �޼��带 �����ε��Ͽ� ���
     */
    public int GetAmount()
    {
        return GetAmount(focusedItemData);
    }

    /*
     * �κ��丮�� �������� �߰��ϴ� method
     * _itemData�� �κ��丮���� ������ �������� �˻��ϰ�
     * ���� �������� ����ų� ������ �߰�
     */
    public virtual bool AddItem(ItemData _itemData, int _amount = -1)
    {        
        int findIndex = itemDic[_itemData.ItemType].FindIndex(it => it.Data.ItemCode == _itemData.ItemCode);

        //_itemData�� ���� �������� ���ٸ� ���� �߰�
        if (findIndex == -1)
        {
            Item itemComponent = Instantiate(_itemData.Prefab, transform).GetComponent<Item>();
            itemComponent.Initialize();
            itemComponent.name = _itemData.name;
            itemComponent.gameObject.SetActive(false);

            //Dictionary�� Item�� ����
            itemDic[_itemData.ItemType].Add(itemComponent);
        }
        //�̹� ���� �Ѵٸ�
        else
        {
            //�� �� �ִ� �������϶� ������ ������Ű��
            if(itemDic[_itemData.ItemType][findIndex] is CountableItem countableItem)
            {
                countableItem.Add(_amount);
            }
            //�ϳ��� ������ �������̶�� false
            else return false;
        }
        return true;
    }

    //���� ��ư�� ������ �� ȣ��Ǵ� �Լ�
    /*
     * ���� ��ư�� ������ �� ȣ��Ǵ� �Լ�
     * ���õǾ� �ִ� �������� WEAPON �̶��
     * Player�� SetGun�� ������ delegate(OnEquipEvent)�� ������ ����
     */
    public virtual void EquipItem()
    {
        Item item = itemDic[focusedItemData.ItemType].Find(it => it.Data.ItemCode == focusedItemData.ItemCode);
        //���õ� �������� gun�̶�� ���� �޼��� ����
        if (item is Gun gun && gun != equipGun)
        {
            equipGun = gun;
            onEquipEvent(gun);
        }
    }

    /*
     * ���� ���� �� ����ص� itemData(Gun)�� ������ �����ϱ� ���� ���
     * EquipItem() �����ε�
     */
    public void EquipItem(ItemData itemData)
    {
        focusedItemData = itemData;
        EquipItem();
    }

    /*
     * Gun���� Reload�� ����ϴ� ammoType�� useAmount�� �޾Ƽ�
     * �ش� Ammo�� Amount�� ����
     * amount�� 0�̵Ǹ� ������Ʈ ���� - ����Ʈ���� ����
     */
    public virtual void UseAmmo(AMMOTYPE ammoType, int useAmount)
    {
        Ammo ammo = FindAmmo(ammoType);
        if (ammo)
        {
            ammo.Use(useAmount);

            if (ammo.Amount <= 0)
            {
                Destroy(ammo.gameObject);
                itemDic[ITEMTYPE.AMMO].Remove(ammo);
            }
        }
    }

    /*
     * ����(��� ������)�� ��������� ������ ȿ���� �÷��̾ �����ϱ� ���� ����ϴ� �޼���
     * ������ Use�� ȣ���Ҷ� LivingEntity�� UsePotion �޼��带 ���� delegate�� �Ű������� ���
     */
    public virtual void UsePotion()
    {
        Item item = itemDic[focusedItemData.ItemType].Find(it => it.Data.ItemCode == focusedItemData.ItemCode);
        if(item is Potion potion)
        {
            potion.Use(onUsePotionEvent);

            if (potion.Amount <= 0)
            {
                Destroy(potion.gameObject);
                itemDic[ITEMTYPE.POTION].Remove(potion);
            }
        }
    }
}
