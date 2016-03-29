using UnityEngine;
using System.Collections;

public class ClimbTargetMatchSMB : CustomSMB
{

    PlayerAtts plAtts;
    public float matchStart;
    public float matchEnd;
    public override void Init(Animator anim)
    {
        plAtts = userInput.GetComponent<PlayerAtts>();
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!animator.IsInTransition(0))
            animator.MatchTarget(plAtts.climbPoint, Quaternion.identity, AvatarTarget.RightHand, new MatchTargetWeightMask(new Vector3(1, 1, 1), 0),
                     matchStart, matchEnd);
    }

}
