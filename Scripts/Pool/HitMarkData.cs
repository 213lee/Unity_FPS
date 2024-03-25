using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HitMark Data", menuName = "ScriptableObject/Pooling Object/HitMark", order = 1)]
public class HitMarkData : ScriptableObject
{
    [SerializeField] float fadeTime = 2f;           //FadeOut 되는 시간
    [SerializeField] float maxAlpha = 0.5f;         //Alpha 최댓값
    [SerializeField] float delay = 0.2f;            //Delay Time

    public float FadeTime => fadeTime;
    public float MaxAlpha => maxAlpha;
    public float Delay => delay;

}