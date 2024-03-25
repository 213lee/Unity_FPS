#define SOUNDTEST

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingEntitySound : MonoBehaviour
{
    [Header("Audio Source")]
    [SerializeField] AudioSource moveAudio;                 //Play로 재생하고 isPlay상태를 통해 Stop하기 위한 AudioSource
    [SerializeField] AudioSource effectAudio;               //PlayOneShot으로 재생시킬 Effect Sound

    [Header("Sound Data")]
    [SerializeField] LivingEntitySoundData data;



    public void Update()
    {
        //timeScale로 Pause를 구현하는데
        //timeScale이 0이 되었을때도 Play()를 사용하는 playAudio는 계속 재생이되는 문제로
        //playAudio의 pitch값을 timeScale과 같이 재생.
        moveAudio.pitch = Time.timeScale;
    }

    public void Initialize()
    {
        if (!data) return;

        moveAudio.clip = data.Move.audioClip;
        moveAudio.volume = data.Move.volume;
    }

    public void Move()
    {
        if (moveAudio.isPlaying) return;

        moveAudio.Play();
    }

    public void Stop()
    {
        moveAudio.Stop();
    }

    public void Jump()
    {
        effectAudio.PlayOneShot(data.Jump.audioClip, data.Jump.volume);
    }

    public void Damaged()
    {
        effectAudio.PlayOneShot(data.Damaged.audioClip, data.Damaged.volume);
    }

    public void Hit()
    {
        effectAudio.PlayOneShot(data.Hit.audioClip, data.Hit.volume);
    }

    public void Die()
    {
        effectAudio.PlayOneShot(data.Die.audioClip, data.Die.volume);
    }
}
