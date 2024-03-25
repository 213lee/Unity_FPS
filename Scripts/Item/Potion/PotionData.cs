using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum POTIONTYPE
{
    HP,
    BUFF_SPEED,
    BUFF_INVINCIBLE,
    COUNT
}

[CreateAssetMenu(fileName = "potionData", menuName = "ScriptableObject/Item Data/potion", order = 4)]
public class PotionData : CountableItemData
{
    [SerializeField] POTIONTYPE potionType;               //������ ������ Ÿ��(Hp, Buff_Speed, Buff_invincibility...)
    [SerializeField] float duration;                        //������ ������ ���ӽð�.
    [SerializeField] [Range(0,1)] float percentage;                      //������ ������ ȿ���� ���� ��ġ(���� %)
    public POTIONTYPE PotionType => potionType;
    public float Duration => duration;
    public float Percentage => percentage;
}
