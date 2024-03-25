using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * LivingEntity(Player, Enemy -> Humanoid)�� Animation�� �����ϴ� Class
 */
public class LivingEntityAnimation : MonoBehaviour
{
    Animator anim;

    Transform Spine;                                //������ ���� ��ġ�� ���� Spine�� ȸ��
    [SerializeField] Transform SpineLookPos;        //Spine�� �ٶ� ���� ������ġ

    Gun gun;

    bool isDie = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        if (anim) Spine = anim.GetBoneTransform(HumanBodyBones.Spine);
    }


    private void LateUpdate()
    {
        if (!isDie)
            Spine.LookAt(SpineLookPos);     //������ ���� �����ӿ� ���� �ش� ��ġ�� �ٶ󺸰��ϱ� ����
    }

    //IK Animation
    private void OnAnimatorIK(int layerIndex)
    {
        if (gun && anim)
        {
            // �ش� ������Ʈ(Gun)�� pivot�� �ش� �ִϸ��̼�(upper body)�� ������ �Ȳ�ġ ��ġ�� �̵�.
            gun.Pivot = anim.GetIKHintPosition(AvatarIKHint.RightElbow);

            // �޼հ� ������ position, rotation�� �ش� ������Ʈ(Gun)�� ���� ������ ��ġ�� �����.
            anim.SetIKPosition(AvatarIKGoal.LeftHand, gun.LeftHandMountPos);
            anim.SetIKRotation(AvatarIKGoal.LeftHand, gun.LeftHandMountRo);
            anim.SetIKPosition(AvatarIKGoal.RightHand, gun.RightHandMountPos);
            anim.SetIKRotation(AvatarIKGoal.RightHand, gun.RightHandMountRo);
            
            // ����ġ(weight)�� �߰��Ͽ� ��ġ, ȸ���� �̼����� �Ѵ�.
            anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, gun.LeftHandPosWeight);
            anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, gun.LeftHandRoWeight);
            anim.SetIKPositionWeight(AvatarIKGoal.RightHand, gun.RightHandPosWeight);
            anim.SetIKRotationWeight(AvatarIKGoal.RightHand, gun.RightHandRoWeight);
        }
    }


    public void Initialize()
    {
        anim = GetComponent<Animator>();
        Spine = anim.GetBoneTransform(HumanBodyBones.Spine);
        SetStart();
    }

    public void SetStart()
    {
        if (anim)
        {
            anim.SetFloat("Move", 0.0f);
            anim.Play("Movement", 0);
        }
        isDie = false;
    }

    public void Stop()
    {
        Move(0.0f);
    }


    public void SetGun(Gun gun)
    {
        this.gun = gun;
    }

    public void Reload()
    {
        if (gun && anim)
        {
            anim.SetTrigger("Reload");
        }
    }

    public void Move(float value)
    {
        if (anim) anim.SetFloat("Move", value);
    }

    public void Jump(bool isJump)
    {
        if (anim) anim.SetBool("Jump", isJump);
    }

    public void Die()
    {
        if (anim) anim.SetTrigger("Die");

        isDie = true;
    }

}
