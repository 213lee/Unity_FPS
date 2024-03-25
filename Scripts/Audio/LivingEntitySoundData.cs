using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]       //Inspector에 노출하기 위해 사용
public struct SoundData
{
    public AudioClip audioClip;
    [Range(0, 1)] public float volume;

    public SoundData(float _volume)
    {
        audioClip = null;
        volume = _volume;
    }
}

[CreateAssetMenu(fileName = "LivingEntity Sound Data", menuName = "ScriptableObject/Audio Data/LivingEntity", order = 1)]
public class LivingEntitySoundData : ScriptableObject
{
    [SerializeField] SoundData move;        //움직이고 있을 때
    [SerializeField] SoundData jump;        //점프할 때
    [SerializeField] SoundData hit;         //적을 맞췄을 때
    [SerializeField] SoundData damaged;     //맞았을 때
    [SerializeField] SoundData die;         //죽었을 때


    public SoundData Move => move;
    public SoundData Jump => jump;
    public SoundData Hit => hit;
    public SoundData Damaged => damaged;
    public SoundData Die => die;

    public LivingEntitySoundData()
    {
        move = new SoundData(1.0f);
        jump = new SoundData(1.0f);
        hit = new SoundData(1.0f);
        damaged = new SoundData(1.0f);
        die = new SoundData(1.0f);
    }
}
