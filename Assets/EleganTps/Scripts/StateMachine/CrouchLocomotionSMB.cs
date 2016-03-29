using UnityEngine;
using System.Collections;

public class CrouchLocomotionSMB : CustomSMB
{
    private LayerMask groundLayer;
    private GroundedManager groundedManager = new GroundedManager();
    private Transform playerT;
    private PlayerAtts plAtts;
    private Vector3 targetLookAt;
    TPSCamera tpsCam;
    Rigidbody rb;

    public float controlDamp = .15f;
    public float rotationSpeed = 4f;

    private readonly int _crouchPar = Animator.StringToHash("Crouch");
    private readonly int _groundedPar = Animator.StringToHash("Grounded");
    private readonly int _velXPar = Animator.StringToHash("VelX");
    private readonly int _velYPar = Animator.StringToHash("VelY");
    private readonly int _speedPar = Animator.StringToHash("Speed");

    public override void Init(Animator anim)
    {
        groundLayer = userInput.groundLayer;
        groundedManager.Init(anim, groundLayer);
        playerT = userInput.transform;
        plAtts = userInput.GetComponent<PlayerAtts>();
        tpsCam = userInput.mainCamera.GetComponent<TPSCamera>();
        rb = userInput.GetComponent<Rigidbody>();
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetFloat(_speedPar, 0);
        plAtts.coverVars.crouchStand = 1;
        animator.SetFloat("CrouchStand", 0);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (userInput.CrouchDown)
        {
            animator.SetBool(_crouchPar, false);
            animator.SetFloat(_velXPar, 0);
            animator.SetFloat(_velYPar, 0);
            return;
        }

        Vector3 moveDirection = Vector3.zero;
        float targetAngle = 0f;
        if ((tpsCam.camMode == 0 && !tpsCam.moving && plAtts.canMove) || plAtts.canMove) // player movement in particular cam mods
            userInput.CalculateRefs(ref moveDirection, ref targetAngle);

        if (plAtts.controlMode == ControlMode.Free)
        {
            // smoothly rotate player
            float angleSmooth = Mathf.Lerp(0, targetAngle, moveDirection.magnitude * Time.deltaTime * rotationSpeed);

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
        }
        else if (plAtts.controlMode == ControlMode.Static)
        {
            Vector3 moveRefFw = new Vector3(userInput.moveReference.forward.x, playerT.position.y, userInput.moveReference.forward.z);
            if (!plAtts.isAiming)
            {
                float angleDif = Vector3.Angle(moveRefFw, playerT.forward);
                float dot = Vector3.Dot(Quaternion.Euler(0, -90, 0) * moveRefFw, playerT.forward);
                targetAngle = angleDif * dot;
                float angleSmooth = Mathf.Lerp(0, targetAngle, Time.deltaTime * rotationSpeed);

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
        }

        animator.SetFloat(_velXPar, moveDirection.x, controlDamp, Time.deltaTime);
        animator.SetFloat(_velYPar, moveDirection.z, controlDamp, Time.deltaTime);

        animator.SetBool(_groundedPar, groundedManager.IsGrounded);
    }

    public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Check whether the character is grounded.
        groundedManager.CheckGroundedWithVelocity();

    }

}
