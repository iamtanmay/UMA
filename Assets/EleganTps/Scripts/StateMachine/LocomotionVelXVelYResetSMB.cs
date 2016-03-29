using UnityEngine;
using System.Collections;
public enum WhenToReset
{
    OnStateEnter, OnStateExit, TimeAfterEnter
}
public class LocomotionVelXVelYResetSMB : CustomSMB
{
    public float timeAfterEnter = 0;
    PlayerAtts plAtts;
    public WhenToReset whenToReset;
    public override void Init(Animator anim)
    {
        plAtts = userInput.GetComponent<PlayerAtts>();
    }
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (whenToReset == WhenToReset.OnStateEnter)
        {
            plAtts.StartCoroutine(plAtts.ResetVelXVelYCoroutine(0));
        }
        else if (whenToReset == WhenToReset.TimeAfterEnter)
        {
            plAtts.StartCoroutine(plAtts.ResetVelXVelYCoroutine(timeAfterEnter));
        }

    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (whenToReset == WhenToReset.OnStateExit)
        {
            plAtts.StartCoroutine(plAtts.ResetVelXVelYCoroutine(0));
        }

    }
}
