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

    [SerializeField] ItemData startItemData;   //���� ���۽� �ش� ItemData�� �κ��丮�� �������� �߰��� ���

    System.Action onDieEvent = null;

    Vector2 moveVec;    //�յ��¿� ������ �Է�(Ű����) �޾ƿ��� 2���� ����
    Vector2 roVec;      //���콺 �����Ϳ� ���� ȸ�� �޾ƿ��� 2���� ����

    [SerializeField] PlayerGroundCollider groundCollider;

    private bool isMove;    //�Է����϶� Ȱ��ȭ ���� Update���� moveVec������ Moveȣ��

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

    //Observer ���
    public void SetObserver(IPlayObserver observer)
    {
        playObserver = observer;
        inventory.playObserver = observer;
        inventory.UpdateMoney();
        if(movement is PlayerMovement move) move.playObserver = observer;
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
        gun.SetGun(aimPos, camPos, inventory, sound.Hit);
    }

    //Potion�� ����Ͽ� Ÿ�Կ� ���� ���� ȿ���� ����
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


    //Buff Item�� ��� ��ƾ
    //Active(true) -> Timer -> Active(false)
    private IEnumerator BuffRoutine(POTIONTYPE type, float duration, float percentage = 0)
    {
        BuffActive(type, true, percentage);
        yield return BuffTimer(type, duration);
        BuffActive(type, false);
        playObserver.SetPotionTimer(type, false);

    }

    //POTIONTYPE�� Active ����
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

    //Buff�� duration�� �������� Timer ������ �ϴ� �ڷ�ƾ
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
            //HitMarkshotPos���� ���� dirToEnemy�� ����� v�� forward�� ������ŭ hitMark�� Rotation(z)
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

    //�÷��̾ �ٴڿ� ������ ����
    public void Land()
    {
        movement.Stop();
        anim.Jump(false);
    }

    //------------------ Input Method ------------------//

    //W,A,S,D �̵� ����
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

    //���콺 �����ͷ� �þ� ����
    public void Rotate(InputAction.CallbackContext context)
    {
        //���콺 �����ͷ� �¿� ȸ��
        roVec = context.ReadValue<Vector2>();
        movement.Rotate(roVec);
    }

    //Space ����
    public void Jump(InputAction.CallbackContext context)
    {
        if(groundCollider.IsGround)
        {
            movement.Jump(moveVec);
            anim.Jump(true);
            sound.Jump();
        }
    }

    //Mouse Left ���
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

    //R ������
    public void Reload(InputAction.CallbackContext context)
    {
        if (context.started && gun.Reload()) anim.Reload();
    }
}
