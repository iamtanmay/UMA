using UnityEngine;
using System.Collections;


public class GroundedControlSMB : CustomSMB
{

    private LayerMask groundLayer;
    private GroundedManager groundedManager = new GroundedManager();
    private readonly int _groundedPar = Animator.StringToHash("Grounded");

    public override void Init(Animator anim)
    {
        groundLayer = userInput.groundLayer;
        groundedManager.Init(anim, groundLayer);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool(_groundedPar, groundedManager.IsGrounded);
    }

    public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        groundedManager.CheckGroundedWithVelocity();
    }
}
