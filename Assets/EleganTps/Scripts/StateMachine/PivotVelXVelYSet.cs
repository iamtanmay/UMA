using UnityEngine;
using System.Collections;

public class PivotVelXVelYSet : CustomSMB
{
    PlayerAtts plAtts;
    private Transform playerT;
    public float rotationSpeed = 4f;
    public override void Init(Animator anim)
    {
        plAtts = userInput.GetComponent<PlayerAtts>();
        playerT = userInput.transform;
    }

    private float horizontal;
    private float vertical;
    Rigidbody rb;
    public float yd_Dump = .25f;

    private readonly int _velXPar = Animator.StringToHash("VelX");
    private readonly int _velYPar = Animator.StringToHash("VelY");
    private readonly int _speedPar = Animator.StringToHash("Speed");
    private readonly int _anglePar = Animator.StringToHash("Angle");

    private readonly int _idlePivotRightState = Animator.StringToHash("IdlePivotRight");
    private readonly int _idlePivotLeftState = Animator.StringToHash("IdlePivotLeft");

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        rb = userInput.GetComponent<Rigidbody>();
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
        animator.SetFloat(_speedPar, Vector2.SqrMagnitude(new Vector2(horizontal, vertical)), .25f, Time.deltaTime);

        animator.SetFloat(_velXPar, moveDirection.x, yd_Dump, Time.deltaTime);
        animator.SetFloat(_velYPar, moveDirection.z, yd_Dump, Time.deltaTime);

        if (stateInfo.shortNameHash == _idlePivotLeftState || stateInfo.shortNameHash == _idlePivotRightState)
        {
            float walkSpeed = plAtts.walkSpeedLocomotion;
            float x = moveDirection.x > walkSpeed ? walkSpeed : moveDirection.x;
            float z = moveDirection.z > walkSpeed ? walkSpeed : moveDirection.z;
            animator.SetFloat(_velXPar, x);
            animator.SetFloat(_velYPar, z);
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetFloat(_anglePar, 0);
    }
}
