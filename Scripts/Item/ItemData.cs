using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum AMMOTYPE
{
    MM_556,
    MM_762,
    ACP_45,
    COUNT
}

public enum ITEMTYPE
{
    WEAPON,
    POTION,
    AMMO
}

public enum ITEMCODE
{
    WP_M4,
    WP_AK,
    WP_UMP,
    AM_556,
    AM_762,
    AM_45ACP,
    HEALPACK,
    ENERGYDRINK,
    ADRENALINE
}

[CreateAssetMenu(fileName = "ItemData", menuName = "ScriptableObject/Item Data/Item", order = 1)]
public class ItemData : ScriptableObject
{
    [SerializeField] string itemName;            //아이템 이름
    [SerializeField] int price;                  //아이템 가격
    [SerializeField] Sprite icon;                //아이템 버튼에 들어가는 아이콘
    [SerializeField] Sprite detailImg;           //아이템 설명에 들어가는 큰 이미지
    [SerializeField] string description;         //아이템 설명
    [SerializeField] GameObject prefab;          //아이템 프리팹
    [SerializeField] ITEMCODE itemCode;          //아이템 코드
    [SerializeField] ITEMTYPE itemType;          //아이템 종류

    public string ItemName => itemName;
    public int Price => price;
    public Sprite Icon => icon;
    public Sprite DetailImg => detailImg;
    public string Description => description;
    public GameObject Prefab => prefab;

    public ITEMCODE ItemCode => itemCode;
    public ITEMTYPE ItemType => itemType;
}
