using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


//ShopObserver, InventoryObserver가 공통적으로 가지는 메서드 interface
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

//Shop과 Inventory가 공통적으로 가지는 멤버를 가지는 부모 class
public abstract class ItemMenuUI : MonoBehaviour, IItemMenuObserver
{

    [Header("Detail")]
    [SerializeField] protected Image detailImg;                   //Detail에 들어가는 큰 이미지

    [SerializeField] protected TextMeshProUGUI itemName;          //item 이름 text
    //item 설명 text
    //ITEMTYPE == WEAPON (attack power, hit range, ammotype)
    //ITEMTYPE == POTION (효과)
    //ITEMTYPE == AMMO   (호환되는 총 목록)
    [SerializeField] protected TextMeshProUGUI itemDescription;   //item 설명

    [SerializeField] protected TextMeshProUGUI itemReserves;      //item이 개수가 있는 아이템일 때 item 인벤토리에 남아있는 양 출력

    [SerializeField] protected Button detailBtn;                         //아이템 디테일 밑에 위치하는 USE, EQUIP 버튼
    [SerializeField] protected TextMeshProUGUI detailBtnText;            //아이템 버튼에 들어갈 Text

    protected Color successColor = new Color();
    protected Color failureColor = new Color();

    protected System.Text.StringBuilder strBuilder;

    public IAlertMessageObserver messageObserver;              //ItemMenu에서 발생하는 message 출력

    protected readonly string blank       = " ";
    protected readonly string reserves    = "Reserve : ";
    protected readonly string enter       = "\n";
    protected string[] btnTypeText        = new string[(int)DETAILBTNTYPE.COUNT];         //Btn 타입별 string
    protected string[] ammoTypeText       = new string[(int)AMMOTYPE.COUNT];              //ammoType별로 가지는 string
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
    
    //아이템 선택됬을 때 공통적으로 수행하는 이미지와 이름 출력
    public virtual void Show(ItemData itemData)
    {
        if (!detailImg.gameObject.activeSelf) detailImg.gameObject.SetActive(true);
        detailImg.sprite = itemData.DetailImg;
        itemName.text = itemData.ItemName;
    }

    //Countable Item은 Description, Reserves를 같은 방식으로 출력
    protected virtual void CountableItemShow(string Description)
    {
        //Description
        itemDescription.text = Description;
    }

    //Menu가 시작될 때 Detail의 Update를 위해 사용
    public abstract void OnMenuStartUpdate();
}

public interface IShopObserver : IItemMenuObserver
{
    public void BuyMessage(BUYCASE buyCase, string itemName = "");
}

/*
 * Shop을 가지는 Shop UI
 * Inventory와 다르게 슬롯을 미리 만들어두고 사용
*/
public class ShopUI : ItemMenuUI, IShopObserver
{
    [SerializeField] Shop shop;

    [Header("Shop Slot Tr")]
    [SerializeField] Transform shopContent;                  //list에 활성화된 slot을 담는 영역

    [Header("Item Slot Root Tr")]
    [SerializeField] Transform weaponSlotRootTr;      //Weapon Slot 최상위 root
    [SerializeField] Transform potionSlotRootTr;     //Potion Slot 최상위 root
    [SerializeField] Transform ammoSlotRootTr;        //Ammo Slot 최상위 root

    Dictionary<string ,Transform> SlotRootDic;       //Shop Slot 종류 별 최상위 Root Transform을 가지는 Dictionary
    Dictionary<string, List<Transform>> SlotDic;     //Slot들을 종류 별로 Transform을 가지는 Dictionary

    [SerializeField] string[] shopMessageText = new string[(int)BUYCASE.COUNT];     //구매시 경우에 따른 Text를 담는 string arr
    
    public override void Initialize()
    {
        base.Initialize();
        shop.SetObserver(this);

        SlotDic = new();
        SlotRootDic = new();
        SlotRootDic.Add(weaponSlotRootTr.name, weaponSlotRootTr);
        SlotRootDic.Add(ammoSlotRootTr.name, ammoSlotRootTr);
        SlotRootDic.Add(potionSlotRootTr.name, potionSlotRootTr);
        
        //각 아이템 버튼을 가지는 Root Transform으로 반복하여 버튼 초기화 및 리스트에 저장
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

        //Weapon Tab이 활성화 된 상태로 초기화
        foreach (Transform item in SlotDic[weaponSlotRootTr.name])
        {
            item.SetParent(shopContent);
        }

        shopMessageText[(int)BUYCASE.FAILURE_MONEY] = "게임머니가 부족합니다";
        shopMessageText[(int)BUYCASE.FAILURE_EXIST] = "더 이상 가질 수 없습니다";
        shopMessageText[(int)BUYCASE.SUCCESS]       = "구매 성공";

        detailBtnText.text = btnTypeText[(int)DETAILBTNTYPE.BUY];
    }

    //Shop에서 item Tab toggle에 변화가 생겼을때(OnValueChanged)
    //isOn : contents로 부모 설정
    //!isOn : 해당 item root로 부모 설정
    public void OnItemTypeTabSelected(Toggle selectedTab)
    {
        Transform parent = selectedTab.isOn ? shopContent : SlotRootDic[selectedTab.name];
        
        foreach (Transform item in SlotDic[selectedTab.name])
        {
            item.SetParent(parent);
        }
    }

    //Menu UI가 시작될 때 UI정보를 갱신
    public override void OnMenuStartUpdate()
    {
        shop.ShowDetail();
    }

    //아이템 선택됬을 때 Detail 출력
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

    //Countable Item은 Description, Reserves를 같은 방식으로 출력
    protected override void CountableItemShow(string Description)
    {
        base.CountableItemShow(Description);
        strBuilder.Clear();
        strBuilder.Append(reserves);
        strBuilder.Append(shop.GetItemAmount().ToString());
        itemReserves.text = strBuilder.ToString();
    }

    //Shop에서 발생하는 Message의 Text를 미리 가지고 Type별로 출력
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
