using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//Item Menu UI�� �����ϴ� Manager
public class ItemMenuUIMgr : MonoBehaviour
{
    [SerializeField] ShopUI shopUI;
    [SerializeField] InventoryUI inventoryUI;

    public ShopUI ShopUI => shopUI;
    public InventoryUI InventoryUI => inventoryUI;

    Dictionary<string, ItemMenuUI> ItemMenuUIs;

    public void Initialize()
    {
        ItemMenuUIs = new();

        ItemMenuUIs.Add(shopUI.name, shopUI);
        ItemMenuUIs.Add(inventoryUI.name, inventoryUI);

        shopUI.Initialize();
        inventoryUI.Initialize();

        ItemMenuActive(false);
    }


    //SHOP or INVENTORY Tab �޴� OnValueChanged �Լ�
    public void OnMenuTabSelected(Toggle selectedTab)
    {
        ItemMenuUIs[selectedTab.name].gameObject.SetActive(selectedTab.isOn);
        if (selectedTab.isOn) ItemMenuUIs[selectedTab.name].OnMenuStartUpdate();
    }

    public void ItemMenuActive(bool isOn)
    {
        gameObject.SetActive(isOn);
        if(isOn)
        {
            foreach(ItemMenuUI im in ItemMenuUIs.Values)
            {
                im.OnMenuStartUpdate();
            }
        }
    }

}
