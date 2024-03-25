using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;


using UnityEngine.UI;
public class SoundMgr : MonoBehaviour
{
    [SerializeField] public AudioMixer audioMixer;

    public void SetVolumeControl(string name, float value)
    {
        audioMixer.SetFloat(name, value);
    }
}
