using UnityEngine;
using System.Collections;

public class ReloadTriggerSMB : CustomSMB
{
    PlayerAtts plAtts;

    public override void Init(Animator anim)
    {
        plAtts = userInput.GetComponent<PlayerAtts>();
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        if (!plAtts.cGun)
            return;

        GunAtt gunAtt = plAtts.cGun.GetComponent<GunAtt>();

        if (userInput.ReloadDown)
        {
            if (gunAtt.currentClipCapacity >= gunAtt.maxClipCapacity)
            {
                if (plAtts.hud)
                    plAtts.hud.ChangeInfoText("Already have full clip");
                return;
            }
            else if (gunAtt.currentClipCapacity < gunAtt.maxClipCapacity && plAtts.currentMagz[gunAtt.bulletStyle] <= 0)
            {
                if (plAtts.hud)
                    plAtts.hud.ChangeInfoText("You have no ammo for this weapon");
                return;
            }
        }

        if (userInput.ReloadDown && !animator.IsInTransition(1) && gunAtt.currentClipCapacity < gunAtt.maxClipCapacity && plAtts.currentMagz[gunAtt.bulletStyle] > 0)
        {
            animator.SetTrigger("Reload");
            gunAtt.sounds.reload.GetComponent<AudioSource>().Play();
#if MOBILE_INPUT
            userInput.Fire2Press = false;
#endif

            if (gunAtt.curClipObject && gunAtt.isUsingClipReload)
            {
                gunAtt.clipDefLocalPos = gunAtt.curClipObject.localPosition;
                gunAtt.clipDefLocalRot = gunAtt.curClipObject.localRotation;

                // drop current clip
                gunAtt.curClipObject.GetComponent<Rigidbody>().isKinematic = false;
                gunAtt.curClipObject.GetComponent<Collider>().enabled = true;
                gunAtt.curClipObject.SetParent(null);
                gunAtt.curClipObject.gameObject.AddComponent<Destroy>();
                gunAtt.curClipObject.GetComponent<Destroy>().destroyTime = gunAtt.droppedClipDestroyTime;
                // new clip will be instantited with ReloadSMB
            }
        }

    }

}
