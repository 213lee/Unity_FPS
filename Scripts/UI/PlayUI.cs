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
 * Play�߿� ���Ǵ� UI�� ����
 * ���� �Ӵ�, ź, (�÷��̾� ü�¹� �߰�)
 */
public class PlayUI : MonoBehaviour, IPlayObserver
{
    [SerializeField] TextMeshProUGUI magAmmoText;       //�����Ǿ��ִ� ź�� �� text UI
    [SerializeField] TextMeshProUGUI ammoRemainText;    //���� ź�� �� text UI
    [SerializeField] TextMeshProUGUI weaponText;        //�������� ��� text UI
    [SerializeField] TextMeshProUGUI moneyText;         //������ �ִ� �� text UI

    [SerializeField] Slider hpSlider;                   //�÷��̾��� HpSlider
    Coroutine hpCoroutine = null;                       //HpSlider�� �ִϸ��̼��� ���� �ڷ�ƾ

    [SerializeField] CrossHair crossHair;               //Cross Hair UI

    [SerializeField] Transform hitMark;                 //HitMark�� root Tr

    Dictionary<POTIONTYPE, Slider> potionTimer;

    [SerializeField] Slider speedUpTimer;               //Speed UP Drink�� ��� Timer
    [SerializeField] Slider invincibleTimer;         //Adrenaline�� ��� Timer

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

    //������ ź�� UI ������Ʈ
    public void UpdateMagAmmoText(int magAmmo)
    {
        magAmmoText.text = magAmmo.ToString();
    }

    public void UpdateWeaponText(string name)
    {
        weaponText.text = name;
    }

    //���� ź�� UI ������Ʈ
    public void UpdateAmmoRemainText(int ammoRemain)
    {
        ammoRemainText.text = ammoRemain.ToString();
    }

    //�÷��̾� ���� ���� �Ӵ� ������Ʈ
    public void UpdateMoneyText(int money)
    {
        moneyText.text = money.ToString();
    }



    //�÷��̾��� Hp ������Ʈ
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

    //Hp Slider�� �ִϸ��̼����� ������Ʈ
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

    //Cross Hair�� ������Ʈ
    public void UpdateCrossHair(bool flag)
    {
        crossHair.Fire(flag);
    }

    public void HitMarkPop(float angle)
    {
        HitMark hm = GameMgr.Instance.PoolMgr.Pop(hitMark.name, hitMark) as HitMark;
        hm.SetRotation(angle);
    }

    //Hit Mark�� Rotation�� angle�� ����
    public void UpdateHitMarkParentRotation(float angle)
    {
        if(hitMark.childCount > 0) hitMark.transform.Rotate(0,0,angle);
    }

    //�� óġ ���� ��� (+ȹ�� ���� �Ӵ�)
    public void KillEnemyMessage(int money)
    {
        messageObserver.ShowMessage(string.Format("���� óġ �߽��ϴ�. +{0}", money), Color.yellow);
    }

    //���� ���� Timer Set
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
