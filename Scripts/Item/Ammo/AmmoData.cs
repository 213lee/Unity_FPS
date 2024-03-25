using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CountableItemData : ItemData
{
    [SerializeField] int oneTimeSupply;         //1회 제공량

    public int OneTimeSupply => oneTimeSupply;
}

[CreateAssetMenu(fileName = "AmmoData", menuName = "ScriptableObject/Item Data/Ammo", order = 3)]
public class AmmoData : CountableItemData
{
    [SerializeField] AMMOTYPE ammoType;   
    public AMMOTYPE AmmoType => ammoType;
}

