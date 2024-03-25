using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "GunData", menuName = "ScriptableObject/Item Data/Gun", order = 2)]
public class GunData : ItemData
{
    [SerializeField] [Range(1, 100)]        float hitRange = 50.0f;         //�����Ÿ�
    [SerializeField] [Range(0.01f, 1.0f)]   float timeBetFire = 0.12f;      //�Ѿ� �߻� ����.
    [SerializeField] [Range(0.1f, 2.0f)]    float reloadTime = 0.9f;        //������ �ҿ� �ð�.
    [SerializeField]                        float verticalRecoil = 0.1f;    //�����ݵ�
    [SerializeField]                        float autoRandomMax = 0.35f;    //����� ���� �ִ�
    [SerializeField]                        float atkPower = 25;            //���ݷ�.
    [SerializeField] [Range(1, 100)]        int magazineCapacity = 25;      //źâ �뷮.
    [SerializeField]                        Vector3 locPosition;
    [SerializeField]                        Vector3 locRotation;
    [SerializeField]                        AMMOTYPE ammoType;
    [SerializeField]                        ParticleSystem muzzleFlash;
    [SerializeField]                        ParticleSystem shellEject;
    [SerializeField]                        GunSoundData soundData;
        
    public float HitRange => hitRange;
    public float TimeBetFire => timeBetFire;
    public float ReloadTime => reloadTime;
    public float VerticalRecoil => verticalRecoil;
    public float AutoRandomMax => autoRandomMax;
    public int MagazineCapacity => magazineCapacity;
    public float AtkPower => atkPower;
    public Vector3 LocalPosition => locPosition;
    public Vector3 LocalRotation => locRotation;
    public AMMOTYPE AmmoType => ammoType;
    public ParticleSystem MuzzleFlash => muzzleFlash;
    public ParticleSystem ShellEject => shellEject;
    public GunSoundData SoundData => soundData;
}
