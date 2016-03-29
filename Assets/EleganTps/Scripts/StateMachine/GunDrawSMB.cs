using UnityEngine;
using System.Collections;


public class GunDrawSMB : CustomSMB
{
    private readonly int _toCoverTag = Animator.StringToHash("ToCover");
    private readonly int _pivotTag = Animator.StringToHash("Pivot");
    private readonly int _climbTag = Animator.StringToHash("Climb");
    private readonly int _drawTag = Animator.StringToHash("Draw");

    PlayerAtts plAtts;
    Transform rightHandHold;

    public override void Init(Animator anim)
    {
        plAtts = userInput.GetComponent<PlayerAtts>();
        rightHandHold = GameObject.FindGameObjectWithTag("RightHandHold").transform;
    }


    private bool CanDraw(AnimatorStateInfo baseStateInfo)
    {
        return !(baseStateInfo.tagHash == _toCoverTag || baseStateInfo.tagHash == _pivotTag ||
            baseStateInfo.tagHash == _climbTag);
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        plAtts.curVectorAngleWTarget = 180;
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        plAtts.isHandAwayFromGun = false;
        plAtts.isHandOnGun = false;
        plAtts.hud.clickedMenuItem = null;
        plAtts.hud.touchedItem = null;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        if (stateInfo.tagHash == _drawTag)
        {
            //// positioning gun
            //if (plAtts.cGun)
            //{
            //    plAtts.cGun.localPosition = Vector3.zero;
            //    plAtts.cGun.localRotation = Quaternion.identity;
            //    rightHandHold.localPosition = plAtts.cGun.GetComponent<GunAtt>().rightHandPositionRotation.localPosition;
            //    rightHandHold.localRotation = plAtts.cGun.GetComponent<GunAtt>().rightHandPositionRotation.localRotation;
            //}

            // change gun pos rot based on animation
            if (stateInfo.tagHash == _drawTag && plAtts.isHandOnGun)
            {
                plAtts.cGun = plAtts.lastGun;
                plAtts.cGun.SetParent(rightHandHold);
                plAtts.cGun.localPosition = Vector3.zero;
                plAtts.cGun.localRotation = Quaternion.identity;
                rightHandHold.localPosition = plAtts.cGun.GetComponent<GunAtt>().rightHandPositionRotation.localPosition;
                rightHandHold.localRotation = plAtts.cGun.GetComponent<GunAtt>().rightHandPositionRotation.localRotation;
                plAtts.isHandOnGun = false;
                GunAtt gunAtt = plAtts.cGun.GetComponent<GunAtt>();
                if (plAtts.hud)
                {
                    plAtts.hud.PrintAmmoText(gunAtt.currentClipCapacity);
                    plAtts.hud.PrintAmmoText(plAtts.currentMagz[gunAtt.bulletStyle], false);
                    if (gunAtt.isMelee)
                    {
                        plAtts.hud.PrintAmmoText(0);
                        plAtts.hud.PrintAmmoText(0, false);
                    }
                    if (gunAtt.hudImage)
                        plAtts.hud.weaponImg.sprite = gunAtt.hudImage;

                    if (gunAtt.GetComponent<WeaponParts>()) // if it uses weaponparts
                    {
                        if (gunAtt.secFire)
                        {
                            plAtts.hud.PrintSecondaryAmmoText(plAtts.currentMagz[gunAtt.secFire.GetComponent<SecondaryFireGP>().bulletStyle]);
                        }
                    }
                }
                if (plAtts.crosshair && gunAtt.crosshairSprite)
                    plAtts.crosshair.ChangeSprites(gunAtt.crosshairSprite);


                return;
            }
        }

        if (stateInfo.IsName("Empty"))
        {
            if (plAtts.lastGun && userInput.DrawDown && plAtts.Guns.Count > 0 && CanDraw(animator.GetCurrentAnimatorStateInfo(0)))   // draw the last gun with holster-Draw Button
            {
                GunAtt gunAtt = plAtts.lastGun.GetComponent<GunAtt>();
                // weapon draw sound
                if (gunAtt.sounds.draw)
                    gunAtt.sounds.draw.GetComponent<AudioSource>().Play();

                animator.SetInteger(_drawTag, plAtts.lastGun.GetComponent<GunAtt>().gunStyle);
                userInput.m_DrawDown = false;
                return;
            }
            else if ((plAtts.hud && plAtts.hud.clickedMenuItem) && !plAtts.cGun && plAtts.lastGun && plAtts.Guns.Count > 0 && CanDraw(animator.GetCurrentAnimatorStateInfo(0)))
            {
                GunAtt gunAtt = plAtts.lastGun.GetComponent<GunAtt>();
                // weapon draw sound
                if (gunAtt.sounds.draw)
                    gunAtt.sounds.draw.GetComponent<AudioSource>().Play();

                plAtts.lastGun = plAtts.hud.clickedMenuItem.go.transform;
                animator.SetInteger(_drawTag, plAtts.lastGun.GetComponent<GunAtt>().gunStyle);
                userInput.m_DrawDown = false;
                plAtts.hud.clickedMenuItem = null;
                return;
            }
        }

        // change weapon
        if (animator.GetInteger(_drawTag) > 0 && stateInfo.tagHash == _drawTag && plAtts.weaponToDraw != null)
        {
            animator.SetInteger(_drawTag, plAtts.weaponToDraw.GetComponent<GunAtt>().gunStyle);
            plAtts.lastGun = plAtts.weaponToDraw;
            plAtts.weaponToDraw = null;

            GunAtt gunAtt = plAtts.lastGun.GetComponent<GunAtt>();
            // weapon draw sound
            if (gunAtt.sounds.draw)
                gunAtt.sounds.draw.GetComponent<AudioSource>().Play();
            return;
        }

    }

}
