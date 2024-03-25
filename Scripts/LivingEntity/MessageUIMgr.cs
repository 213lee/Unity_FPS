using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageUIMgr : MonoBehaviour
{
    [SerializeField] AlertMessage alertMessage;             //다른 UI에서 Observer로 사용할 message
    [SerializeField] ToastMessage middleLargeMessage;       //중간에 큰 Message(Stage Clear..등등 사용가능)

    public void Initialize(PlayUI playUI, InventoryUI inventoryUI, ShopUI shopUI)
    {
        playUI.messageObserver = alertMessage;
        inventoryUI.messageObserver = alertMessage;
        shopUI.messageObserver = alertMessage;
    }

    //Message를 모두 Clear
    public void AllClear()
    {
        alertMessage.ClearMesseage();
        middleLargeMessage.ClearMesseage();
    }

    public void ShowLargeMessage(string message, Color? color = null, float duration = 3.0f)
    {
        middleLargeMessage.ShowMessage(message, color, duration);
    }

}
