using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossHair : MonoBehaviour
{
    [SerializeField] Animator anim;

    public void Fire(bool flag)
    {
        if (anim)
        {
            anim.SetBool("Fire", flag);
        }
    }
}
