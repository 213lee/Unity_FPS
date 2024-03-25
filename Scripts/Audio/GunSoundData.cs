using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Gun Sound Data", menuName = "ScriptableObject/Audio Data/Gun", order = 2)]
public class GunSoundData : ScriptableObject
{
    [SerializeField] SoundData shot;
    [SerializeField] SoundData reload;

    public SoundData Shot => shot;
    public SoundData Reload => reload;

    public GunSoundData()
    {
        shot = new SoundData(1.0f);
        reload = new SoundData(1.0f);
    }
}

