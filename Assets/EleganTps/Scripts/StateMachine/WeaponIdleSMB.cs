using UnityEngine;
using System.Collections;

public class WeaponIdleSMB : CustomSMB
{
    #region Animator Hash
    private readonly int _climbTag = Animator.StringToHash("Climb");
    private readonly int _rollState = Animator.StringToHash("Roll");
    private readonly int _footOnGroundState = Animator.StringToHash("FootOnGround");
    private readonly int _handOnGroundState = Animator.StringToHash("HandOnGround");
    private readonly int _pivotTag = Animator.StringToHash("Pivot");
    private readonly int _aimPar = Animator.StringToHash("Aim");
    private readonly int _locomotionState = Animator.StringToHash("Locomotion");
    private readonly int _crouchLocomotionState = Animator.StringToHash("CrouchLocomotion");
    #endregion

    private PlayerAtts plAtts;
    private Transform cGunT;
    private TPSCamera tpsCam;
    private GunAtt gunAtt;

    public override void Init(Animator anim)
    {
        plAtts = userInput.GetComponent<PlayerAtts>();
        tpsCam = userInput.mainCamera.GetComponent<TPSCamera>();
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        plAtts.weaponToDraw = null;
        plAtts.turningWithGun = false;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (userInput.FlashLightDown && cGunT && cGunT.GetComponent<WeaponParts>())
        {
            plAtts.WeaponFlashLightONOFF(cGunT.GetComponent<WeaponParts>());
            userInput.m_FlashLightDown = false;
        }

        if (userInput.ChangeGunPartsDown && plAtts.cGun && plAtts.cGun.GetComponent<WeaponParts>() && !animator.IsInTransition(1) && animator.GetCurrentAnimatorStateInfo(0).shortNameHash == _locomotionState)    // move fw to look at gun
        {
            tpsCam.camMode = 1;
            tpsCam.moving = true;
            animator.SetFloat("VelX", 0);
            animator.SetFloat("VelY", 0);
            tpsCam.focusedPartHolderIndex = 0;
            animator.SetBool("LookAtWeapon", true);
            userInput.movementInputEnabled = false;
            return;
        }

        if (userInput.DrawDown)
        {
            animator.SetInteger("Draw", 0);
        }

        if (!plAtts.cGun)
            return;
        gunAtt = plAtts.cGun.GetComponent<GunAtt>();
        cGunT = plAtts.cGun.transform;

        // drop cGun pressing g
        if (userInput.DropDown && plAtts.cGun && !animator.IsInTransition(1))
        {
            userInput.m_DropDown = false;
            plAtts.cGun.SetParent(null);
            Transform gun = plAtts.cGun;
            plAtts.cGun = null;
            gun.GetComponent<Rigidbody>().isKinematic = false;
            gun.GetComponent<BoxCollider>().enabled = true;
            gun.GetComponent<SphereCollider>().enabled = true;
            gun.GetComponent<Rigidbody>().AddForce(plAtts.transform.forward * 9f);
            animator.SetBool("DropGun", true);
            animator.SetInteger("Draw", 0);
            plAtts.hud.DeleteFromGoMenu(gun.gameObject);
            plAtts.lastGun = null;
            GameObject gunItemInList = plAtts.Guns.Find(a => a.GetComponent<GunAtt>().codeNumber == gun.GetComponent<GunAtt>().codeNumber);
            plAtts.Guns.Remove(gunItemInList);
            return;
        }

        if ((userInput.Fire2Press || plAtts.debugFire2Press || userInput.FirePress) && CanAim(animator.GetCurrentAnimatorStateInfo(0)) && !gunAtt.isMelee)
        {
            plAtts.turningWithGun = true;   // turn to targetPosition
            if (plAtts.curVectorAngleWTarget < plAtts.minVectorAngleWTargToRaiseWeapon) // raise weapon after turning enough
            {
                animator.SetBool(_aimPar, true);

            }
            plAtts.isAiming = true;  // turn to targetPosition

        }
        else
            plAtts.curVectorAngleWTarget = 180;

        if (!(userInput.Fire2Press || plAtts.debugFire2Press || userInput.FirePress))
        {
            plAtts.isAiming = false;
            plAtts.turningWithGun = false;
        }

    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("DropGun", false);
    }

    private bool CanAim(AnimatorStateInfo baseStateInfo)
    {
        bool canAim = !(
     baseStateInfo.tagHash == _climbTag || baseStateInfo.shortNameHash == _rollState || baseStateInfo.shortNameHash == _footOnGroundState || baseStateInfo.shortNameHash == _handOnGroundState ||
     baseStateInfo.tagHash == _pivotTag || (plAtts.isGunCollidingWall && (baseStateInfo.shortNameHash == _locomotionState || baseStateInfo.shortNameHash == _crouchLocomotionState))
     );
        return canAim;
    }


}
