using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageUIMgr : MonoBehaviour
{
    [SerializeField] AlertMessage alertMessage;             //�ٸ� UI���� Observer�� ����� message
    [SerializeField] ToastMessage middleLargeMessage;       //�߰��� ū Message(Stage Clear..��� ��밡��)

    public void Initialize(PlayUI playUI, InventoryUI inventoryUI, ShopUI shopUI)
    {
        playUI.messageObserver = alertMessage;
        inventoryUI.messageObserver = alertMessage;
        shopUI.messageObserver = alertMessage;
    }

    //Message�� ��� Clear
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
