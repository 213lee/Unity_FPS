using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy Data", menuName = "ScriptableObject/LivingEntity/EnemyData", order = 0)]
public class EnemyData : ScriptableObject
{
    [SerializeField] GunData startGunData;      //Enemy가 가지는 GunData
    [SerializeField] AmmoData ammoData;         //GunData에서 가지는 AmmoType의 AmmoData
    [SerializeField] LayerMask targetMask;      //Enemy가 찾는 TargetMask(Player)
    [SerializeField] float moveTime;            //앞으로 이동하는 시간
    [SerializeField] float stopDelay;           //멈춰 대기하는 시간
    [SerializeField] float reloadDelay;         //재장전 대기 시간
    [SerializeField] float dieDelay;            //죽고 사라지기까지 대기하는 시간
    [SerializeField] float beforeAtkDelay;      //공격 전 선 딜레이
    [SerializeField] float afterAtkDelay;       //공격 끝 후 딜레이
    [SerializeField] float viewRadius;          //플레이어를 찾는 시야 거리
    [SerializeField] float viewAngle;           //플레이어를 찾는 시야 각
    [SerializeField] float updateTime;          //FSM 업데이트 주기
    [SerializeField] int   reward;              //죽이면 주어지는 보상(게임 머니)
    [SerializeField] int   repeater;            //Attack 한번에 연사 횟수

    public GunData StartGunData => startGunData;
    public AmmoData AmmoData => ammoData;
    public LayerMask TargetMask => targetMask;
    public float MoveTime => moveTime;
    public float StopDelay => stopDelay;
    public float ReloadDelay => reloadDelay;
    public float DieDelay => dieDelay;
    public float BeforeAtkDelay => beforeAtkDelay;
    public float AfterAtkDelay => afterAtkDelay;
    public float ViewRadius => viewRadius;
    public float ViewAngle => viewAngle;
    public float UpdateTime => updateTime;
    public int Reward => reward;
    public int Repeater => repeater;
}
