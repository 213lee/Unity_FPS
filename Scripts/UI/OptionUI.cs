using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionUI : MonoBehaviour
{
    [SerializeField] Slider masterSlider;
    [SerializeField] Slider bgmSlider;
    [SerializeField] Slider sfxSlider;

    public void Initialize()
    {
        masterSlider.onValueChanged.AddListener((value) => { GameMgr.Instance.SoundMgr.SetVolumeControl(masterSlider.name, value); });
        bgmSlider.onValueChanged.AddListener((value) => { GameMgr.Instance.SoundMgr.SetVolumeControl(bgmSlider.name, value); });
        sfxSlider.onValueChanged.AddListener((value) => { GameMgr.Instance.SoundMgr.SetVolumeControl(sfxSlider.name, value); });

        SetActive(false);
    }

    public void SetActive(bool isOn)
    {
        gameObject.SetActive(isOn);
    }
}
