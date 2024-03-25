using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void OnEquipEvent(Gun equip);                                                 //인벤토리에서 장착이 이루어질때 사용되는 delegate
public delegate bool OneUsePotionEvent(POTIONTYPE type, float duration, float percentage);    //인벤토리에서 포션을 사용했을때 사용되는 delegate

public class Inventory : ItemMenu
{   
    [SerializeField] protected Dictionary<ITEMTYPE, List<Item>> itemDic;        //같은 타입의 아이템을 가지는 리스트를 담는 Dictionary

    protected OnEquipEvent onEquipEvent;                       //플레이어에서 장착을 수행하는 함수를 가지는 delegate
    protected OneUsePotionEvent onUsePotionEvent;              //플레이어에서 아이템 사용을 수행하는 함수를 가지는 delegate
    protected Gun equipGun = null;                             //현재 장착중인 총

    public virtual void Initialize(LivingEntity entity)
    {
        itemDic = new();
        itemDic[ITEMTYPE.WEAPON] = new List<Item>();
        itemDic[ITEMTYPE.POTION] = new List<Item>();
        itemDic[ITEMTYPE.AMMO] = new List<Item>();

        //Player가 가지는 EquipWeapon 메쏘드를 Inventory에서 수행하기 위함.
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
     * Ammo List를 순회하면서 ammoType과 일치하는 Ammo를 얻는다.
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
     * Gun에서 Reload시 사용하는 ammoType을 받아서
     * 해당 Ammo의 Amount를 Return
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
     * Shop에서 ItemData를 받아
     * 해당 아이템의 개수를 return하는 함수
     */
    public int GetAmount(ItemData itemData)
    {
        //Countable아이템이라면
        if(itemData is CountableItemData)
        {
            //해당 Dictionary에서 아이템을 찾고
            CountableItem countableItem = itemDic[itemData.ItemType].Find(it => it.Data.ItemCode == itemData.ItemCode) as CountableItem;

            //아이템이 있다면 수량을 반환한다.
            if(countableItem)
            {
                return countableItem.Amount;
            }

        }
        return 0;
    }
      

    /*
     * inventory에서 현재 focusedItemData와 일치하는 아이템의 개수를 return
     * GetAmount(ItemData itemData) 메서드를 오버로딩하여 사용
     */
    public int GetAmount()
    {
        return GetAmount(focusedItemData);
    }

    /*
     * 인벤토리에 아이템을 추가하는 method
     * _itemData로 인벤토리에서 가지는 아이템을 검색하고
     * 새로 아이템을 만들거나 개수를 추가
     */
    public virtual bool AddItem(ItemData _itemData, int _amount = -1)
    {        
        int findIndex = itemDic[_itemData.ItemType].FindIndex(it => it.Data.ItemCode == _itemData.ItemCode);

        //_itemData와 같은 아이템이 없다면 새로 추가
        if (findIndex == -1)
        {
            Item itemComponent = Instantiate(_itemData.Prefab, transform).GetComponent<Item>();
            itemComponent.Initialize();
            itemComponent.name = _itemData.name;
            itemComponent.gameObject.SetActive(false);

            //Dictionary로 Item을 관리
            itemDic[_itemData.ItemType].Add(itemComponent);
        }
        //이미 존재 한다면
        else
        {
            //셀 수 있는 아이템일때 개수를 증가시키고
            if(itemDic[_itemData.ItemType][findIndex] is CountableItem countableItem)
            {
                countableItem.Add(_amount);
            }
            //하나만 가지는 아이템이라면 false
            else return false;
        }
        return true;
    }

    //장착 버튼을 눌렀을 때 호출되는 함수
    /*
     * 장착 버튼을 눌렀을 때 호출되는 함수
     * 선택되어 있는 아이템이 WEAPON 이라면
     * Player에 SetGun을 가지는 delegate(OnEquipEvent)로 장착을 수행
     */
    public virtual void EquipItem()
    {
        Item item = itemDic[focusedItemData.ItemType].Find(it => it.Data.ItemCode == focusedItemData.ItemCode);
        //선택된 아이템이 gun이라면 장착 메서드 수행
        if (item is Gun gun && gun != equipGun)
        {
            equipGun = gun;
            onEquipEvent(gun);
        }
    }

    /*
     * 게임 시작 시 등록해둔 itemData(Gun)로 장착을 수행하기 위해 사용
     * EquipItem() 오버로딩
     */
    public void EquipItem(ItemData itemData)
    {
        focusedItemData = itemData;
        EquipItem();
    }

    /*
     * Gun에서 Reload시 사용하는 ammoType과 useAmount를 받아서
     * 해당 Ammo의 Amount를 설정
     * amount가 0이되면 오브젝트 삭제 - 리스트에서 제거
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
     * 포션(사용 가능한)을 사용했을때 아이템 효과를 플레이어에 적용하기 위해 사용하는 메서드
     * 포션의 Use를 호출할때 LivingEntity의 UsePotion 메서드를 가진 delegate를 매개변수로 사용
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
