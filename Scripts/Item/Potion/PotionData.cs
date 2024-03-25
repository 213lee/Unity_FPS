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
    [SerializeField] POTIONTYPE potionType;               //포션이 가지는 타입(Hp, Buff_Speed, Buff_invincibility...)
    [SerializeField] float duration;                        //포션이 가지는 지속시간.
    [SerializeField] [Range(0,1)] float percentage;                      //포션이 가지는 효과의 증가 수치(비율 %)
    public POTIONTYPE PotionType => potionType;
    public float Duration => duration;
    public float Percentage => percentage;
}
