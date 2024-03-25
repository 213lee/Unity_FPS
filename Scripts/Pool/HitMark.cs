using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HitMark : PoolableObject
{
    [SerializeField] Image hitMarkImage;
    [SerializeField] HitMarkData data;

    private void OnEnable()
    {
        if (TryGetComponent<RectTransform>(out RectTransform rectTr))
        {
            rectTr.offsetMin = new Vector2(0, 0);
            rectTr.offsetMax = new Vector2(0, 0);
        }
        StartCoroutine(FadeOut());
    }

    //Object가 Pop될 때 Rotation method
    public void SetRotation(float angle)
    {
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    //Object가 활성화 되고 다시 Push되기까지 FadeOut 효과를 주는 Coroutine
    private IEnumerator FadeOut()
    {
        float alpha, elipsed = 0;
        Color origin = hitMarkImage.color;
        //Fade out
        while(elipsed < data.FadeTime)
        {
            alpha = Mathf.Lerp(data.MaxAlpha, 0, elipsed / data.FadeTime);
            elipsed += data.Delay;
            hitMarkImage.color = new Color(origin.r, origin.g, origin.b, alpha);
            yield return new WaitForSecondsRealtime(data.Delay);
        }

        GameMgr.Instance.PoolMgr.Push(this);
    }


}
