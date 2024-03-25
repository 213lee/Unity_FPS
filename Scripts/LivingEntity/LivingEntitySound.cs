#define SOUNDTEST

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingEntitySound : MonoBehaviour
{
    [Header("Audio Source")]
    [SerializeField] AudioSource moveAudio;                 //Play�� ����ϰ� isPlay���¸� ���� Stop�ϱ� ���� AudioSource
    [SerializeField] AudioSource effectAudio;               //PlayOneShot���� �����ų Effect Sound

    [Header("Sound Data")]
    [SerializeField] LivingEntitySoundData data;



    public void Update()
    {
        //timeScale�� Pause�� �����ϴµ�
        //timeScale�� 0�� �Ǿ������� Play()�� ����ϴ� playAudio�� ��� ����̵Ǵ� ������
        //playAudio�� pitch���� timeScale�� ���� ���.
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
