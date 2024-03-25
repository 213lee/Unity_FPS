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

    StateData stateData = null;         //Enemy가 가지는 FSM State Data

    EnemyData enemyData = null;

    FSM stateMachine = null;

    //Move, Stop에서 시간으로 사용할 프로퍼티
    public float timer = 0.0f;

    //회전이 끝났는지 확인 하기위해 사용되는 프로퍼티
    public bool isRotating { get; private set; } = true;
    //공격이 끝났는지 확인하기 위해 사용되는 프로퍼티
    public bool isAttackFinished { get; private set; } = true;

    float randomAngle;                  //랜덤으로 결정되는 회전 각도

    //DEBUG용 시야각 그리기
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
        //Enemy가 죽었을 때 Player의 AddMoney를 호출할 수 있도록 함수 등록
        OnDieEvent += () => { player.inventory.EarnByMoney(enemyData.Reward); } ;
        OnDieEvent += anim.Die;
        OnDieEvent += sound.Die;
        OnDieEvent += Stop;
        //Enemy가 죽었을 때 EnemyManager의 list에서 해당 Enemy를 삭제하기 위해 사용하는 Observer
        dieObserver = enemyManager;

        //Enemy는 Init과 동시에 Start Setting
        StartSet();
    }

    public override void StartSet()
    {
        base.StartSet();
        SetStartWeapon();
        SetData();
    }

    //시작 장비 설정
    public void SetStartWeapon()
    {
        inventory.AddItem(enemyData.StartGunData);
        inventory.AddItem(enemyData.AmmoData);
        inventory.AddItem(enemyData.AmmoData, 9999);
        inventory.EquipItem(enemyData.StartGunData);
    }

    //장비 장착
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

    //Enemy Potion Use 미구현
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

    //게임시작 직전 수행되는 메서드
    public bool SetData()
    {
        if (stateMachine == null) stateMachine = new FSM(this);
        if (!stateMachine.SetCurrState(stateData.StopState))
        {
            ErroMessage("Current State가 NULL 입니다.");
            return false;
        }

        gameObject.SetActive(true);
        anim.Initialize();
        StartCoroutine(OnUpdate());
        return true;
    }

    //움직임을 멈추기 위한 Stop method
    private void Stop()
    {
        movement.Stop();
        anim.Stop();
    }

    //State에서 딜레이에 사용될 Timer method
    public bool TimeCheck()
    {
        if(0.0f <= timer)
        {
            timer -= enemyData.UpdateTime;
            return true;
        }
        return false;
    }

    //이동 중 앞에 벽이 있는지 체크하는 method
    public bool wallCheck()
    {
        return obstacleCollider.wallTrigger;
    }

    //현재 Enemy 시야각에 플레이어가 있는지 체크
    public bool FindTarget()
    {
        Collider[] targetInViewRadius = Physics.OverlapSphere(camPos.position, enemyData.ViewRadius, enemyData.TargetMask);

        for (int i = 0; i < targetInViewRadius.Length; i++)
        {
            Vector3 targetPos = targetInViewRadius[i].transform.position + new Vector3(0, camPos.position.y - 0.3f, 0);
            Vector3 dirToTarget = targetPos - camPos.position;

            //시야각에 타겟이 존재하는지 확인
            if (Vector3.Angle(camPos.forward, dirToTarget.normalized) < enemyData.ViewAngle * 0.5f)
            {
                float dstToTarget = Vector3.Distance(camPos.position, targetPos);

                //타겟 방향으로 Raycast를 쏘고 타겟이 아닌 물체가 맞는다면, 시야각에 존재하지만 다른 장애물에 가려서 보이지 않는 상태로 본다.
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

    //Ememy의 FOV를 확인하기 위한 함수
    private void DrawFOV()
    {
        float angleInDegrees = enemyData.ViewAngle * 0.5f;

        float lookingAngle = transform.eulerAngles.y;  //캐릭터가 바라보는 방향의 각도
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
        
        int sign = Random.Range(0, 2) == 1 ? 1 : -1;            // 부호를 결정할 sign

        randomAngle = Random.Range(2, 7) * 15.0f * sign;        // -30 ~ ~105, 30 ~ 105의 범위로 랜덤하게 회전을 설정한다.

        //벽에 부딪친 상태라면 랜덤한 회전값에서 180도 추가
        if (wallCheck())
        {
            randomAngle += 180.0f * -sign;
        }

        rotateCoroutine = StartCoroutine(Rotate());        
    }

    //회전을 자연스럽게 하기위해 사용하는 코루틴.
    Coroutine rotateCoroutine;

    //부드럽게 회전하기 위해 코루틴을 사용해 FSM Update 주기보다 더 빠르게 한번씩 회전
    IEnumerator Rotate()
    {
        //랜덤하게 회전할 각도의 10%를 한번 회전에 회전
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
            //사격
            gun.OnFire(enemyData.Repeater);
#if DEBUG_ENEMY_LOG
            Debug.Log(string.Format("Mag Ammo == {0}", gun.magAmmo));
#endif
            //공격이 끝나면 boolean 변경하는 코루틴 호출
            AttackCoroutine = StartCoroutine(AttackCheck());
        }
    }
       

    //공격이 끝나면 isAttackFinished를 true로 바꾸는 코루틴
    IEnumerator AttackCheck()
    {
        //총이 발사중일 때 true 끝나면 false
        while(gun.fireCheck) {  yield return new WaitForSeconds(0.2f); }
        AttackCoroutine = null;                   //공격중인지 체크하는 코루틴 null
        isAttackFinished = true;                  //공격 종료 프로퍼티 true 설정
        timer = enemyData.AfterAtkDelay;          //후딜레이 타이머 설정
#if DEBUG_ENEMY_LOG
        Debug.Log("Finish Attack");
#endif
    }

    public void SetAttack()
    {
        Stop();
        timer = enemyData.BeforeAtkDelay;          //선딜레이 타이머 설정
        isAttackFinished = false;                  //공격 종료 프로퍼티 false 설정
    }

    //Attack Exit에서 코루틴이 종료되지 않았다면 종료하고
    //에임 위치 초기화.
    public void AttackFinished()
    {   
        if (AttackCoroutine != null)    //Attack Coroutine 실행중이라면 Stop
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
     * Enemy가 죽으면
     * Player의 Money 증가.
     * n초 후에 Destroy()
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
            ErroMessage("StopState Null 입니다");
        }
#if DEBUG_ENEMY_LOG
        Debug.Log("Stop Enter");
#endif
    }

    public void OnMoveState()
    {
        if (!stateMachine.ChangeState(stateData.MoveState))
        {
            ErroMessage("MoveState Null 입니다");
        }
#if DEBUG_ENEMY_LOG
        Debug.Log("Move Enter");
#endif
    }

    public void OnRotateState()
    {
        if (!stateMachine.ChangeState(stateData.RotateState))
        {
            ErroMessage("RotateState Null 입니다");
        }
#if DEBUG_ENEMY_LOG
        Debug.Log("Rotate Enter");
#endif
    }

    public void OnAttackState()
    {
        if (!stateMachine.ChangeState(stateData.AttackState))
        {
            ErroMessage("AttackState Null 입니다");
        }
#if DEBUG_ENEMY_LOG
        Debug.Log("Attack Enter");
#endif
    }

    public void OnReloadState()
    {
        if(!stateMachine.ChangeState(stateData.ReloadState))
        {
            ErroMessage("ReloadState Null 입니다");
        }
#if DEBUG_ENEMY_LOG
        Debug.Log("Reload Enter");
#endif
    }

    public void OnDieState()
    {
        if (!stateMachine.ChangeState(stateData.DieState))
        {
            ErroMessage("ReloadState Null 입니다");
        }
#if DEBUG_ENEMY_LOG
        Debug.Log("Load Enter");
#endif
    }


}
