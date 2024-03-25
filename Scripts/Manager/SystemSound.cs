using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemSound : MonoBehaviour
{
    [Header("Audio Source")]
    [SerializeField] AudioSource playAudio;                 //Play로 재생하고 isPlay상태를 통해 Stop하기 위한 AudioSource
    [SerializeField] AudioSource playOneShotAudio;          //PlayOneShot으로 재생시킬 Effect Sound

    [Header("Sound Data")]
    [SerializeField] SystemSoundData data;


    public void Initialize()
    {
        if (data)
        {
            playAudio.clip = data.BGM.audioClip;
            playAudio.volume = data.BGM.volume;
        }
    }

    public void PlayBGM()
    {
        if (!playAudio.isPlaying)
        {
            playAudio.clip = data.BGM.audioClip;
            playAudio.volume = data.BGM.volume;
            playAudio.Play();
        }
    }

    public void StopBGM()
    {
        playAudio.Stop();
    }

    public void StopEffect()
    {
        playOneShotAudio.Stop();
    }

    public void Click()
    {
        playOneShotAudio.PlayOneShot(data.Click.audioClip, data.Click.volume);
    }

    public void StageStart()
    {
        playOneShotAudio.PlayOneShot(data.StartStage.audioClip, data.StartStage.volume);
    }

    public void GameFinish(bool isClear)
    {
        if(isClear) playOneShotAudio.PlayOneShot(data.Clear.audioClip, data.Clear.volume);
        else        playOneShotAudio.PlayOneShot(data.Over.audioClip, data.Over.volume);
    }
}
