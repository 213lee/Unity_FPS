//#define DEBUG_PLAYUI_LOG

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IPlayObserver
{
    public void UpdateCrossHair(bool flag);
    public void UpdateHp(float hp);
    public void UpdateHpAnimation(float hp);
    public void UpdateMoneyText(int money);
    public void UpdateWeaponText(string name);
    public void UpdateMagAmmoText(int magAmmo);
    public void UpdateAmmoRemainText(int ammoRemain);
    public void UpdateHitMarkParentRotation(float angle);
    public void HitMarkPop(float angle);
    public void KillEnemyMessage(int money);
    public void SetPotionTimer(POTIONTYPE type, bool isOn = true, float duration = 0.0f);
    public void UpdatePotionTimer(POTIONTYPE type, float value);
}

/*
 * Play UI
 * Play중에 사용되는 UI를 관리
 * 게임 머니, 탄, (플레이어 체력바 추가)
 */
public class PlayUI : MonoBehaviour, IPlayObserver
{
    [SerializeField] TextMeshProUGUI magAmmoText;       //장전되어있는 탄의 수 text UI
    [SerializeField] TextMeshProUGUI ammoRemainText;    //남은 탄의 수 text UI
    [SerializeField] TextMeshProUGUI weaponText;        //장착중인 장비 text UI
    [SerializeField] TextMeshProUGUI moneyText;         //가지고 있는 돈 text UI

    [SerializeField] Slider hpSlider;                   //플레이어의 HpSlider
    Coroutine hpCoroutine = null;                       //HpSlider의 애니메이션을 위한 코루틴

    [SerializeField] CrossHair crossHair;               //Cross Hair UI

    [SerializeField] Transform hitMark;                 //HitMark의 root Tr

    Dictionary<POTIONTYPE, Slider> potionTimer;

    [SerializeField] Slider speedUpTimer;               //Speed UP Drink의 사용 Timer
    [SerializeField] Slider invincibleTimer;         //Adrenaline의 사용 Timer

    public IAlertMessageObserver messageObserver;              //Message Observer


    public void OnDisable()
    {
        GameMgr.Instance.PoolMgr.ReturnToPool("HitMark", hitMark);
    }

    public void Initialize(Player player)
    {        
        player.SetObserver(this);

        hpSlider.maxValue = player.maxHealth;
        hpSlider.value = player.health;

        potionTimer = new Dictionary<POTIONTYPE, Slider>();
        potionTimer.Add(POTIONTYPE.BUFF_SPEED, speedUpTimer);
        potionTimer.Add(POTIONTYPE.BUFF_INVINCIBLE, invincibleTimer);
    }

    //장전된 탄약 UI 업데이트
    public void UpdateMagAmmoText(int magAmmo)
    {
        magAmmoText.text = magAmmo.ToString();
    }

    public void UpdateWeaponText(string name)
    {
        weaponText.text = name;
    }

    //남은 탄약 UI 업데이트
    public void UpdateAmmoRemainText(int ammoRemain)
    {
        ammoRemainText.text = ammoRemain.ToString();
    }

    //플레이어 가진 게임 머니 업데이트
    public void UpdateMoneyText(int money)
    {
        moneyText.text = money.ToString();
    }



    //플레이어의 Hp 업데이트
    public void UpdateHp(float hp)
    {
        hpSlider.value = hp;
    }

    public void UpdateHpAnimation(float hp)
    {
#if DEBUG_PLAYUI_LOG
        Debug.Log(string.Format("Player Hp = {0}", hp));
#endif
        if (hpCoroutine != null)
        {
            StopCoroutine(hpCoroutine);
            hpCoroutine = null;
        }
        hpCoroutine = StartCoroutine(HpSliderAnimation(hp));
    }

    //Hp Slider를 애니메이션으로 업데이트
    IEnumerator HpSliderAnimation(float hp)
    {
        float animationHpValue = hpSlider.value;
        float t = 0.0f;
        float elipsed = 2.0f / hpSlider.maxValue;
        while (hpSlider.value != hp)
        {
            t += elipsed;
            hpSlider.value = Mathf.Lerp(animationHpValue, hp, t);
            yield return new WaitForSecondsRealtime(elipsed);
        }

        hpSlider.value = hp;
        hpCoroutine = null;
    }

    //Cross Hair를 업데이트
    public void UpdateCrossHair(bool flag)
    {
        crossHair.Fire(flag);
    }

    public void HitMarkPop(float angle)
    {
        HitMark hm = GameMgr.Instance.PoolMgr.Pop(hitMark.name, hitMark) as HitMark;
        hm.SetRotation(angle);
    }

    //Hit Mark의 Rotation을 angle로 설정
    public void UpdateHitMarkParentRotation(float angle)
    {
        if(hitMark.childCount > 0) hitMark.transform.Rotate(0,0,angle);
    }

    //적 처치 문구 출력 (+획득 게임 머니)
    public void KillEnemyMessage(int money)
    {
        messageObserver.ShowMessage(string.Format("적을 처치 했습니다. +{0}", money), Color.yellow);
    }

    //포션 사용시 Timer Set
    public void SetPotionTimer(POTIONTYPE type, bool isOn = true, float duration = 0.0f)
    {
        if (!potionTimer.ContainsKey(type)) return;
        
        potionTimer[type].gameObject.SetActive(isOn);
        potionTimer[type].maxValue = duration;
        potionTimer[type].value = duration;
    }

    //Timer Value Update
    public void UpdatePotionTimer(POTIONTYPE type, float value)
    {
        potionTimer[type].value = value;
        if(value < 0) potionTimer[type].gameObject.SetActive(false);
    }

}
