using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : LivingEntityMovement
{
    public IPlayObserver playObserver = null;

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        playObserver?.UpdateHitMarkParentRotation(roX);
    }
}
