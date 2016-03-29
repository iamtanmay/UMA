using UnityEngine;
using System.Collections;

public class WeaponHolsterSMB : CustomSMB
{
    private PlayerAtts plAtts;

    public override void Init(Animator anim)
    {
        plAtts = userInput.GetComponent<PlayerAtts>();
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GunAtt gunAtt = plAtts.cGun.GetComponent<GunAtt>();
        // weapon holster sound
        if (gunAtt.sounds.holster)
            gunAtt.sounds.holster.GetComponent<AudioSource>().Play();

    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (plAtts.weaponToDraw == null)    // not changing weapon, holstering
        {
            if (plAtts.isHandAwayFromGun)
            {
                plAtts.cGun.SetParent(null);
                plAtts.cGun.position = new Vector3(0, -200, 0);
                plAtts.isHandAwayFromGun = false;
                plAtts.cGun = null;
            }
        }
        else
        {
            if (plAtts.isHandAwayFromGun)
            {
                plAtts.cGun.SetParent(null);
                plAtts.cGun.position = new Vector3(0, -200, 0);
                plAtts.isHandAwayFromGun = false;
                plAtts.cGun = null;
            }
            GunAtt nextGunAtt = plAtts.weaponToDraw.GetComponent<GunAtt>();
            animator.SetInteger("Draw", nextGunAtt.gunStyle);
            animator.SetInteger("ChangeGun", 0);
            plAtts.hud.PrintAmmoText(nextGunAtt.currentClipCapacity);
            plAtts.hud.PrintAmmoText(plAtts.currentMagz[nextGunAtt.bulletStyle], false);
            if (nextGunAtt.hudImage)
                plAtts.hud.ChangeWeaponImg(nextGunAtt.hudImage);
            if (nextGunAtt.isMelee)
            {
                plAtts.hud.PrintAmmoText(0);
                plAtts.hud.PrintAmmoText(0, false);
            }
        }

    }

}
