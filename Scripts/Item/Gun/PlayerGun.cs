using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerGun : Gun
{
    public IPlayObserver playObserver;

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Shot()
    {
        base.Shot();
        playObserver.UpdateMagAmmoText(magAmmo);
    }

    protected override IEnumerator ReloadRoutine()
    {
        yield return base.ReloadRoutine();
        playObserver.UpdateMagAmmoText(magAmmo);
    }

    public override void SetGun(Transform aim, Transform camfollow, Inventory inventory, System.Action action = null)
    {
        base.SetGun(aim, camfollow, inventory, action);
        playObserver.UpdateMagAmmoText(magAmmo);

    }
}
