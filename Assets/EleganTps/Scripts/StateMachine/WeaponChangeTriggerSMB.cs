using UnityEngine;
using System.Collections;

public class WeaponChangeTriggerSMB : CustomSMB
{

    private PlayerAtts plAtts;

    public override void Init(Animator anim)
    {
        plAtts = userInput.GetComponent<PlayerAtts>();
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.IsInTransition(1))
            return;
        // change weapon with hud item menu
        if (plAtts.hud && plAtts.hud.clickedMenuItem && plAtts.hud.clickedMenuItem.go.transform != plAtts.cGun)
        {
            {
                if (plAtts.Guns.Count > 0)
                {
                    foreach (GameObject gunGo in plAtts.Guns)   // find this gun in plAtts
                    {
                        if (gunGo.transform == plAtts.hud.clickedMenuItem.go.transform)
                        {
                            animator.SetInteger("ChangeGun", gunGo.GetComponent<GunAtt>().gunStyle);
                            animator.SetInteger("Draw", gunGo.GetComponent<GunAtt>().gunStyle);
                            plAtts.weaponToDraw = gunGo.transform;
                            plAtts.hud.clickedMenuItem = null;
                            return;
                        }
                    }
                }
            }
        }
        else
            plAtts.hud.clickedMenuItem = null;

#if !MOBILE_INPUT
        /* rest is not really needed, used to change weapon with item buttons */
        if (userInput.Item1Down && plAtts.Guns.Count > 0 && plAtts.Guns[0].transform == plAtts.cGun)
        {
            if (plAtts.hud)
                plAtts.hud.ChangeInfoText("You are already using this weapon");
            return;
        }
        else if (userInput.Item2Down && plAtts.Guns.Count > 1 && plAtts.Guns[1].transform == plAtts.cGun)
        {
            if (plAtts.hud)
                plAtts.hud.ChangeInfoText("You are already using this weapon");
            return;
        }
        else if (userInput.Item3Down && plAtts.Guns.Count > 2 && plAtts.Guns[2].transform == plAtts.cGun)
        {
            if (plAtts.hud)
                plAtts.hud.ChangeInfoText("You are already using this weapon");
            return;
        }

        if (userInput.Item1Down && plAtts.Guns.Count > 0 && plAtts.Guns[0] != null)
        {
            animator.SetInteger("ChangeGun", plAtts.Guns[0].GetComponent<GunAtt>().gunStyle);
            animator.SetInteger("Draw", plAtts.Guns[0].GetComponent<GunAtt>().gunStyle);
            plAtts.weaponToDraw = plAtts.Guns[0].transform;

        }
        else if (userInput.Item2Down && plAtts.Guns.Count > 1 && plAtts.Guns[1] != null)
        {
            animator.SetInteger("ChangeGun", plAtts.Guns[0].GetComponent<GunAtt>().gunStyle);
            animator.SetInteger("Draw", plAtts.Guns[1].GetComponent<GunAtt>().gunStyle);
            plAtts.weaponToDraw = plAtts.Guns[1].transform;
        }
        else if (userInput.Item3Down && plAtts.Guns.Count > 2 && plAtts.Guns[2] != null)
        {
            animator.SetInteger("ChangeGun", plAtts.Guns[0].GetComponent<GunAtt>().gunStyle);
            animator.SetInteger("Draw", plAtts.Guns[2].GetComponent<GunAtt>().gunStyle);
            plAtts.weaponToDraw = plAtts.Guns[2].transform;

        }
#endif
    }

}
