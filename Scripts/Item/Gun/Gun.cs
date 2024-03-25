//#define DEBUG_GUN_LOG
#define DEBUG_GUN_DRAW

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void OnReloadEvent(AMMOTYPE ammoType, int _amount);

public delegate int AmmoRemain(AMMOTYPE ammoType);

public delegate void AmmoUse(AMMOTYPE ammoType, int useAmount);

public class Gun : Item
{
    private Transform pivot;

    [Header("Effect")]
    [SerializeField] protected ParticleSystem muzzleFlashEffect;        //shot effect
    [SerializeField] protected ParticleSystem shellEjectEffect;         //shot effect

    [Header("Audio")]
    [SerializeField] AudioSource audioSource;

    [Header("Gun")] // Inspector�� ǥ��, �ش� Data���� ���ó�� �˷��ش�.
    [SerializeField] GunData gunData;
    [SerializeField] private Transform leftHandMount; // �޼� ��ġ ����.
    [SerializeField] private Transform rightHandMount; // ������ ��ġ ����.
    [SerializeField] private Transform firePos;        // �ѱ� ��ġ
    [SerializeField] private Transform shellEject = null;  // ź�� ȿ�� ��ġ
    [SerializeField] Transform AimPos;      //���� ��ġ
    [SerializeField] Transform CamFollow;   //ī�޶� ����ٴϴ� ��ġ(�Ӹ�)
    
    [Header("IK")]
    [SerializeField] [Range(0, 1)] private float leftHandPosWeight = 1f;
    [SerializeField] [Range(0, 1)] private float leftHandRoWeight = 1f;
    [SerializeField] [Range(0, 1)] private float rightHandPosWeight = 1f;
    [SerializeField] [Range(0, 1)] private float rightHandRoWeight = 1f;

    public event System.Action OnHitTargetEvent = null;

    public GunData GunData => gunData;

    enum State { Ready, Empty, Reloading }
    State state = State.Ready;

    public int magAmmo = 0;              //���� źâ�� �����ִ� �Ѿ�(ź��)�� ��
    
    LineRenderer bulletLineRender = null;   //�Ѿ� ������ �׸��� ���� ����.

    bool firstShot = true;                  //��ݽ� ��ź���� ����
    Coroutine fireCoroutine = null;         //��� Routine


    public AmmoRemain GetAmmo;    //invetory�� GetAmount�� ������ delegate
    public AmmoUse UseAmmo;       //invetory�� UseAmmo�� ������ delegate

    public bool fireCheck { get { return fireCoroutine != null; } }     //���� ���� �߻������� üũ�ϴ� ������Ƽ
    public bool magAmmoCheck { get { return state.Equals(State.Empty); } }  //���� źâ�� �Ѿ��� ������� Ȯ���ϴ� ������Ƽ

    public Vector3 Pivot { get { return (pivot) ? pivot.position : Vector3.zero; } set { if (pivot) pivot.position = value; } }
    public Vector3 LeftHandMountPos { get { return (leftHandMount) ? leftHandMount.position : Vector3.zero; } }
    public Quaternion LeftHandMountRo { get { return (leftHandMount) ? leftHandMount.rotation : Quaternion.identity; } }
    public Vector3 RightHandMountPos { get { return (rightHandMount) ? rightHandMount.position : Vector3.zero; } }
    public Quaternion RightHandMountRo { get { return (rightHandMount) ? rightHandMount.rotation : Quaternion.identity; } }
    public float LeftHandPosWeight { get { return leftHandPosWeight; } }
    public float LeftHandRoWeight { get { return leftHandRoWeight; } }
    public float RightHandPosWeight { get { return rightHandPosWeight; } }
    public float RightHandRoWeight { get { return rightHandRoWeight; } }

    public override void Initialize()
    {
        data = gunData;
        pivot = transform.parent;

        if (TryGetComponent(out bulletLineRender))
        {
            bulletLineRender.positionCount = 2;
            bulletLineRender.enabled = false;
        }
        magAmmo = gunData.MagazineCapacity;    //źâ�� �Ѿ� ���� ä���.
        state = State.Ready;

        muzzleFlashEffect = Instantiate(gunData.MuzzleFlash, firePos);
        shellEjectEffect = Instantiate(gunData.ShellEject, shellEject ? shellEject : transform);

        audioSource.spatialBlend = 1;   //3d ���� ������� ���� ����ϱ� ���� spatialBlend = 1
    }

    //��� �� �ѹ߸��� ȣ��Ǵ� method
    public virtual void Shot()
    {
        Vector3 start = CamFollow.position;
        Vector3 originLocal = AimPos.localPosition;
        Vector3 dst = new Vector3();
        //��ź�� �ƴ϶�� ������ ź���� �ȿ��� �����ϰ� ���ϱ�
        if (!firstShot)
        {
            dst.x += Random.Range(-gunData.AutoRandomMax, gunData.AutoRandomMax);
            dst.y += Random.Range(-gunData.AutoRandomMax, gunData.AutoRandomMax);
            AimPos.localPosition += dst;
        }

        dst = AimPos.position;

        Vector3 dir = dst - start;
        Vector3 hitPos = start + dir * gunData.HitRange;
        if (Physics.Raycast(start, dir, out RaycastHit hit, gunData.HitRange))
        {
            hitPos = hit.point;
#if DEBUG_GUN_LOG
                    Debug.Log(string.Format("y : {0}", hitPos.y));
#endif
            if (hit.collider.TryGetComponent(out IDamageable target))
            {
                target.OnDamage(gunData.AtkPower, hitPos, hit.normal, transform.position);
                OnHitTargetEvent?.Invoke();
            }
            else if (!hit.collider.CompareTag("Boundary"))
            {
                BulletHole bh = GameMgr.Instance.PoolMgr.Pop("Hole", GameMgr.Instance.BulletTr) as BulletHole;
                bh.SetPosition(hitPos);
            }
        }
        ShotEffect();
#if DEBUG_GUN_DRAW
        Debug.DrawRay(start, dir, Color.yellow, gunData.HitRange);
#endif
        magAmmo--;
        if (magAmmo <= 0) state = State.Empty;
        //�����ݵ�
        firstShot = false;

        AimPos.localPosition = originLocal;
        AimPos.Translate(Vector3.up * gunData.VerticalRecoil);
    }

    IEnumerator Fire(int numShot = -1)
    {
        //Ready�� �ƴϰų� numShot�� 0�϶� ����
        while(state.Equals(State.Ready))
        {
            if (numShot == 0) break;
            Shot();
            numShot--;
#if DEBUG_GUN_LOG
            Debug.Log(string.Format("Curr Num {0}", numShot));
#endif
            yield return new WaitForSeconds(gunData.TimeBetFire);
        }
        fireCoroutine = null;
    }

    //�߻� ����Ʈ
    private void ShotEffect()
    {
        if (gunData.SoundData) audioSource.PlayOneShot(gunData.SoundData.Shot.audioClip, gunData.SoundData.Shot.volume);    //shot audio play
        if (muzzleFlashEffect) muzzleFlashEffect.Play();    //�ѱ� ���� ����Ʈ(Particle System)
        if (shellEjectEffect) shellEjectEffect.Play();      //ź�� �������� ����Ʈ(Particle System)
    }
       

    /*
     * ����� ���۵� �� ȣ���
     * Enemy�� ��� �߻�Ƚ�� numShot�� �����ϱ� ���� Default Parameter�� ���
     */
    public void OnFire(int numShot = -1)
    {
        StopFire();
        fireCoroutine = StartCoroutine(Fire(numShot));
    }

    //��� �ڷ�ƾ ����
    public void StopFire()
    {
        if (fireCoroutine != null)
        {
            firstShot = true;
            StopCoroutine(fireCoroutine);
            fireCoroutine = null;
        }
    }

    /*
     * ������ ��ƾ���� 
     * ���� �������� ������ Data�� ������� ������ ����
     */
    protected virtual IEnumerator ReloadRoutine()
    {
        state = State.Reloading;
        if (audioSource) audioSource.PlayOneShot(gunData.SoundData.Reload.audioClip, gunData.SoundData.Reload.volume);
        yield return new WaitForSeconds(gunData.ReloadTime);
              
        int ammoTofill = Mathf.Min(gunData.MagazineCapacity - magAmmo, GetAmmo(gunData.AmmoType));
        magAmmo += ammoTofill;
        state = State.Ready;
        //������ ���� �� �κ��丮�� Ammo�� ����
        UseAmmo(gunData.AmmoType, ammoTofill);
    }

    //�������� ȣ��
    public bool Reload() 
    {
        if (state.Equals(State.Reloading) || 0 >= GetAmmo(gunData.AmmoType) || gunData.MagazineCapacity <= magAmmo) return false;
        StartCoroutine(ReloadRoutine());
        return true; 
    }

    //���� ���� Entity�� �κ��丮�� ���� �ش� Gun�� �������� �� ȣ��Ǵ� �޼���
    public virtual void SetGun(Transform aim, Transform camfollow, Inventory inventory, System.Action action = null)
    {
        pivot = transform.parent;

        if (magAmmo > 0) state = State.Ready;
        else state = State.Empty;

        AimPos = aim;
        CamFollow = camfollow;
        transform.localPosition = gunData.LocalPosition;
        transform.localRotation = Quaternion.Euler(gunData.LocalRotation);

        GetAmmo = inventory.GetAmountByAmmoType;
        UseAmmo = inventory.UseAmmo;
        OnHitTargetEvent = action;
    }

}
