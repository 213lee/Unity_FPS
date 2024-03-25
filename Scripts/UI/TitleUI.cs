using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleUI : MonoBehaviour
{
    [SerializeField] Button start;
    [SerializeField] Button option;
    [SerializeField] Button quit;

    public void Initialize()
    {
        start.onClick.AddListener(GameMgr.Instance.StartStageSet);
        option.onClick.AddListener(() => { GameMgr.Instance.OptionActive(true); });
        option.onClick.AddListener(() => { gameObject.SetActive(false); });
        quit.onClick.AddListener(GameMgr.Instance.Quit);
    }
}
