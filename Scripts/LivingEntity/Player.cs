//#define DEBUG_PLAYER_DRAW
//#define DEBUG_PLAYER_LOG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class Player : LivingEntity
{
    [SerializeField] public Transform endingCamPos;
    [SerializeField] public Transform endingAimPos;

    [SerializeField] ItemData startItemData;   //게임 시작시 해당 ItemData로 인벤토리에 아이템을 추가해 사용

    System.Action onDieEvent = null;

    Vector2 moveVec;    //앞뒤좌우 움직임 입력(키보드) 받아오는 2차원 벡터
    Vector2 roVec;      //마우스 포인터에 따른 회전 받아오는 2차원 벡터

    [SerializeField] PlayerGroundCollider groundCollider;

    private bool isMove;    //입력중일때 활성화 시켜 Update에서 moveVec값으로 Move호출

    [Header("Inventory")]
    [SerializeField] public PlayerInventory inventory;

    public IPlayObserver playObserver;

    void Update()
    {
        if (isMove && groundCollider.IsGround)
        {
            movement.Move(moveVec);
            anim.Move((moveVec == Vector2.zero) ? .0f : 1.0f);
            sound.Move();
        }
        else
        {
            sound.Stop();
        }
    }

    public override void Initialize()
    {
        base.Initialize();
        inventory.Initialize(this);
        groundCollider.Initialize(this);

        OnDamagedEvent += () => { playObserver.UpdateHpAnimation(health); };

        onDieEvent += movement.Stop;
        onDieEvent += anim.Die;
        onDieEvent += sound.Die;
    }

    public override void StartSet()
    {
        base.StartSet();
        playObserver.UpdateHp(health);
        inventory.StartSet(startItemData);
        gameObject.layer = LayerMask.NameToLayer("Player");
    }

    //Observer 등록
    public void SetObserver(IPlayObserver observer)
    {
        playObserver = observer;
        inventory.playObserver = observer;
        inventory.UpdateMoney();
        if(movement is PlayerMovement move) move.playObserver = observer;
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
        gun.SetGun(aimPos, camPos, inventory, sound.Hit);
    }

    //Potion을 사용하여 타입에 따라 포션 효과를 적용
    public override bool UsePotion(POTIONTYPE type, float duration = 0, float percentage = 0)
    {
        switch (type)
        {
            //HEAL PACK
            case POTIONTYPE.HP:
                if (health == maxHealth) return false;
                health = Mathf.Min(maxHealth, maxHealth * percentage + health);
                playObserver.UpdateHpAnimation(health);
                break;
            //SPEED UP Drink
            case POTIONTYPE.BUFF_SPEED:
                if (!movement.SpeedUpState) return false;
                StartCoroutine(BuffRoutine(type, duration, percentage));
                break;
            //INVINCIBLE Adrenaline
            case POTIONTYPE.BUFF_INVINCIBLE:
                if (invincibleState) return false;
                StartCoroutine(BuffRoutine(type, duration, percentage));
                break;
        }
        playObserver.SetPotionTimer(type, true, duration);
        return true;
    }


    //Buff Item의 사용 루틴
    //Active(true) -> Timer -> Active(false)
    private IEnumerator BuffRoutine(POTIONTYPE type, float duration, float percentage = 0)
    {
        BuffActive(type, true, percentage);
        yield return BuffTimer(type, duration);
        BuffActive(type, false);
        playObserver.SetPotionTimer(type, false);

    }

    //POTIONTYPE별 Active 적용
    private void BuffActive(POTIONTYPE type, bool isOn, float percentage = 0)
    {
        switch (type)
        {
            case POTIONTYPE.BUFF_SPEED:
                if (isOn) movement.SpeedUp(isOn, percentage);
                else movement.SpeedUp(isOn);
                break;
            case POTIONTYPE.BUFF_INVINCIBLE:
                invincibleState = isOn;
                break;
            default:
                break;
        }
    }

    //Buff의 duration을 기준으로 Timer 역할을 하는 코루틴
    private IEnumerator BuffTimer(POTIONTYPE type, float duration)
    {
        float start = duration;
        float end = 0;
        float t = 0;
        while (t <= duration)
        {
            playObserver.UpdatePotionTimer(type, Mathf.Lerp(start, end, t / duration));
            t += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
    }

    public override bool OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal, Vector3 shotPos)
    {
        if (base.OnDamage(damage, hitPoint, hitNormal, shotPos))
        {
            //HitMarkshotPos로의 벡터 dirToEnemy를 계산해 v와 forward의 각도만큼 hitMark를 Rotation(z)
            Vector3 dirToEnemy = (shotPos - transform.position).normalized;
            float angle = Vector3.SignedAngle(dirToEnemy, transform.forward, Vector3.up);
            playObserver.HitMarkPop(angle);

            return true;
        }
        else
        {
            onDieEvent?.Invoke();
            GameMgr.Instance.GameFinish();
            return false;
        }
    }

    //플레이어가 바닥에 착지시 수행
    public void Land()
    {
        movement.Stop();
        anim.Jump(false);
    }

    //------------------ Input Method ------------------//

    //W,A,S,D 이동 조작
    public void Move(InputAction.CallbackContext context)
    {
        moveVec = context.ReadValue<Vector2>();

        if (context.performed)
        {
            isMove = true;
        }
        else
        {
            isMove = false;
            if (groundCollider.IsGround) movement.Move(moveVec);
            anim.Move(0.0f);
        }
    }

    //마우스 포인터로 시야 조작
    public void Rotate(InputAction.CallbackContext context)
    {
        //마우스 포인터로 좌우 회전
        roVec = context.ReadValue<Vector2>();
        movement.Rotate(roVec);
    }

    //Space 점프
    public void Jump(InputAction.CallbackContext context)
    {
        if(groundCollider.IsGround)
        {
            movement.Jump(moveVec);
            anim.Jump(true);
            sound.Jump();
        }
    }

    //Mouse Left 사격
    public void Fire(InputAction.CallbackContext context)
    {
        if(gun)
        {
            if (context.started)
            {
                gun.OnFire();
                playObserver.UpdateCrossHair(true);
            }

            if (context.canceled)
            {
                gun.StopFire();
                playObserver.UpdateCrossHair(false);
            }
        }
    }

    //R 재장전
    public void Reload(InputAction.CallbackContext context)
    {
        if (context.started && gun.Reload()) anim.Reload();
    }
}
