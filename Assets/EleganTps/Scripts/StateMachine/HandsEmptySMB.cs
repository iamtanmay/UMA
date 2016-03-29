using UnityEngine;
using System.Collections;

public class HandsEmptySMB : CustomSMB
{
    private readonly int _locomotionState = Animator.StringToHash("Locomotion");

    private PlayerAtts plAtts;
    private Transform playerT;
    float lHandAim = 0, lHandAimV;
    float rHandAim = 0, rHandAimV;

    public float smooth = 1f;

    RaycastHit hitRight, hitLeft;
    bool isLeftWall, isRightWall;
    Ray rayLeft, rayRight;
    public override void Init(Animator anim)
    {
        plAtts = userInput.GetComponent<PlayerAtts>();
        playerT = plAtts.transform;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (plAtts.cGun || !plAtts.isWalking || animator.GetCurrentAnimatorStateInfo(0).shortNameHash != _locomotionState)
        {
            isRightWall = false;
            isLeftWall = false;
            return;
        }

        rayLeft = new Ray(playerT.position + playerT.TransformDirection(new Vector3(-plAtts.handsToWallRayCPos.x, plAtts.handsToWallRayCPos.y, plAtts.handsToWallRayCPos.z)), -playerT.right);
        rayRight = new Ray(playerT.position + playerT.TransformDirection(plAtts.handsToWallRayCPos), playerT.right);
        //Debug.DrawRay(rayLeft.origin,rayLeft.direction*plAtts.handsToWallDistLR, Color.red);
        //Debug.DrawRay(rayRight.origin, rayRight.direction*plAtts.handsToWallDistLR, Color.blue);

        if (Physics.Raycast(rayLeft, out hitLeft, plAtts.handsToWallDistLR, plAtts.handsToWallLayerMask))
        {
            //Debug.DrawRay(hitLeft.point, hitLeft.normal*.2f, Color.green);
            isLeftWall = true;
        }
        else
        {
            isLeftWall = false;
        }

        if (Physics.Raycast(rayRight, out hitRight, plAtts.handsToWallDistLR, plAtts.handsToWallLayerMask))
        {
            //Debug.DrawRay(hitRight.point, hitRight.normal*.2f, Color.green);
            isRightWall = true;
        }
        else
        {
            isRightWall = false;
        }

        rightHQua = Quaternion.Lerp(rightHQua, Quaternion.FromToRotation(rayLeft.origin, hitLeft.normal) * Quaternion.Euler(new Vector3(plAtts.handsToWallFix.x, -plAtts.handsToWallFix.y, -plAtts.handsToWallFix.z)), Time.deltaTime * rotSmooth);
        leftHQua = Quaternion.Lerp(leftHQua, Quaternion.FromToRotation(rayRight.origin, hitRight.normal) * Quaternion.Euler(plAtts.handsToWallFix), Time.deltaTime * rotSmooth);

        leftHPos = hitLeft.point + hitLeft.normal * plAtts.handDistFromWall;
        rightHPos = hitRight.point + hitRight.normal * plAtts.handDistFromWall;
    }
    private bool leftGoinZero = false;
    private bool rightGoinZero = false;
    private Quaternion rightHQua, leftHQua;
    private Vector3 rightHPos, leftHPos;
    public float rotSmooth = 10;
    public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (plAtts.cGun || !plAtts.isWalking || animator.GetCurrentAnimatorStateInfo(0).shortNameHash != _locomotionState)
        {
            isRightWall = false;
            isLeftWall = false;
            lHandAim = 0;
            rHandAim = 0;
            return;
        }
        if (isLeftWall && !leftGoinZero)
        {
            lHandAim = Mathf.SmoothDamp(lHandAim, 1, ref lHandAimV, smooth);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHPos);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, rightHQua);
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, lHandAim);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, lHandAim);
        }
        if (isRightWall && !rightGoinZero)
        {
            rHandAim = Mathf.SmoothDamp(rHandAim, 1, ref rHandAimV, smooth);
            animator.SetIKPosition(AvatarIKGoal.RightHand, rightHPos);
            animator.SetIKRotation(AvatarIKGoal.RightHand, leftHQua);
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, rHandAim);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, rHandAim);
        }

        if (!isLeftWall)
        {
            lHandAim = Mathf.SmoothDamp(lHandAim, 0, ref lHandAimV, smooth);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHPos);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHQua);
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, lHandAim);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, lHandAim);
            leftGoinZero = true;
            if (lHandAim < .05f)
                leftGoinZero = false;
        }
        if (!isRightWall || isLeftWall)
        {
            rHandAim = Mathf.SmoothDamp(rHandAim, 0, ref rHandAimV, smooth);
            animator.SetIKPosition(AvatarIKGoal.RightHand, rightHPos);
            animator.SetIKRotation(AvatarIKGoal.RightHand, rightHQua);
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, rHandAim);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, rHandAim);
            rightGoinZero = true;
            if (rHandAim < .05f)
                rightGoinZero = false;

        }
    }

}
