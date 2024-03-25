using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.UI;

public enum MESSAGETYPE
{
    ALERT,
    LARGE
}

public class GUIMgr : MonoBehaviour
{
    [Header("Title")]
    [SerializeField] TitleUI titleUI;
    [Header("Play")]
    [SerializeField] PlayUI playUI;
    [Header("Item Menu(Shop, Inventory) UI Manager")]
    [SerializeField] ItemMenuUIMgr itemMenuUIMgr;         //Shop, Inventory UI를 가지고 Toggle 값에 따라 표시하는 UI
    [Header("Pause Menu")]
    [SerializeField] PauseMenuUI pauseMenu;
    [Header("Ending")]
    [SerializeField] EndingUI endingUI;
    [Header("Option")]
    [SerializeField] OptionUI optionUI;
    [Header("Loading")]
    [SerializeField] LoadingUI loadingUI;

    [Header("Message")]
    [SerializeField] AlertMessage alertMessage;             //다른 UI에서 Observer로 사용할 중앙 하단부에 위치한 message
    [SerializeField] ToastMessage middleLargeMessage;       //중간에 큰 Message(Stage Clear..등등 사용가능)

    public void Initialize(Player player)
    {
        titleUI.Initialize();
        playUI.Initialize(player);
        itemMenuUIMgr.Initialize();
        endingUI.Initialize();
        optionUI.Initialize();
        loadingUI.Initialize();
        pauseMenu.Initialize();
        alertMessage.Initialize(playUI, itemMenuUIMgr.InventoryUI, itemMenuUIMgr.ShopUI);
    }

    public void TitleActive(bool isOn)
    {
        titleUI.gameObject.SetActive(isOn);
    }

    public void PlayUIActive(bool isOn)
    {
        playUI.gameObject.SetActive(isOn);
    }

    //아이템 메뉴 On Off
    public void ItemMenuActive(bool isOn)
    {
        itemMenuUIMgr.ItemMenuActive(isOn);
    }
    //Pause Menu OnOff
    public void PauseMenuActive(bool isOn)
    {
        pauseMenu.PauseMenuActive(isOn);
    }

    public void OptionMenuActive(bool isOn = true)
    {
        if(isOn)
        {
            PauseMenuActive(false);
            TitleActive(false);
        }

        optionUI.SetActive(isOn);
    }

    public void ShowMessage(MESSAGETYPE type, string message, Color? color = null, float duration = 3.0f)
    {
        switch(type)
        {
            case MESSAGETYPE.ALERT:
                alertMessage.ShowMessage(message, color, duration);
                break;
            case MESSAGETYPE.LARGE:
                middleLargeMessage.ShowMessage(message, color, duration);
                break;
        }
    }

    //Toast Message Clear
    public void ClearMessage()
    {
        alertMessage.ClearMesseage();
        middleLargeMessage.ClearMesseage();
    }

    public void StartEndingUI(bool isClear, int sec)
    {
        endingUI.StartEndingUI(isClear, sec);
    }

    public void UpdateEnding(int sec)
    {
        endingUI.UpdateSecond(sec);
    }

    public void EndingUIActive(bool isOn)
    {
        endingUI.EndingUIActive(isOn);
    }

    public void LoadingUIActive(bool isOn)
    {
        loadingUI.SetActive(isOn);
    }

    public float UpdateLoadingProgress(float progress, float timer)
    {
        return loadingUI.UpdateLoadingProgress(progress, timer);
    }

}
