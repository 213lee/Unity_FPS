//#define DEBUG_PLAYUI_LOG

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
 * Ending UI
 * Game Clear or Over 시에 활성화 되는 UI
 * Description(text : n초 후 Title로 돌아갑니다, slider : 남은 시간(n))
 * Retry, Quit 버튼 두개
 */
public class EndingUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI endingText;
    [SerializeField] TextMeshProUGUI descriptionText;
    [SerializeField] Slider descriptionSlider;

    [SerializeField] Button retry;
    [SerializeField] Button quit;

    [SerializeField] Color gameOverColor;
    [SerializeField] Color gameClearColor;
    private readonly string gameOverString      = "Game Over";
    private readonly string gameClearString     = "Game Clear";
    private readonly string descriptionString   = "초뒤 타이틀로 돌아갑니다.";

    System.Text.StringBuilder strBuilder;

    Coroutine secondCoroutine;
    public void Initialize()
    {
        strBuilder = new();       

        retry.onClick.AddListener(GameMgr.Instance.StartStageSet);
        retry.onClick.AddListener(FinishEndingUI);
        quit.onClick.AddListener(GameMgr.Instance.TitleSet);
        quit.onClick.AddListener(FinishEndingUI);

        EndingUIActive(false);
    }
    public void StartEndingUI(bool isClear, int sec)
    {
        endingText.color = isClear ? gameClearColor : gameOverColor;
        endingText.text = isClear ? gameClearString : gameOverString;
        descriptionText.text = sec.ToString() + descriptionString;
        descriptionSlider.maxValue = sec;
        descriptionSlider.value = sec;
        gameObject.SetActive(true);

        if (secondCoroutine != null) StopCoroutine(secondCoroutine);
        secondCoroutine = StartCoroutine(SecondBarAnimatioin());
    }

    public void UpdateSecond(int sec)
    {
        strBuilder.Clear();
        strBuilder.Append(sec.ToString());
        strBuilder.Append(descriptionString);
        descriptionText.text = strBuilder.ToString();
                
    }

    private IEnumerator SecondBarAnimatioin()
    {
        float t = 0.0f;
        float calldelay = 0.02f;
        float elipsed = calldelay / descriptionSlider.maxValue;
        while(t < 1)
        {
            t += elipsed;
            descriptionSlider.value = Mathf.Lerp(descriptionSlider.maxValue, descriptionSlider.minValue, t);
            yield return new WaitForSecondsRealtime(calldelay);
        }

        secondCoroutine = null;
    }

    public void EndingUIActive(bool isOn = false)
    {
        gameObject.SetActive(isOn);
    }

    public void FinishEndingUI()
    {
        gameObject.SetActive(false);
    }
}