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

    [Header("Gun")] // Inspector에 표시, 해당 Data들의 사용처를 알려준다.
    [SerializeField] GunData gunData;
    [SerializeField] private Transform leftHandMount; // 왼손 위치 지점.
    [SerializeField] private Transform rightHandMount; // 오른손 위치 지점.
    [SerializeField] private Transform firePos;        // 총구 위치
    [SerializeField] private Transform shellEject = null;  // 탄피 효과 위치
    [SerializeField] Transform AimPos;      //조준 위치
    [SerializeField] Transform CamFollow;   //카메라가 따라다니는 위치(머리)
    
    [Header("IK")]
    [SerializeField] [Range(0, 1)] private float leftHandPosWeight = 1f;
    [SerializeField] [Range(0, 1)] private float leftHandRoWeight = 1f;
    [SerializeField] [Range(0, 1)] private float rightHandPosWeight = 1f;
    [SerializeField] [Range(0, 1)] private float rightHandRoWeight = 1f;

    public event System.Action OnHitTargetEvent = null;

    public GunData GunData => gunData;

    enum State { Ready, Empty, Reloading }
    State state = State.Ready;

    public int magAmmo = 0;              //현재 탄창에 남아있는 총알(탄약)의 수
    
    LineRenderer bulletLineRender = null;   //총알 궤적을 그리기 위한 도구.

    bool firstShot = true;                  //사격시 초탄인지 여부
    Coroutine fireCoroutine = null;         //사격 Routine


    public AmmoRemain GetAmmo;    //invetory의 GetAmount를 가지는 delegate
    public AmmoUse UseAmmo;       //invetory의 UseAmmo를 가지는 delegate

    public bool fireCheck { get { return fireCoroutine != null; } }     //현재 총이 발사중인지 체크하는 프로퍼티
    public bool magAmmoCheck { get { return state.Equals(State.Empty); } }  //현재 탄창에 총알이 비었는지 확인하는 프로퍼티

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
        magAmmo = gunData.MagazineCapacity;    //탄창에 총알 가득 채운다.
        state = State.Ready;

        muzzleFlashEffect = Instantiate(gunData.MuzzleFlash, firePos);
        shellEjectEffect = Instantiate(gunData.ShellEject, shellEject ? shellEject : transform);

        audioSource.spatialBlend = 1;   //3d 공간 기반으로 사운드 재생하기 위해 spatialBlend = 1
    }

    //사격 시 한발마다 호출되는 method
    public virtual void Shot()
    {
        Vector3 start = CamFollow.position;
        Vector3 originLocal = AimPos.localPosition;
        Vector3 dst = new Vector3();
        //초탄이 아니라면 벌어진 탄착군 안에서 랜덤하게 더하기
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
        //수직반동
        firstShot = false;

        AimPos.localPosition = originLocal;
        AimPos.Translate(Vector3.up * gunData.VerticalRecoil);
    }

    IEnumerator Fire(int numShot = -1)
    {
        //Ready가 아니거나 numShot이 0일때 종료
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

    //발사 이펙트
    private void ShotEffect()
    {
        if (gunData.SoundData) audioSource.PlayOneShot(gunData.SoundData.Shot.audioClip, gunData.SoundData.Shot.volume);    //shot audio play
        if (muzzleFlashEffect) muzzleFlashEffect.Play();    //총구 섬광 이펙트(Particle System)
        if (shellEjectEffect) shellEjectEffect.Play();      //탄피 떨어지는 이펙트(Particle System)
    }
       

    /*
     * 사격이 시작될 때 호출됨
     * Enemy의 경우 발사횟수 numShot을 지정하기 위해 Default Parameter를 사용
     */
    public void OnFire(int numShot = -1)
    {
        StopFire();
        fireCoroutine = StartCoroutine(Fire(numShot));
    }

    //사격 코루틴 종료
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
     * 재장전 루틴으로 
     * 총의 종류별로 가지는 Data를 기반으로 로직을 수행
     */
    protected virtual IEnumerator ReloadRoutine()
    {
        state = State.Reloading;
        if (audioSource) audioSource.PlayOneShot(gunData.SoundData.Reload.audioClip, gunData.SoundData.Reload.volume);
        yield return new WaitForSeconds(gunData.ReloadTime);
              
        int ammoTofill = Mathf.Min(gunData.MagazineCapacity - magAmmo, GetAmmo(gunData.AmmoType));
        magAmmo += ammoTofill;
        state = State.Ready;
        //재장전 종료 후 인벤토리의 Ammo를 갱신
        UseAmmo(gunData.AmmoType, ammoTofill);
    }

    //재장전시 호출
    public bool Reload() 
    {
        if (state.Equals(State.Reloading) || 0 >= GetAmmo(gunData.AmmoType) || gunData.MagazineCapacity <= magAmmo) return false;
        StartCoroutine(ReloadRoutine());
        return true; 
    }

    //총을 가진 Entity가 인벤토리를 통해 해당 Gun을 장착했을 때 호출되는 메서드
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
