#define DEBUG_LE_LOG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * �������� �޴� ��ü�� ������ interface
 */
public interface IDamageable
{
    bool OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal, Vector3 shotPos);
}

/*
 * Player�� Enemy�� �������� ������ �θ� class
 */
public abstract class LivingEntity : MonoBehaviour, IDamageable
{
    [Header("Effect")]
    [SerializeField] ParticleSystem damagedEffect;             //�ǰ� ����Ʈ particlesystem
 
    //Player Enemy ���������� ������ Component
    [Header("Function")]
    [SerializeField] protected LivingEntityMovement movement;
    [SerializeField] protected LivingEntityAnimation anim;
    [SerializeField] protected LivingEntitySound sound;
    [SerializeField] protected Gun gun;

    [Header("Transform")]
    [SerializeField] protected Transform gunPivot;
    [SerializeField] public    Transform camPos;
    [SerializeField] public    Transform aimPos;

    protected bool invincibleState = false;                //���� ����

    public float maxHealth { get; protected set; } = 100;

    public float health { get; protected set; } = 0;        //ü��
    
    public bool isDead => (0.0f >= health);                 //���� ���� Ȯ��

    public event System.Action OnDamagedEvent = null;       //�ǰ� �̺�Ʈ
    
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

    //������ ����Ҷ� �޼���
    public abstract bool UsePotion(POTIONTYPE type, float duration = 0, float percentage = 0);

    //��� �����Ҷ� �޼���
    public abstract void EquipWeapon(Gun equipGun);

    public virtual bool OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal, Vector3 shotPoint)
    {
        if (!invincibleState) health = Mathf.Max(health - damage, 0);
        OnDamagedEvent?.Invoke();

        if (isDead)
        {
            //�������¿��� layer = 2 Ignore RayCast�� �����Ѵ�.
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
