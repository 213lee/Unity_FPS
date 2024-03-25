using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


//ShopObserver, InventoryObserver�� ���������� ������ �޼��� interface
public interface IItemMenuObserver
{
    public void Show(ItemData itemData);
}


public enum GUNDESCRIPTIONTYPE
{
    ATTAK_POWER,
    HIT_RANGE,
    AMMO_TYPE,
    COUNT
}

//Shop�� Inventory�� ���������� ������ ����� ������ �θ� class
public abstract class ItemMenuUI : MonoBehaviour, IItemMenuObserver
{

    [Header("Detail")]
    [SerializeField] protected Image detailImg;                   //Detail�� ���� ū �̹���

    [SerializeField] protected TextMeshProUGUI itemName;          //item �̸� text
    //item ���� text
    //ITEMTYPE == WEAPON (attack power, hit range, ammotype)
    //ITEMTYPE == POTION (ȿ��)
    //ITEMTYPE == AMMO   (ȣȯ�Ǵ� �� ���)
    [SerializeField] protected TextMeshProUGUI itemDescription;   //item ����

    [SerializeField] protected TextMeshProUGUI itemReserves;      //item�� ������ �ִ� �������� �� item �κ��丮�� �����ִ� �� ���

    [SerializeField] protected Button detailBtn;                         //������ ������ �ؿ� ��ġ�ϴ� USE, EQUIP ��ư
    [SerializeField] protected TextMeshProUGUI detailBtnText;            //������ ��ư�� �� Text

    protected Color successColor = new Color();
    protected Color failureColor = new Color();

    protected System.Text.StringBuilder strBuilder;

    public IAlertMessageObserver messageObserver;              //ItemMenu���� �߻��ϴ� message ���

    protected readonly string blank       = " ";
    protected readonly string reserves    = "Reserve : ";
    protected readonly string enter       = "\n";
    protected string[] btnTypeText        = new string[(int)DETAILBTNTYPE.COUNT];         //Btn Ÿ�Ժ� string
    protected string[] ammoTypeText       = new string[(int)AMMOTYPE.COUNT];              //ammoType���� ������ string
    protected string[] gunDescriptionText = new string[(int)GUNDESCRIPTIONTYPE.COUNT];    //Gun Description

    public virtual void Initialize()
    {
        strBuilder = new System.Text.StringBuilder();

        btnTypeText[(int)DETAILBTNTYPE.BUY]   = "BUY";
        btnTypeText[(int)DETAILBTNTYPE.EQUIP] = "EQUIP";
        btnTypeText[(int)DETAILBTNTYPE.USE]   = "USE";

        
        ammoTypeText[(int)AMMOTYPE.MM_556] = "5.56MM";
        ammoTypeText[(int)AMMOTYPE.MM_762] = "7.62MM";
        ammoTypeText[(int)AMMOTYPE.ACP_45] = "0.45ACP";

        gunDescriptionText[(int)GUNDESCRIPTIONTYPE.ATTAK_POWER] = "Attack Power : ";
        gunDescriptionText[(int)GUNDESCRIPTIONTYPE.HIT_RANGE]   = "Hit Range : ";
        gunDescriptionText[(int)GUNDESCRIPTIONTYPE.AMMO_TYPE]   = "Ammo Type : ";

        detailBtnText = detailBtn.GetComponentInChildren<TextMeshProUGUI>();

        detailImg.gameObject.SetActive(false);

        successColor = new Color();
        successColor = Color.yellow;
        failureColor = new Color();
        failureColor = Color.red;
    }
    
    //������ ���É��� �� ���������� �����ϴ� �̹����� �̸� ���
    public virtual void Show(ItemData itemData)
    {
        if (!detailImg.gameObject.activeSelf) detailImg.gameObject.SetActive(true);
        detailImg.sprite = itemData.DetailImg;
        itemName.text = itemData.ItemName;
    }

    //Countable Item�� Description, Reserves�� ���� ������� ���
    protected virtual void CountableItemShow(string Description)
    {
        //Description
        itemDescription.text = Description;
    }

    //Menu�� ���۵� �� Detail�� Update�� ���� ���
    public abstract void OnMenuStartUpdate();
}

public interface IShopObserver : IItemMenuObserver
{
    public void BuyMessage(BUYCASE buyCase, string itemName = "");
}

/*
 * Shop�� ������ Shop UI
 * Inventory�� �ٸ��� ������ �̸� �����ΰ� ���
*/
public class ShopUI : ItemMenuUI, IShopObserver
{
    [SerializeField] Shop shop;

    [Header("Shop Slot Tr")]
    [SerializeField] Transform shopContent;                  //list�� Ȱ��ȭ�� slot�� ��� ����

    [Header("Item Slot Root Tr")]
    [SerializeField] Transform weaponSlotRootTr;      //Weapon Slot �ֻ��� root
    [SerializeField] Transform potionSlotRootTr;     //Potion Slot �ֻ��� root
    [SerializeField] Transform ammoSlotRootTr;        //Ammo Slot �ֻ��� root

    Dictionary<string ,Transform> SlotRootDic;       //Shop Slot ���� �� �ֻ��� Root Transform�� ������ Dictionary
    Dictionary<string, List<Transform>> SlotDic;     //Slot���� ���� ���� Transform�� ������ Dictionary

    [SerializeField] string[] shopMessageText = new string[(int)BUYCASE.COUNT];     //���Ž� ��쿡 ���� Text�� ��� string arr
    
    public override void Initialize()
    {
        base.Initialize();
        shop.SetObserver(this);

        SlotDic = new();
        SlotRootDic = new();
        SlotRootDic.Add(weaponSlotRootTr.name, weaponSlotRootTr);
        SlotRootDic.Add(ammoSlotRootTr.name, ammoSlotRootTr);
        SlotRootDic.Add(potionSlotRootTr.name, potionSlotRootTr);
        
        //�� ������ ��ư�� ������ Root Transform���� �ݺ��Ͽ� ��ư �ʱ�ȭ �� ����Ʈ�� ����
        foreach (var root in SlotRootDic)
        {
            List<Transform> container = new List<Transform>();
            foreach (Transform slot in root.Value)
            {
                slot.GetComponent<ShopSlot>().Initialize(shop);
                container.Add(slot);
            }
            SlotDic.Add(root.Key, container);
        }

        //Weapon Tab�� Ȱ��ȭ �� ���·� �ʱ�ȭ
        foreach (Transform item in SlotDic[weaponSlotRootTr.name])
        {
            item.SetParent(shopContent);
        }

        shopMessageText[(int)BUYCASE.FAILURE_MONEY] = "���ӸӴϰ� �����մϴ�";
        shopMessageText[(int)BUYCASE.FAILURE_EXIST] = "�� �̻� ���� �� �����ϴ�";
        shopMessageText[(int)BUYCASE.SUCCESS]       = "���� ����";

        detailBtnText.text = btnTypeText[(int)DETAILBTNTYPE.BUY];
    }

    //Shop���� item Tab toggle�� ��ȭ�� ��������(OnValueChanged)
    //isOn : contents�� �θ� ����
    //!isOn : �ش� item root�� �θ� ����
    public void OnItemTypeTabSelected(Toggle selectedTab)
    {
        Transform parent = selectedTab.isOn ? shopContent : SlotRootDic[selectedTab.name];
        
        foreach (Transform item in SlotDic[selectedTab.name])
        {
            item.SetParent(parent);
        }
    }

    //Menu UI�� ���۵� �� UI������ ����
    public override void OnMenuStartUpdate()
    {
        shop.ShowDetail();
    }

    //������ ���É��� �� Detail ���
    public override void Show(ItemData itemData)
    {
        base.Show(itemData);

        if(itemData.ItemType == ITEMTYPE.WEAPON)
        {
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
        }
        else 
        {
            CountableItemShow(itemData.Description);
        }
    }

    //Countable Item�� Description, Reserves�� ���� ������� ���
    protected override void CountableItemShow(string Description)
    {
        base.CountableItemShow(Description);
        strBuilder.Clear();
        strBuilder.Append(reserves);
        strBuilder.Append(shop.GetItemAmount().ToString());
        itemReserves.text = strBuilder.ToString();
    }

    //Shop���� �߻��ϴ� Message�� Text�� �̸� ������ Type���� ���
    public void BuyMessage(BUYCASE buyCase, string itemName = "")
    {
        Color color = failureColor;
        strBuilder.Clear();
        strBuilder.Append(itemName);
        if (buyCase == BUYCASE.SUCCESS)
        {
            strBuilder.Append(blank);
            color = successColor;
        }
        strBuilder.Append(shopMessageText[(int)buyCase]);
        messageObserver.ShowMessage(strBuilder.ToString(), color);
    }
}
