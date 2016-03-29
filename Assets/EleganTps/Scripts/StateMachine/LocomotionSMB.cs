using UnityEngine;
using System.Collections;

public class LocomotionSMB : CustomSMB
{
    public float controlDamp = .2f;
    public float rotationSpeed = 4f;
    public float timeToGoIdle = 8;
    public float speedDamp = .3f;

    #region Private Variables
    private LayerMask groundLayer;
    private GroundedManager groundedManager = new GroundedManager();
    private Transform playerT;
    private PlayerAtts plAtts;
    private Vector3 targetLookAt;
    private TPSCamera tpsCam;
    private Rigidbody rb;
    private float horizontal;
    private float vertical;
    private float lastSpeed = 50, curSpeed;
    private bool isSlowing = false;
    #endregion

    #region Animator Hash
    private readonly int _groundedPar = Animator.StringToHash("Grounded");
    private readonly int _crouchPar = Animator.StringToHash("Crouch");
    private readonly int _idleBoolPar = Animator.StringToHash("IdleBool");
    private readonly int _readyIdleTag = Animator.StringToHash("ReadyIdle");
    private readonly int _velXPar = Animator.StringToHash("VelX");
    private readonly int _velYPar = Animator.StringToHash("VelY");
    private readonly int _speedPar = Animator.StringToHash("Speed");
    private readonly int _anglePar = Animator.StringToHash("Angle");
    private readonly int _sprintPar = Animator.StringToHash("Sprint");
    private readonly int _stopPar = Animator.StringToHash("Stop");
    private readonly int _isLeftPar = Animator.StringToHash("IsLeft");
    #endregion

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
        plAtts.coverVars.crouchStand = 1;
        animator.SetFloat("CrouchStand", 1);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        #region Button Hits & Idle Animations
        // walkMode
        if (userInput.WalkToggleDown)
        {
            plAtts.isWalking = !plAtts.isWalking;
            userInput.m_WalkToggleDown = false;
        }
        // melee animations trigger
        if (userInput.Fire1Down)
        {
            if (plAtts.cGun)
                if (plAtts.cGun.GetComponent<GunAtt>().isMelee)
                {
                    userInput.m_Fire1Down = false;
                    animator.SetBool("Hit", true);
                }
        }

        // idle animations
        if ((Time.time - userInput.LastInputAt) > timeToGoIdle && !plAtts.isAiming && !animator.GetBool("LookAtWeapon"))
            animator.SetBool(_idleBoolPar, true);

        // crouch
        if (userInput.CrouchDown)
        {
            animator.SetBool(_crouchPar, true);
            userInput.m_CrouchDown = false;
            return;
        }
        if (userInput.SprintPress && animator.GetFloat(_velYPar) > .9f && animator.GetFloat(_velXPar) > -.52f && animator.GetFloat(_velXPar) < .52f)
            if ((!plAtts.cGun || (plAtts.cGun && plAtts.cGun.GetComponent<GunAtt>().canSprintOnHand)) && !plAtts.isAiming)
                animator.SetBool(_sprintPar, true);
            else
                animator.SetBool(_sprintPar, false);
        #endregion

        #region Sudden Stop
        if (plAtts.useSuddenStopAnimation)
        {
            // determine if we are slowing
            lastSpeed = curSpeed;
            curSpeed = animator.GetFloat(_speedPar);
            if (lastSpeed > curSpeed)
                isSlowing = true;
            else
                isSlowing = false;
            // if we are slowing and not pressing movement buttons play stop animation ( determine which foot is front by looking at pivotWeight )
            if (isSlowing && !plAtts.isAiming && plAtts.canMove && curSpeed > .9f && curSpeed < 1 && groundedManager.IsGrounded && Mathf.Abs(Input.GetAxisRaw("Horizontal")) < .05f && Mathf.Abs(Input.GetAxisRaw("Vertical")) < .05f && !animator.IsInTransition(0))
            {
                animator.SetTrigger(_stopPar);
                if (animator.pivotWeight > .5f)
                    animator.SetBool(_isLeftPar, true);
                else
                    animator.SetBool(_isLeftPar, false);
            }
            // reset stop trigger to prevent playing stop animation at wrong time
            if (curSpeed < .6f && !animator.GetAnimatorTransitionInfo(0).IsName("Sprint -> Locomotion"))
                animator.ResetTrigger(_stopPar);
        }

        #endregion

        horizontal = userInput.Horizontal;
        vertical = userInput.Vertical;
        Vector3 moveDirection = Vector3.zero;
        float targetAngle = 0f;

        // change move buttons(rotate axis') so that they match the camera(move reference) position
        if ((tpsCam.camMode == 0 && !tpsCam.moving && plAtts.canMove) || plAtts.canMove) // stop player movement in particular cam mod-s
            userInput.CalculateRefs(ref moveDirection, ref targetAngle);

        if (plAtts.controlMode == ControlMode.Free)
        {
            // smoothly rotate player
            float angleSmooth = Mathf.Lerp(0, targetAngle, moveDirection.magnitude * Time.deltaTime * rotationSpeed);

            if (!animator.GetNextAnimatorStateInfo(layerIndex).IsTag("Pivot"))
            {
                if (!plAtts.isAiming)
                {
                    rb.AddTorque(playerT.up * angleSmooth, ForceMode.VelocityChange);
                }
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

            // Set pivot angle if player is not aiming(some calcs in animator controller, speed-angle etc.)
            if (!plAtts.isAiming)
            {
                if (moveDirection != Vector3.zero && !animator.IsInTransition(0))
                {
                    animator.SetFloat(_anglePar, -1 * targetAngle);
                }
            }
            else
            {
                // otherwise make sure we dont pivot
                animator.SetFloat(_anglePar, 0);
            }
            if (animator.GetCurrentAnimatorStateInfo(1).tagHash == _readyIdleTag)
                animator.SetFloat(_anglePar, 0);
        }
        #region Static Control Mode
        else if (plAtts.controlMode == ControlMode.Static)
        {
            animator.ResetTrigger(_stopPar);
            Vector3 moveRefFw = new Vector3(userInput.moveReference.forward.x, playerT.position.y, userInput.moveReference.forward.z);

            if (!plAtts.isAiming)
            {
                float angleDif = Vector3.Angle(moveRefFw, playerT.forward);
                float dot = Vector3.Dot(Quaternion.Euler(0, -90, 0) * moveRefFw, playerT.forward);
                targetAngle = angleDif * dot;
                float angleSmooth = Mathf.Lerp(0, targetAngle, Time.deltaTime * rotationSpeed);
                animator.SetFloat(_anglePar, -1 * targetAngle);
                rb.AddTorque(playerT.up * angleSmooth, ForceMode.VelocityChange);
            }


            if (plAtts.isAiming)
            {
                Vector3 fromTargetToPlayer = -new Vector3(plAtts.target.position.x, playerT.position.y, plAtts.target.position.z) + playerT.position;
                float angle1 = Vector3.Angle(playerT.forward, -fromTargetToPlayer);
                float dot1 = Vector3.Dot(playerT.forward, Quaternion.Euler(0, 90, 0) * fromTargetToPlayer);

                if (!plAtts.turningWithGun)
                    rb.AddTorque(dot1 * angle1 * playerT.up * Time.deltaTime * plAtts.toAimRotationForce, ForceMode.VelocityChange);
                else // when approached epsilon target angle(plAtts variable) force will be applied
                    playerT.Rotate(Vector3.up, Time.deltaTime * plAtts.toAimRotationSpeed * dot1);  // faster transition when there is a big angle to turn

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

            // Set pivot angle if player is not aiming(some calcs in animator controller, speed-angle etc.)
            if (!plAtts.isAiming)
            {
                if (moveDirection != Vector3.zero && !animator.IsInTransition(0))
                {
                    animator.SetFloat(_anglePar, -1 * targetAngle);
                }
            }
            else
            {
                // otherwise make sure we dont pivot
                animator.SetFloat(_anglePar, 0);
            }
            if (animator.GetCurrentAnimatorStateInfo(1).tagHash == _readyIdleTag)
                animator.SetFloat(_anglePar, 0);
        }
        #endregion

        // get speed from buttons but dont let it go over magnitude of 1
        float sqrM = Vector2.SqrMagnitude(new Vector2(horizontal, vertical)) > 1 ? Vector2.SqrMagnitude(new Vector2(horizontal, vertical).normalized) : Vector2.SqrMagnitude(new Vector2(horizontal, vertical));
        animator.SetFloat(_speedPar, sqrM, speedDamp, Time.deltaTime);

        // set locomotion vars
        if (!plAtts.isWalking)
        {
            animator.SetFloat(_velXPar, moveDirection.x, controlDamp, Time.deltaTime);
            animator.SetFloat(_velYPar, moveDirection.z, controlDamp, Time.deltaTime);
        }

        #region Walk Calculations
        float walkSpeed = plAtts.walkSpeedLocomotion;
        float x1 = moveDirection.x, z1 = moveDirection.z;
        if (plAtts.isWalking)
        {
            if (x1 > 0)
            {
                x1 = x1 > walkSpeed ? walkSpeed : x1;
            }
            else if (x1 < 0)
            {
                x1 = -x1 > walkSpeed ? -walkSpeed : x1;
            }
            if (z1 > 0)
            {
                z1 = z1 > walkSpeed ? walkSpeed : z1;
            }
            else if (z1 < 0)
            {
                z1 = -z1 > walkSpeed ? -walkSpeed : z1;
            }

            animator.SetFloat(_velXPar, x1, controlDamp, Time.deltaTime);
            animator.SetFloat(_velYPar, z1, controlDamp, Time.deltaTime);
            sqrM = Vector2.SqrMagnitude(new Vector2(x1, z1)) > walkSpeed ? Vector2.SqrMagnitude(new Vector2(x1, z1).normalized) - .1f : Vector2.SqrMagnitude(new Vector2(x1, z1));
            animator.SetFloat(_speedPar, sqrM, .25f, Time.deltaTime);
        }
        #endregion



        animator.SetBool(_groundedPar, groundedManager.IsGrounded);
    }

    //private float headAim = 0f, headAim_V;
    public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Check whether the character is grounded.
        groundedManager.CheckGroundedWithVelocity();
    }


}
