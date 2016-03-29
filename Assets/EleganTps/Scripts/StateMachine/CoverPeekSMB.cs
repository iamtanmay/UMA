using UnityEngine;
using System.Collections;

public class CoverPeekSMB : CustomSMB
{

    private Transform playerT;
    private PlayerAtts plAtts;
    CoverVars cv;

    public override void Init(Animator anim)
    {
        plAtts = userInput.GetComponent<PlayerAtts>();
        playerT = userInput.transform;
        cv = plAtts.coverVars;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        #region angle calculator
        Vector3 relationalDir = Quaternion.Euler(0, 90, 0) * (new Vector3(plAtts.target.position.x, playerT.position.y, plAtts.target.position.z) - playerT.position);
        cv.angle = Vector3.Angle(playerT.right, relationalDir);
        cv.dot = -Mathf.Sign(Vector3.Dot(playerT.right, Quaternion.Euler(0, -90, 0) * relationalDir));
        animator.SetFloat("AngleCover", cv.angle * cv.dot);
        #endregion

        if (plAtts.isAiming)
        {
            plAtts.turningWithGun = false;
            plAtts.curVectorAngleWTarget = 0;
        }
        if (!(userInput.Fire2Press || plAtts.debugFire2Press))
        {
            animator.SetBool("EdgePeek", false);
            animator.SetBool("UpPeek", false);
        }

    }
}
