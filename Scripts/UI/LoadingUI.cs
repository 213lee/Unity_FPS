using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour
{
    [SerializeField] Image progressBar;

    public void Initialize()
    {
        progressBar.fillAmount = 0;
        gameObject.SetActive(false);
    }

    public void SetActive(bool isOn)
    {
        if (isOn) progressBar.fillAmount = 0;
        gameObject.SetActive(isOn);
    }

    public float UpdateLoadingProgress(float progress, float timer)
    {
        progressBar.fillAmount = Mathf.Lerp(progressBar.fillAmount, progress, timer);
        return progressBar.fillAmount;
    }
}
