#define DEBUG_ENEMY_DRAW
//#define DEBUG_ENEMY_LOG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Enemy : LivingEntity
{
    [Header("Inventory")]
    [SerializeField] public Inventory inventory;
    [SerializeField] EnemyObstacleCollider obstacleCollider;
 
    public event System.Action OnDieEvent = null;

    IEnemyDieObserver dieObserver;

    StateData stateData = null;         //Enemy�� ������ FSM State Data

    EnemyData enemyData = null;

    FSM stateMachine = null;

    //Move, Stop���� �ð����� ����� ������Ƽ
    public float timer = 0.0f;

    //ȸ���� �������� Ȯ�� �ϱ����� ���Ǵ� ������Ƽ
    public bool isRotating { get; private set; } = true;
    //������ �������� Ȯ���ϱ� ���� ���Ǵ� ������Ƽ
    public bool isAttackFinished { get; private set; } = true;

    float randomAngle;                  //�������� �����Ǵ� ȸ�� ����

    //DEBUG�� �þ߰� �׸���
    private void Update()
    {
#if DEBUG_ENEMY_DRAW
        DrawFOV();
#endif
    }

    public void Initialize(EnemyData _enemyData, StateData _stateData, EnemyMgr enemyManager, Player player)
    {
        base.Initialize();
        inventory.Initialize(this);
        stateData = _stateData;
        enemyData = _enemyData;
        //Enemy�� �׾��� �� Player�� AddMoney�� ȣ���� �� �ֵ��� �Լ� ���
        OnDieEvent += () => { player.inventory.EarnByMoney(enemyData.Reward); } ;
        OnDieEvent += anim.Die;
        OnDieEvent += sound.Die;
        OnDieEvent += Stop;
        //Enemy�� �׾��� �� EnemyManager�� list���� �ش� Enemy�� �����ϱ� ���� ����ϴ� Observer
        dieObserver = enemyManager;

        //Enemy�� Init�� ���ÿ� Start Setting
        StartSet();
    }

    public override void StartSet()
    {
        base.StartSet();
        SetStartWeapon();
        SetData();
    }

    //���� ��� ����
    public void SetStartWeapon()
    {
        inventory.AddItem(enemyData.StartGunData);
        inventory.AddItem(enemyData.AmmoData);
        inventory.AddItem(enemyData.AmmoData, 9999);
        inventory.EquipItem(enemyData.StartGunData);
    }

    //��� ����
    public override void EquipWeapon(Gun equipGun)
    {
        if (gun)
        {
            gun.transform.SetParent(inventory.transform);
            gun.gameObject.SetActive(false);
        }
        equipGun.transform.SetParent(gunPivot);
        equipGun.gameObject.SetActive(true);
        anim.SetGun(equipGun);
        gun = equipGun;
        gun.SetGun(aimPos, camPos, inventory);
    }

    //Enemy Potion Use �̱���
    public override bool UsePotion(POTIONTYPE type, float duration = 0, float percentage = 0)
    {
        return true;
    }


    public override bool OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal, Vector3 shotPos)
    {
        if (!base.OnDamage(damage, hitPoint, hitNormal, shotPos))
        {
            anim.Die();
            OnDieState();
            
            return false;
        }
        return true;
    }

    void ErroMessage(string msg)
    {
#if DEBUG_ENEMY_LOG
        Debug.LogError(msg);
#endif
        gameObject.SetActive(false);
    }

    IEnumerator OnUpdate()
    {
        while (true)
        {
            stateMachine.Update();
            yield return new WaitForSeconds(enemyData.UpdateTime);
        }
    }

    //���ӽ��� ���� ����Ǵ� �޼���
    public bool SetData()
    {
        if (stateMachine == null) stateMachine = new FSM(this);
        if (!stateMachine.SetCurrState(stateData.StopState))
        {
            ErroMessage("Current State�� NULL �Դϴ�.");
            return false;
        }

        gameObject.SetActive(true);
        anim.Initialize();
        StartCoroutine(OnUpdate());
        return true;
    }

    //�������� ���߱� ���� Stop method
    private void Stop()
    {
        movement.Stop();
        anim.Stop();
    }

    //State���� �����̿� ���� Timer method
    public bool TimeCheck()
    {
        if(0.0f <= timer)
        {
            timer -= enemyData.UpdateTime;
            return true;
        }
        return false;
    }

    //�̵� �� �տ� ���� �ִ��� üũ�ϴ� method
    public bool wallCheck()
    {
        return obstacleCollider.wallTrigger;
    }

    //���� Enemy �þ߰��� �÷��̾ �ִ��� üũ
    public bool FindTarget()
    {
        Collider[] targetInViewRadius = Physics.OverlapSphere(camPos.position, enemyData.ViewRadius, enemyData.TargetMask);

        for (int i = 0; i < targetInViewRadius.Length; i++)
        {
            Vector3 targetPos = targetInViewRadius[i].transform.position + new Vector3(0, camPos.position.y - 0.3f, 0);
            Vector3 dirToTarget = targetPos - camPos.position;

            //�þ߰��� Ÿ���� �����ϴ��� Ȯ��
            if (Vector3.Angle(camPos.forward, dirToTarget.normalized) < enemyData.ViewAngle * 0.5f)
            {
                float dstToTarget = Vector3.Distance(camPos.position, targetPos);

                //Ÿ�� �������� Raycast�� ��� Ÿ���� �ƴ� ��ü�� �´´ٸ�, �þ߰��� ���������� �ٸ� ��ֹ��� ������ ������ �ʴ� ���·� ����.
                if (Physics.Raycast(camPos.position, dirToTarget, out RaycastHit hit, dstToTarget)
                    && hit.collider.CompareTag("Player"))
                {
                    movement.LookTarget(dirToTarget, targetPos.y > camPos.position.y);
                    return true;
                }
            }
        }
        return false;
    }

    Vector3 AngleToDir(float angle)
    {
        float radian = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(radian), 0f, Mathf.Cos(radian));
    }

    //Ememy�� FOV�� Ȯ���ϱ� ���� �Լ�
    private void DrawFOV()
    {
        float angleInDegrees = enemyData.ViewAngle * 0.5f;

        float lookingAngle = transform.eulerAngles.y;  //ĳ���Ͱ� �ٶ󺸴� ������ ����
        Vector3 rightDir = AngleToDir(lookingAngle + angleInDegrees);
        Vector3 leftDir = AngleToDir(lookingAngle - angleInDegrees);
        Vector3 lookDir = AngleToDir(lookingAngle);

        Debug.DrawRay(transform.position, rightDir * enemyData.ViewRadius, Color.blue);
        Debug.DrawRay(transform.position, leftDir * enemyData.ViewRadius, Color.blue);
        Debug.DrawRay(transform.position, lookDir * enemyData.ViewRadius, Color.cyan);
    }


    //--------------- Stop State ---------------//
    public void SetPatrolStop()
    {
        Stop();
        movement.InitAim();
        sound.Stop();
        timer = enemyData.StopDelay;
    }


    //--------------- Rotate State ---------------//
    public void SetPatrolRotate()
    {
        Stop();        
        isRotating = true;
        
        int sign = Random.Range(0, 2) == 1 ? 1 : -1;            // ��ȣ�� ������ sign

        randomAngle = Random.Range(2, 7) * 15.0f * sign;        // -30 ~ ~105, 30 ~ 105�� ������ �����ϰ� ȸ���� �����Ѵ�.

        //���� �ε�ģ ���¶�� ������ ȸ�������� 180�� �߰�
        if (wallCheck())
        {
            randomAngle += 180.0f * -sign;
        }

        rotateCoroutine = StartCoroutine(Rotate());        
    }

    //ȸ���� �ڿ������� �ϱ����� ����ϴ� �ڷ�ƾ.
    Coroutine rotateCoroutine;

    //�ε巴�� ȸ���ϱ� ���� �ڷ�ƾ�� ����� FSM Update �ֱ⺸�� �� ������ �ѹ��� ȸ��
    IEnumerator Rotate()
    {
        //�����ϰ� ȸ���� ������ 10%�� �ѹ� ȸ���� ȸ��
        float angle = randomAngle * 0.1f;
        while (true)
        {
            if (Mathf.Abs(randomAngle) <= Mathf.Abs(angle))
                break;
            randomAngle -= angle;
            movement.RotateXByAngle(angle);
            yield return new WaitForSeconds(0.02f);
        }
        isRotating = false;
        rotateCoroutine = null;
    }

    public void StopRotate()
    {
        if (rotateCoroutine != null)
        {
            StopCoroutine(rotateCoroutine);
            rotateCoroutine = null;
        }
    }

    //--------------- Move State ---------------//
    public void SetPatrolMove()
    {
        Stop();
        timer = enemyData.MoveTime;
        Move();
    }

    public void Move()
    {
        sound.Move();
        movement.Move(new Vector2(0.0f, 0.5f));
        anim.Move(0.5f);
        sound.Move();
    }

    public void SetPatroMoveFinish()
    {
        sound.Stop();
    }


    //--------------- Attack State ---------------//
    Coroutine AttackCoroutine = null;

    public void AttackToPlayer()
    {
        if(AttackCoroutine == null)
        {
            //���
            gun.OnFire(enemyData.Repeater);
#if DEBUG_ENEMY_LOG
            Debug.Log(string.Format("Mag Ammo == {0}", gun.magAmmo));
#endif
            //������ ������ boolean �����ϴ� �ڷ�ƾ ȣ��
            AttackCoroutine = StartCoroutine(AttackCheck());
        }
    }
       

    //������ ������ isAttackFinished�� true�� �ٲٴ� �ڷ�ƾ
    IEnumerator AttackCheck()
    {
        //���� �߻����� �� true ������ false
        while(gun.fireCheck) {  yield return new WaitForSeconds(0.2f); }
        AttackCoroutine = null;                   //���������� üũ�ϴ� �ڷ�ƾ null
        isAttackFinished = true;                  //���� ���� ������Ƽ true ����
        timer = enemyData.AfterAtkDelay;          //�ĵ����� Ÿ�̸� ����
#if DEBUG_ENEMY_LOG
        Debug.Log("Finish Attack");
#endif
    }

    public void SetAttack()
    {
        Stop();
        timer = enemyData.BeforeAtkDelay;          //�������� Ÿ�̸� ����
        isAttackFinished = false;                  //���� ���� ������Ƽ false ����
    }

    //Attack Exit���� �ڷ�ƾ�� ������� �ʾҴٸ� �����ϰ�
    //���� ��ġ �ʱ�ȭ.
    public void AttackFinished()
    {   
        if (AttackCoroutine != null)    //Attack Coroutine �������̶�� Stop
        {
            AttackCoroutine = null;
            StopCoroutine(AttackCheck()); 
        }
    }

    public bool AmmoCheck()
    {
        return gun.magAmmoCheck;
    }

    //--------------- Reload State ---------------//
    public void SetReload()
    {
        if (gun.Reload()) anim.Reload();
        timer = enemyData.ReloadDelay;
    }

    /*
     * Enemy�� ������
     * Player�� Money ����.
     * n�� �Ŀ� Destroy()
     */
    //--------------- Die State ---------------//
    public void SetDie()
    {
        timer = enemyData.DieDelay;
        OnDieEvent?.Invoke();
    }

    public void Die()
    {
        dieObserver.DestroyEnemy(this);
        Destroy(gameObject);
    }

    
    //--------------- Change State ---------------//
    public void OnStopState()
    {
        if (!stateMachine.ChangeState(stateData.StopState))
        {
            ErroMessage("StopState Null �Դϴ�");
        }
#if DEBUG_ENEMY_LOG
        Debug.Log("Stop Enter");
#endif
    }

    public void OnMoveState()
    {
        if (!stateMachine.ChangeState(stateData.MoveState))
        {
            ErroMessage("MoveState Null �Դϴ�");
        }
#if DEBUG_ENEMY_LOG
        Debug.Log("Move Enter");
#endif
    }

    public void OnRotateState()
    {
        if (!stateMachine.ChangeState(stateData.RotateState))
        {
            ErroMessage("RotateState Null �Դϴ�");
        }
#if DEBUG_ENEMY_LOG
        Debug.Log("Rotate Enter");
#endif
    }

    public void OnAttackState()
    {
        if (!stateMachine.ChangeState(stateData.AttackState))
        {
            ErroMessage("AttackState Null �Դϴ�");
        }
#if DEBUG_ENEMY_LOG
        Debug.Log("Attack Enter");
#endif
    }

    public void OnReloadState()
    {
        if(!stateMachine.ChangeState(stateData.ReloadState))
        {
            ErroMessage("ReloadState Null �Դϴ�");
        }
#if DEBUG_ENEMY_LOG
        Debug.Log("Reload Enter");
#endif
    }

    public void OnDieState()
    {
        if (!stateMachine.ChangeState(stateData.DieState))
        {
            ErroMessage("ReloadState Null �Դϴ�");
        }
#if DEBUG_ENEMY_LOG
        Debug.Log("Load Enter");
#endif
    }


}
