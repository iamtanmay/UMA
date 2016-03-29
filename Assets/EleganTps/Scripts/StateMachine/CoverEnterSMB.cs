using UnityEngine;
using System.Collections;

public class CoverEnterSMB : CustomSMB
{
    PlayerAtts plAtts;
    public float speedPos = 5;
    public float speedRot = 15;
    Transform playerT;
    public override void Init(Animator anim)
    {
        plAtts = userInput.GetComponent<PlayerAtts>();
        playerT = plAtts.transform;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetFloat("CrouchStand", Mathf.Lerp(animator.GetFloat("CrouchStand"), plAtts.coverVars.crouchStand, Time.deltaTime * 10f));
        playerT.position = Vector3.Lerp(playerT.position, plAtts.coverVars.coverPosition + plAtts.coverVars.coverNormal * plAtts.coverVars.wallOffset, Time.deltaTime * speedPos);
        Vector3 fw = (Quaternion.Euler(0, 90 * plAtts.coverVars.lookTarget, 0)) * plAtts.coverVars.coverNormal;
        fw = new Vector3(fw.x, 0, fw.z);
        playerT.forward = Vector3.Lerp(playerT.forward, fw, Time.deltaTime * speedRot);
    }
}
