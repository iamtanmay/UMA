using UnityEngine;
using System.Collections;

public class StopSMB : CustomSMB
{

    PlayerAtts plAtts;
    private Transform playerT;
    private float horizontal;
    private float vertical;
    private Rigidbody rb;
    public float yd_Dump = .25f;
    private readonly int _speedPar = Animator.StringToHash("Speed");
    private readonly int _anglePar = Animator.StringToHash("Angle");

    public override void Init(Animator anim)
    {
        plAtts = userInput.GetComponent<PlayerAtts>();
        playerT = userInput.transform;
        rb = plAtts.GetComponent<Rigidbody>();
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetFloat(_speedPar, 0);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        horizontal = userInput.Horizontal;
        vertical = userInput.Vertical;
        Vector3 moveDirection = Vector3.zero;
        float targetAngle = 0f;
        userInput.CalculateRefs(ref moveDirection, ref targetAngle);

        // if control mode is static continue to rotate player smoothly like in locomotion state
        if (plAtts.controlMode == ControlMode.Static)
        {
            Vector3 moveRefFw = new Vector3(userInput.moveReference.forward.x, playerT.position.y, userInput.moveReference.forward.z);
            float angleDif = Vector3.Angle(moveRefFw, playerT.forward);
            float dot = Vector3.Dot(Quaternion.Euler(0, -90, 0) * moveRefFw, playerT.forward);
            targetAngle = angleDif * dot;
            float angleSmooth = Mathf.Lerp(0, targetAngle, Time.deltaTime * 4.5f);
            animator.SetFloat(_anglePar, -1 * targetAngle);

            rb.AddTorque(playerT.up * angleSmooth, ForceMode.VelocityChange);
        }
        animator.SetFloat(_speedPar, Vector2.SqrMagnitude(new Vector2(horizontal, vertical)), .05f, Time.deltaTime);
        if (!animator.IsInTransition(0))
            animator.SetFloat(_anglePar, -1 * targetAngle);
        else
            animator.SetFloat(_anglePar, 0);
    }

}
