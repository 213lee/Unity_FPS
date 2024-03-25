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

    //Inventory���� Useȣ���,
    //LivingEntity���� Usepotion�� �޼��带 ������ delegate�� �Ű������� �޾�
    //potion(duration, percentage)�� �´� ȿ���� �Ű����� �����Ͽ� delegate ȣ��
    public bool Use(OneUsePotionEvent usePotionEvent, int _amount = 1)
    {
        if(usePotionEvent?.Invoke(potionData.PotionType, potionData.Duration, potionData.Percentage) == true)
            return base.Use(_amount);
        return false;
    }
}
