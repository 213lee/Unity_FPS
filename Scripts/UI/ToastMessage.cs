using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/*
 * Toast Message
 * ShowMessage로 
 * fade in / out 되며 
 * duration만큼 표시하고 사라지는 Message UI
 * 
 */
public class ToastMessage : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI messageText;

    [SerializeField] Color defaultColor;

    Coroutine showCoroutine = null;

    public void Initialize()
    {
        ClearMesseage();
    }
    /*
     * ShowMessage(string, duration)
     * message를 duration만큼 출력하고 사라짐.
     * Deafualt Color = white
     */
    //Color Default Parameter 사용안됨.
    //Color.white .. 컴파일 상수 타입이 아니기때문에 Nullable을 사용
    public void ShowMessage(string message, Color? color = null, float duration = 3.0f)
    {
        ClearMesseage();
        messageText.text = message;
        messageText.color = color.HasValue ? color.Value : defaultColor;
        showCoroutine = StartCoroutine(ShowMessageRoutine(duration));
    }

    //Color.alpha를 조절해 등장하고 사라지는 효과를 주는 coroutine method
    private IEnumerator FadeInOut(bool inout, float fadeTime)
    {
        float start, end;
        float fadeDelayTime = fadeTime * 0.1f;

        //Fade in  alpha value 0 -> 1
        //Fade out alpha value 1 -> 0
        if(inout)
        {
            start = 0.0f;
            end = 1.0f;
        }
        else
        {
            start = 1.0f;
            end = 0.0f;
        }

        float elipsed = 0.0f;
        Color origin = messageText.color;

        while(elipsed < fadeTime)
        {
            float alpha = Mathf.Lerp(start, end, elipsed / fadeTime);
            messageText.color = new Color(origin.r, origin.g, origin.b, alpha);
            
            //Time.timeScale에 영향을 받지 않고 실행하기 위해 설정한 Delay
            elipsed += fadeDelayTime;
            yield return new WaitForSecondsRealtime(fadeDelayTime);
        }
    }

    private IEnumerator ShowMessageRoutine(float duration)
    {
        float fadeTime = duration * 0.2f;
        float durationTime = duration * 0.6f;
        messageText.enabled = true;
        yield return FadeInOut(true, fadeTime);

        //Pause(Time.timeScale == 0) 상태에서도 동작하게 하기 위해서
        //WaitForSecondsReltime을 사용.
        yield return new WaitForSecondsRealtime(durationTime);

        yield return FadeInOut(false, fadeTime);
        messageText.enabled = false;
    }

    public void ClearMesseage()
    {
        if(showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
            showCoroutine = null;
        }
        messageText.text = "";
        messageText.enabled = false;
    }

}
