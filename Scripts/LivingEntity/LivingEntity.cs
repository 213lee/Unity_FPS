#define DEBUG_LE_LOG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 데미지를 받는 객체가 가지는 interface
 */
public interface IDamageable
{
    bool OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal, Vector3 shotPos);
}

/*
 * Player와 Enemy가 공통으로 가지는 부모 class
 */
public abstract class LivingEntity : MonoBehaviour, IDamageable
{
    [Header("Effect")]
    [SerializeField] ParticleSystem damagedEffect;             //피격 이펙트 particlesystem
 
    //Player Enemy 공통적으로 가지는 Component
    [Header("Function")]
    [SerializeField] protected LivingEntityMovement movement;
    [SerializeField] protected LivingEntityAnimation anim;
    [SerializeField] protected LivingEntitySound sound;
    [SerializeField] protected Gun gun;

    [Header("Transform")]
    [SerializeField] protected Transform gunPivot;
    [SerializeField] public    Transform camPos;
    [SerializeField] public    Transform aimPos;

    protected bool invincibleState = false;                //무적 상태

    public float maxHealth { get; protected set; } = 100;

    public float health { get; protected set; } = 0;        //체력
    
    public bool isDead => (0.0f >= health);                 //죽음 상태 확인

    public event System.Action OnDamagedEvent = null;       //피격 이벤트
    
    public virtual void Initialize()
    {
        movement.Initialize();
        anim.Initialize();
        sound.Initialize();
        OnDamagedEvent += sound.Damaged;
    }

    public virtual void StartSet()
    {
        health = maxHealth;
        movement.SetStart();
        anim.SetStart();
    }

    public void SetActive(bool isOn)
    {
        gameObject.SetActive(isOn);
    }

    //포션을 사용할때 메서드
    public abstract bool UsePotion(POTIONTYPE type, float duration = 0, float percentage = 0);

    //장비를 장착할때 메서드
    public abstract void EquipWeapon(Gun equipGun);

    public virtual bool OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal, Vector3 shotPoint)
    {
        if (!invincibleState) health = Mathf.Max(health - damage, 0);
        OnDamagedEvent?.Invoke();

        if (isDead)
        {
            //죽은상태에서 layer = 2 Ignore RayCast로 지정한다.
            gameObject.layer = 2;
            return false;
        }
        
#if DEBUG_LE_LOG
        Debug.Log(string.Format("Remain Health : {0}", health));
#endif

        if (damagedEffect)
        {
            Transform effectTr = damagedEffect.transform;
            effectTr.position = hitPoint;
            effectTr.rotation = Quaternion.LookRotation(hitNormal);
            damagedEffect.Play();
        }
        return true;
    }
}
