using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] Button continueButton;
    [SerializeField] Button optionButton;
    [SerializeField] Button quitButton;
    [SerializeField] Button closeButton;

    public void Initialize()
    {
        continueButton.onClick.AddListener(GameMgr.Instance.ClosePauseMenu);
        optionButton.onClick.AddListener(() => { GameMgr.Instance.OptionActive(true); });
        quitButton.onClick.AddListener(GameMgr.Instance.ClosePauseMenu);
        quitButton.onClick.AddListener(GameMgr.Instance.TitleSet);
        closeButton.onClick.AddListener(GameMgr.Instance.ClosePauseMenu);

        PauseMenuActive(false);
    }

    public void PauseMenuActive(bool isOn)
    {
        gameObject.SetActive(isOn);
    }
}
