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
 * Inventory�� Observer
 * inventory���� ������ �����۰� ���� ���ĵ� ������ ������.
 */
public class InventoryUI : ItemMenuUI, IInventoryObserver
{
    [SerializeField] GameObject inventorySlotPrefab;     //InventorySlot�� ������

    [SerializeField] PlayerInventory inventory;

    [Header("Inventory Slot Tr")]
    [SerializeField] Transform inventoryContent;         //list�� Ȱ��ȭ�� slot�� ��� ����

    SortedList<ITEMCODE, InventorySlot> sortedInvetorySlotList;     //InventorySlot�� ITEMCODE ������������ �����Ͽ� ������ List

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

        equipMessageText[(int)EQUIPCASE.SUCCESS]           = "����";
        equipMessageText[(int)EQUIPCASE.FAILURE]           = "�̹� ���� ���Դϴ�";

        useSuccessText[(int)POTIONTYPE.HP]                 = "Hp�� ȸ���Ǿ����ϴ�";
        useSuccessText[(int)POTIONTYPE.BUFF_SPEED]         = "�ӵ��� �����մϴ�";
        useSuccessText[(int)POTIONTYPE.BUFF_INVINCIBLE]    = "�������°� �˴ϴ�";

        useFailureText[(int)POTIONTYPE.HP]                 = "�̹� Hp�� �ִ�ġ �Դϴ�";
        useFailureText[(int)POTIONTYPE.BUFF_SPEED]         = "�̹� �ӵ� ���� �����Դϴ�";
        useFailureText[(int)POTIONTYPE.BUFF_INVINCIBLE]    = "�̹� ���� �����Դϴ�";
    }


    //���ĵ� inventory slot�� ������ �°� �� slot.transform�� �����Ѵ�.
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

        //������ Ÿ�Կ� ���� ��� ��ư Ȱ��ȭ ����, ������ ���� ���� 
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


    //Countable Item�� Description, Reserves�� ���� ������� ���
    protected override void CountableItemShow(string Description)
    {
        base.CountableItemShow(Description);
        //Reserves
        strBuilder.Clear();
        strBuilder.Append(reserves);
        strBuilder.Append(inventory.GetAmount().ToString());

        itemReserves.text = strBuilder.ToString();
    }

    //InventorySlot�� ItemMenuSlot�� ��ӹ����� �������� �����ϱ� ������ �߰��� �ʱ�ȭ �ʿ�
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

    //Inventory�� �߰��ǰų� ���� �� ������ ����
    public void UpdateAmount(ITEMCODE itemCode, int updateAmount)
    {
        if(sortedInvetorySlotList[itemCode])
        {
            sortedInvetorySlotList[itemCode].UpdateAmount(updateAmount);
            SlotTransformSiblingSort();
        }
    }

    //�ش� ������ �ڵ�(code)�� ���� ������ ���� ���� -> ����Ʈ���� ����
    public void Remove(ITEMCODE code)
    {
        Destroy(sortedInvetorySlotList[code].gameObject);
        sortedInvetorySlotList.Remove(code);
    }

    //focusedItem�� ���� �������� �������� �� Detail Clear
    public void ClearDetail()
    {
        detailImg.gameObject.SetActive(false);
        detailBtn.gameObject.SetActive(false);
        itemName.text = blank;
        itemDescription.text = blank;
        itemReserves.text = blank;
    }

    //������ ���۵� �� �κ��丮 ����Ʈ�� Clear�ϴ� ����
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

    //��� ���� �޽��� ���
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

    //���� ��� �޽��� ���
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
