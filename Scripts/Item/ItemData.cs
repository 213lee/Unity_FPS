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
    [SerializeField] string itemName;            //������ �̸�
    [SerializeField] int price;                  //������ ����
    [SerializeField] Sprite icon;                //������ ��ư�� ���� ������
    [SerializeField] Sprite detailImg;           //������ ���� ���� ū �̹���
    [SerializeField] string description;         //������ ����
    [SerializeField] GameObject prefab;          //������ ������
    [SerializeField] ITEMCODE itemCode;          //������ �ڵ�
    [SerializeField] ITEMTYPE itemType;          //������ ����

    public string ItemName => itemName;
    public int Price => price;
    public Sprite Icon => icon;
    public Sprite DetailImg => detailImg;
    public string Description => description;
    public GameObject Prefab => prefab;

    public ITEMCODE ItemCode => itemCode;
    public ITEMTYPE ItemType => itemType;
}
