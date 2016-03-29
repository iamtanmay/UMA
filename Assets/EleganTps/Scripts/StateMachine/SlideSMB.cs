using UnityEngine;
using System.Collections;

public class SlideSMB : CustomSMB
{

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (userInput.CrouchDown && !animator.GetAnimatorTransitionInfo(0).IsName("Slide -> CrouchLocomotion"))
        {
            animator.SetBool("Crouch", true);
        }


    }

}
