using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * LivingEntity(Player, Enemy -> Humanoid)의 Animation을 관리하는 Class
 */
public class LivingEntityAnimation : MonoBehaviour
{
    Animator anim;

    Transform Spine;                                //에임의 상하 위치에 따라 Spine을 회전
    [SerializeField] Transform SpineLookPos;        //Spine이 바라볼 방향 에임위치

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
            Spine.LookAt(SpineLookPos);     //에임의 상하 움직임에 따라 해당 위치를 바라보게하기 위함
    }

    //IK Animation
    private void OnAnimatorIK(int layerIndex)
    {
        if (gun && anim)
        {
            // 해당 오브젝트(Gun)의 pivot을 해당 애니메이션(upper body)의 오른쪽 팔꿈치 위치로 이동.
            gun.Pivot = anim.GetIKHintPosition(AvatarIKHint.RightElbow);

            // 왼손과 오른속 position, rotation을 해당 오브젝트(Gun)의 왼쪽 손잡이 위치에 맞춘다.
            anim.SetIKPosition(AvatarIKGoal.LeftHand, gun.LeftHandMountPos);
            anim.SetIKRotation(AvatarIKGoal.LeftHand, gun.LeftHandMountRo);
            anim.SetIKPosition(AvatarIKGoal.RightHand, gun.RightHandMountPos);
            anim.SetIKRotation(AvatarIKGoal.RightHand, gun.RightHandMountRo);
            
            // 가중치(weight)를 추가하여 위치, 회전을 미세조정 한다.
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
