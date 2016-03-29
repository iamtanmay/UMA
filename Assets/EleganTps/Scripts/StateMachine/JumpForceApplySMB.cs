using UnityEngine;
using System.Collections;

public class JumpForceApplySMB : CustomSMB
{
    public enum WhenToApplyJumpForce
    {
        OnStateEnter, OnStateExit, TimeAfterEnter
    }
    public enum WhichForceToApply
    {
        IdleJumping, RunJumping,
    }

    public WhenToApplyJumpForce whenToApplyForce;
    public WhichForceToApply whichForceToApply;
    public float timeAfterEnterToApplyForce = .2f;
    private float tempTime;

    private PlayerAtts plAtts;
    private Rigidbody rb;

    public override void Init(Animator anim)
    {
        plAtts = userInput.GetComponent<PlayerAtts>();
        rb = userInput.GetComponent<Rigidbody>();
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (whenToApplyForce == WhenToApplyJumpForce.OnStateEnter)
        {
            if (whichForceToApply == WhichForceToApply.IdleJumping)
                rb.AddForce(new Vector3(0, plAtts.idleJumpUpForceUp, 0), ForceMode.Impulse);
            else
                rb.AddForce(new Vector3(0, plAtts.runJumpUpForceUp, 0), ForceMode.Impulse);
        }
        else if (whenToApplyForce == WhenToApplyJumpForce.TimeAfterEnter)
        {
            tempTime = timeAfterEnterToApplyForce;
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (whenToApplyForce != WhenToApplyJumpForce.TimeAfterEnter)
            return;
        if (tempTime < 0)
        {
            if (whichForceToApply == WhichForceToApply.IdleJumping)
                rb.AddForce(new Vector3(0, plAtts.idleJumpUpForceUp, 0), ForceMode.Impulse);
            else
                rb.AddForce(new Vector3(0, plAtts.runJumpUpForceUp, 0), ForceMode.Impulse);
            tempTime = 999999;
        }
        tempTime -= Time.deltaTime;
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (whenToApplyForce == WhenToApplyJumpForce.OnStateExit)
        {
            if (whichForceToApply == WhichForceToApply.IdleJumping)
                rb.AddForce(new Vector3(0, plAtts.idleJumpUpForceUp, 0), ForceMode.Impulse);
            else
                rb.AddForce(new Vector3(0, plAtts.runJumpUpForceUp, 0), ForceMode.Impulse);
        }

        //if (stateInfo.IsName("RunJumpStart"))
        //{
        //   // rb.AddForce(rb.transform.forward * plAtts.runJumpForceForward, ForceMode.Impulse);  
        //}
    }
}
