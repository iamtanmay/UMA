using UnityEngine;
using System.Collections;

public class JumpAirControlSMB : CustomSMB
{

    private readonly int _groundedPar = Animator.StringToHash("Grounded");
    private readonly int _speedPar = Animator.StringToHash("Speed");
    private readonly int _VelocityYPar = Animator.StringToHash("VelocityY");
    private readonly int _RunJumpStartState = Animator.StringToHash("RunJumpStart");
    private readonly int _idleJumpStartState = Animator.StringToHash("IdleJumpStart");
    private readonly int _FootOnGroundState = Animator.StringToHash("FootOnGround");
    private readonly int _toIdleState = Animator.StringToHash("ToIdle");
    private readonly int _HandOnGroundState = Animator.StringToHash("HandOnGround");
    private readonly int _rollState = Animator.StringToHash("Roll");
    private readonly int _toRunState = Animator.StringToHash("ToRun");
    private readonly int _handOnGroundWaitState = Animator.StringToHash("HandOnGroundWait");
    private readonly int _handOnGroundToIdleState = Animator.StringToHash("HandOnGroundToIdle");
    private readonly int _idleTorise = Animator.StringToHash("IdleToRise");
    private readonly int _riseToTop = Animator.StringToHash("RiseToTop");
    private readonly int _topPose = Animator.StringToHash("TopPose");
    private readonly int _topToFall = Animator.StringToHash("TopToFall");
    private readonly int _fall = Animator.StringToHash("Fall");

    private Transform playerT;
    private PlayerAtts plAtts;
    private float horizontal;
    private float vertical;
    private Vector3 targetLookAt;
    private Rigidbody rb;

    public override void Init(Animator anim)
    {
        playerT = userInput.transform;
        plAtts = userInput.GetComponent<PlayerAtts>();
        rb = userInput.GetComponent<Rigidbody>();
    }

    public override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        horizontal = userInput.Horizontal;
        vertical = userInput.Vertical;

        if (!animator.GetBool(_groundedPar))
            animator.SetFloat(_VelocityYPar, playerT.GetComponent<Rigidbody>().velocity.y);

        // mid air states that u cant have air control
        if (stateInfo.shortNameHash == _RunJumpStartState || stateInfo.shortNameHash == _idleJumpStartState || stateInfo.shortNameHash == _FootOnGroundState ||
            stateInfo.shortNameHash == _toIdleState || stateInfo.shortNameHash == _HandOnGroundState || stateInfo.shortNameHash == _rollState ||
            stateInfo.shortNameHash == _toRunState || stateInfo.shortNameHash == _handOnGroundWaitState || stateInfo.shortNameHash == _handOnGroundToIdleState)
            return;

        Vector3 moveDirection = Vector3.zero;
        float targetAngle = 0f;
        userInput.CalculateRefs(ref moveDirection, ref targetAngle);

        // smoothly rotate player
        float angleSmooth = Mathf.Lerp(0, targetAngle, moveDirection.magnitude * Time.deltaTime * plAtts.airRotationSpeed);

        if (!plAtts.isAiming)
        {
            rb.AddTorque(playerT.up * angleSmooth, ForceMode.VelocityChange);
        }

        if (plAtts.isAiming)
        {
            Vector3 fromTargetToPlayer = -new Vector3(plAtts.target.position.x, playerT.position.y, plAtts.target.position.z) + playerT.position;
            float angle = Vector3.Angle(playerT.forward, -fromTargetToPlayer);
            float dot = Vector3.Dot(playerT.forward, Quaternion.Euler(0, 90, 0) * fromTargetToPlayer);

            if (!plAtts.turningWithGun)
                rb.AddTorque(dot * angle * playerT.up * Time.deltaTime * plAtts.toAimRotationForce, ForceMode.VelocityChange);
            else // when approached epsilon target angle(plAtts variable) force will be applied
                playerT.Rotate(Vector3.up, Time.deltaTime * plAtts.toAimRotationSpeed * dot);  // faster transition when there is a big angle to turn

            Vector3 fromTargDir = new Vector3(plAtts.target.position.x, playerT.position.y, plAtts.target.position.z) - playerT.position;
            float vectorAngleWTarget = Vector3.Angle(fromTargDir, playerT.forward);
            plAtts.curVectorAngleWTarget = vectorAngleWTarget;
            if (vectorAngleWTarget < plAtts.vectorAngleWTargEpsilon)
            {
                plAtts.turningWithGun = false;
            }
            else
                plAtts.turningWithGun = true;
        }

        if (isOnAir(stateInfo))
        {
            Vector3 stickDirection = new Vector3(userInput.Horizontal, 0, userInput.Vertical);
            Vector3 camDirection = userInput.moveReference.forward;
            camDirection.y = 0;
            Quaternion refShift1 = Quaternion.FromToRotation(Vector3.forward, camDirection);
            Vector3 targetMoveDirection = refShift1 * stickDirection;
            rb.MovePosition(playerT.position + targetMoveDirection * plAtts.airControlAmount * Time.deltaTime);
        }

        if (rb.velocity.y < plAtts.airDownForceStartVelocity)
        {
            rb.AddForce(Vector3.down * plAtts.airDownForce, ForceMode.Acceleration);
        }

        float sqrM = Vector2.SqrMagnitude(new Vector2(horizontal, vertical)) > 1 ? Vector2.SqrMagnitude(new Vector2(horizontal, vertical).normalized) : Vector2.SqrMagnitude(new Vector2(horizontal, vertical));
        animator.SetFloat(_speedPar, sqrM, .2f, Time.deltaTime);
    }

    public override void OnStateMachineExit(Animator animator, int stateMachinePathHash)
    {
        animator.SetFloat(_VelocityYPar, 0);
    }

    bool isOnAir(AnimatorStateInfo si) // states that we can control
    {
        return (si.shortNameHash == _idleTorise || si.shortNameHash == _riseToTop || si.shortNameHash == _riseToTop || si.shortNameHash == _topPose ||
            si.shortNameHash == _topToFall || si.shortNameHash == _fall);
    }
}
