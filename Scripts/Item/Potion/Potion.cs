using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Potion : CountableItem
{
    [SerializeField] PotionData potionData;

    public PotionData PotionData => potionData;

    public override void Initialize()
    {
        countableItemData = potionData;
        base.Initialize();
    }

    //Inventory에서 Use호출시,
    //LivingEntity에서 Usepotion의 메서드를 가지는 delegate를 매개변수로 받아
    //potion(duration, percentage)에 맞는 효과로 매개변수 설정하여 delegate 호출
    public bool Use(OneUsePotionEvent usePotionEvent, int _amount = 1)
    {
        if(usePotionEvent?.Invoke(potionData.PotionType, potionData.Duration, potionData.Percentage) == true)
            return base.Use(_amount);
        return false;
    }
}
