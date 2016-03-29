using UnityEngine;
using System.Collections;

public class MeleeHitsSMB : CustomSMB
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("Hit", false);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (userInput.Fire1Down)
        {
            userInput.m_Fire1Down = false;
            animator.SetBool("Hit", true);
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("Hit", false);
    }
}
