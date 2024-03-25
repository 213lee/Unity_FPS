using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IInventoryObserver : IItemMenuObserver
{
    public void Add(Item item, ItemMenu itemMenu);
    public void UpdateAmount(ITEMCODE itemCode, int updateAmount);
    public void Remove(ITEMCODE code);
    public void StartSet();
    public void ClearDetail();
    public void EquipMessage(EQUIPCASE success, string gunName = "");
    public void UseMessage(POTIONTYPE type, bool use);
}

public enum DETAILBTNTYPE
{
    BUY,
    EQUIP,
    USE,
    COUNT
}
/*
 * Inventory의 Observer
 * inventory에서 가지는 아이템과 같이 정렬된 슬롯을 가진다.
 */
public class InventoryUI : ItemMenuUI, IInventoryObserver
{
    [SerializeField] GameObject inventorySlotPrefab;     //InventorySlot의 프리팹

    [SerializeField] PlayerInventory inventory;

    [Header("Inventory Slot Tr")]
    [SerializeField] Transform inventoryContent;         //list에 활성화된 slot을 담는 영역

    SortedList<ITEMCODE, InventorySlot> sortedInvetorySlotList;     //InventorySlot을 ITEMCODE 오름차순으로 정렬하여 가지는 List

    [SerializeField] string[] equipMessageText = new string[(int)EQUIPCASE.COUNT];
    [SerializeField] string[] useSuccessText   = new string[(int)POTIONTYPE.COUNT];
    [SerializeField] string[] useFailureText   = new string[(int)POTIONTYPE.COUNT];

    public override void Initialize()
    {
        base.Initialize();
        sortedInvetorySlotList = new();
        inventory.SetObserver(this);
        detailBtn.gameObject.SetActive(false);
        detailBtnText = detailBtn.GetComponentInChildren<TextMeshProUGUI>();

        equipMessageText[(int)EQUIPCASE.SUCCESS]           = "장착";
        equipMessageText[(int)EQUIPCASE.FAILURE]           = "이미 장착 중입니다";

        useSuccessText[(int)POTIONTYPE.HP]                 = "Hp가 회복되었습니다";
        useSuccessText[(int)POTIONTYPE.BUFF_SPEED]         = "속도가 증가합니다";
        useSuccessText[(int)POTIONTYPE.BUFF_INVINCIBLE]    = "무적상태가 됩니다";

        useFailureText[(int)POTIONTYPE.HP]                 = "이미 Hp가 최대치 입니다";
        useFailureText[(int)POTIONTYPE.BUFF_SPEED]         = "이미 속도 증가 상태입니다";
        useFailureText[(int)POTIONTYPE.BUFF_INVINCIBLE]    = "이미 무적 상태입니다";
    }


    //정렬된 inventory slot의 순서에 맞게 각 slot.transform을 정렬한다.
    private void SlotTransformSiblingSort()
    {
        for(int i=0; i< sortedInvetorySlotList.Count; i++)
        {
            sortedInvetorySlotList.Values[i].transform.SetSiblingIndex(i);
        }
    }

    public override void Show(ItemData itemData)
    {
        base.Show(itemData);

        //아이템 타입에 따라 사용 버튼 활성화 여부, 아이템 설명 결정 
        switch (itemData.ItemType)
        {
            case ITEMTYPE.WEAPON:
                GunData gun = itemData as GunData;
                strBuilder.Clear();
                strBuilder.Append(gunDescriptionText[(int)GUNDESCRIPTIONTYPE.ATTAK_POWER]);
                strBuilder.Append(gun.AtkPower.ToString());
                strBuilder.Append(enter);

                strBuilder.Append(gunDescriptionText[(int)GUNDESCRIPTIONTYPE.HIT_RANGE]);
                strBuilder.Append(gun.HitRange.ToString());
                strBuilder.Append(enter);

                strBuilder.Append(gunDescriptionText[(int)GUNDESCRIPTIONTYPE.AMMO_TYPE]);
                strBuilder.Append(ammoTypeText[(int)gun.AmmoType]);
                itemDescription.text = strBuilder.ToString();

                itemReserves.text = blank;

                //EQUIP Btn Set
                detailBtn.onClick.RemoveAllListeners();
                detailBtn.onClick.AddListener(inventory.EquipItem);
                detailBtnText.text = btnTypeText[(int)DETAILBTNTYPE.EQUIP];
                detailBtn.gameObject.SetActive(true);
                break;
            case ITEMTYPE.POTION:
                CountableItemShow(itemData.Description);

                //Use Btn Set
                detailBtn.onClick.RemoveAllListeners();
                detailBtn.onClick.AddListener(inventory.UsePotion);
                detailBtnText.text = btnTypeText[(int)DETAILBTNTYPE.USE];
                detailBtn.gameObject.SetActive(true);
                break;
            case ITEMTYPE.AMMO:
                CountableItemShow(itemData.Description);

                //Use Btn  Inactive
                detailBtn.gameObject.SetActive(false);
                break;
        }
    }


    //Countable Item은 Description, Reserves를 같은 방식으로 출력
    protected override void CountableItemShow(string Description)
    {
        base.CountableItemShow(Description);
        //Reserves
        strBuilder.Clear();
        strBuilder.Append(reserves);
        strBuilder.Append(inventory.GetAmount().ToString());

        itemReserves.text = strBuilder.ToString();
    }

    //InventorySlot은 ItemMenuSlot을 상속받지만 동적으로 생성하기 때문에 추가로 초기화 필요
    public void Add(Item item, ItemMenu itemMenu)
    {
        InventorySlot slot = Instantiate(inventorySlotPrefab, inventoryContent).GetComponent<InventorySlot>();
        if(slot)
        {
            slot.Initialize(item, itemMenu);
            slot.name = item.name;

            sortedInvetorySlotList.Add(item.Data.ItemCode, slot);
            SlotTransformSiblingSort();
        }
    }

    //Inventory에 추가되거나 사용될 때 개수를 갱신
    public void UpdateAmount(ITEMCODE itemCode, int updateAmount)
    {
        if(sortedInvetorySlotList[itemCode])
        {
            sortedInvetorySlotList[itemCode].UpdateAmount(updateAmount);
            SlotTransformSiblingSort();
        }
    }

    //해당 아이템 코드(code)를 가진 아이템 슬롯 제거 -> 리스트에서 삭제
    public void Remove(ITEMCODE code)
    {
        Destroy(sortedInvetorySlotList[code].gameObject);
        sortedInvetorySlotList.Remove(code);
    }

    //focusedItem과 같은 아이템이 지워졌을 때 Detail Clear
    public void ClearDetail()
    {
        detailImg.gameObject.SetActive(false);
        detailBtn.gameObject.SetActive(false);
        itemName.text = blank;
        itemDescription.text = blank;
        itemReserves.text = blank;
    }

    //게임이 시작될 때 인벤토리 리스트를 Clear하는 설정
    public void StartSet()
    {
        foreach(var slot in sortedInvetorySlotList)
        {
            Destroy(slot.Value.gameObject);
        }
        sortedInvetorySlotList.Clear();
    }

    public override void OnMenuStartUpdate()
    {
        inventory.ShowDetail();
    }

    //장비 장착 메시지 출력
    public void EquipMessage(EQUIPCASE equipCase, string gunName = "")
    {
        strBuilder.Clear();
        Color color = failureColor;
        if(equipCase == EQUIPCASE.SUCCESS)
        {
            strBuilder.Append(gunName);
            strBuilder.Append(blank);
            color = successColor;
        }
        strBuilder.Append(equipMessageText[(int)equipCase]);
        messageObserver.ShowMessage(strBuilder.ToString(), color);
    }

    //포션 사용 메시지 출력
    public void UseMessage(POTIONTYPE type, bool use)
    {
        strBuilder.Clear();
        Color color = failureColor;
        if (use)
        {
            strBuilder.Append(useSuccessText[(int)type]);
            color = successColor;
        }
        else strBuilder.Append(useFailureText[(int)type]);
        messageObserver.ShowMessage(strBuilder.ToString(), color);
    }
}
