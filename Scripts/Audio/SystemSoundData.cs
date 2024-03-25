using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "System Sound Data", menuName = "ScriptableObject/Audio Data/System", order = 0)]
public class SystemSoundData : ScriptableObject
{
    [SerializeField] SoundData bgm;         //BGM
    [SerializeField] SoundData click;       //Click시 재생되는 효과
    [SerializeField] SoundData startStage;  //Stage 시작 시 재생
    [SerializeField] SoundData over;        //Game Over
    [SerializeField] SoundData clear;       //Game Clear

    public SoundData BGM => bgm;
    public SoundData Click => click;
    public SoundData StartStage => startStage;
    public SoundData Over => over;
    public SoundData Clear => clear;

    public SystemSoundData()
    {
        bgm = new SoundData(1.0f);
        click = new SoundData(1.0f);
        startStage = new SoundData(1.0f);
        over = new SoundData(1.0f);
        clear = new SoundData(1.0f);
    }
}
