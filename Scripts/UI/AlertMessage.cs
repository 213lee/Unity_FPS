using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAlertMessageObserver
{
    public void ShowMessage(string message, Color? color = null, float duration = 3.0f);
}

public class AlertMessage : ToastMessage, IAlertMessageObserver
{
    public void Initialize(PlayUI playUI, InventoryUI inventoryUI, ShopUI shopUI)
    {
        base.Initialize();
        playUI.messageObserver = this;
        inventoryUI.messageObserver = this;
        shopUI.messageObserver = this;
    }
}