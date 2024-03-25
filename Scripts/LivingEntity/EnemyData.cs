using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy Data", menuName = "ScriptableObject/LivingEntity/EnemyData", order = 0)]
public class EnemyData : ScriptableObject
{
    [SerializeField] GunData startGunData;      //Enemy�� ������ GunData
    [SerializeField] AmmoData ammoData;         //GunData���� ������ AmmoType�� AmmoData
    [SerializeField] LayerMask targetMask;      //Enemy�� ã�� TargetMask(Player)
    [SerializeField] float moveTime;            //������ �̵��ϴ� �ð�
    [SerializeField] float stopDelay;           //���� ����ϴ� �ð�
    [SerializeField] float reloadDelay;         //������ ��� �ð�
    [SerializeField] float dieDelay;            //�װ� ���������� ����ϴ� �ð�
    [SerializeField] float beforeAtkDelay;      //���� �� �� ������
    [SerializeField] float afterAtkDelay;       //���� �� �� ������
    [SerializeField] float viewRadius;          //�÷��̾ ã�� �þ� �Ÿ�
    [SerializeField] float viewAngle;           //�÷��̾ ã�� �þ� ��
    [SerializeField] float updateTime;          //FSM ������Ʈ �ֱ�
    [SerializeField] int   reward;              //���̸� �־����� ����(���� �Ӵ�)
    [SerializeField] int   repeater;            //Attack �ѹ��� ���� Ƚ��

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
