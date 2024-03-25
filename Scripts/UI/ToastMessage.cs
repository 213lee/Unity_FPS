using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/*
 * Toast Message
 * ShowMessage�� 
 * fade in / out �Ǹ� 
 * duration��ŭ ǥ���ϰ� ������� Message UI
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
     * message�� duration��ŭ ����ϰ� �����.
     * Deafualt Color = white
     */
    //Color Default Parameter ���ȵ�.
    //Color.white .. ������ ��� Ÿ���� �ƴϱ⶧���� Nullable�� ���
    public void ShowMessage(string message, Color? color = null, float duration = 3.0f)
    {
        ClearMesseage();
        messageText.text = message;
        messageText.color = color.HasValue ? color.Value : defaultColor;
        showCoroutine = StartCoroutine(ShowMessageRoutine(duration));
    }

    //Color.alpha�� ������ �����ϰ� ������� ȿ���� �ִ� coroutine method
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
            
            //Time.timeScale�� ������ ���� �ʰ� �����ϱ� ���� ������ Delay
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

        //Pause(Time.timeScale == 0) ���¿����� �����ϰ� �ϱ� ���ؼ�
        //WaitForSecondsReltime�� ���.
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
