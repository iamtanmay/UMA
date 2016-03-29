using UnityEngine;
using System.Collections;

public class SprintSMB : CustomSMB
{
    public float controlDamp = .25f;
    public float rotationSpeed = 4f;
    public float speedDamp = .11f;
    private LayerMask groundLayer;
    private GroundedManager groundedManager = new GroundedManager();
    private PlayerAtts plAtts;
    private float horizontal;
    private float vertical;
    private Transform playerT;
    private Rigidbody rb;
    private TPSCamera tpsCam;

    #region Animation Hash
    private readonly int _groundedPar = Animator.StringToHash("Grounded");
    private readonly int _velXPar = Animator.StringToHash("VelX");
    private readonly int _velYPar = Animator.StringToHash("VelY");
    private readonly int _speedPar = Animator.StringToHash("Speed");
    private readonly int _anglePar = Animator.StringToHash("Angle");
    private readonly int _sprintPar = Animator.StringToHash("Sprint");
    private readonly int _slidePar = Animator.StringToHash("Slide");
    #endregion

    public override void Init(Animator anim)
    {
        rb = userInput.GetComponent<Rigidbody>();
        groundLayer = userInput.groundLayer;
        groundedManager.Init(anim, groundLayer);
        plAtts = userInput.GetComponent<PlayerAtts>();
        playerT = userInput.transform;
        tpsCam = userInput.mainCamera.GetComponent<TPSCamera>();

    }
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool(_slidePar, false);
    }
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.GetFloat(_anglePar) > 135 || animator.GetFloat(_anglePar) < -135)
        {
            animator.SetBool(_sprintPar, true);
            return;
        }

        if ((plAtts.isAiming || animator.GetFloat(_velYPar) < .95f || (animator.GetFloat(_velXPar) < -.55f || animator.GetFloat(_velXPar) > .55f)))
        {
            animator.SetBool(_sprintPar, false);
        }
        else if (userInput.SprintPress && !plAtts.isAiming)
        {
            animator.SetBool(_sprintPar, true);
        }
        else
            animator.SetBool(_sprintPar, false);

        // Sliding
        if (userInput.CrouchDown && !animator.IsInTransition(0))
        {
            animator.SetTrigger(_slidePar);
            userInput.m_CrouchDown = false;
            return;
        }

        horizontal = userInput.Horizontal;
        vertical = userInput.Vertical;
        Vector3 moveDirection = Vector3.zero;
        float targetAngle = 0f;

        if (tpsCam.camMode == 0 && !tpsCam.moving) // stop player movement in particular cam mod-s
            userInput.CalculateRefs(ref moveDirection, ref targetAngle);

        if (plAtts.controlMode == ControlMode.Free)
        {
            float angleSmooth = Mathf.Lerp(0, targetAngle, moveDirection.magnitude * Time.deltaTime * rotationSpeed);

            if (moveDirection != Vector3.zero || !animator.IsInTransition(0))
                rb.AddTorque(playerT.up * angleSmooth, ForceMode.VelocityChange);

            if (moveDirection != Vector3.zero && !animator.IsInTransition(0))
            {
                animator.SetFloat(_anglePar, -1 * targetAngle);
            }

            float sqrM = Vector2.SqrMagnitude(new Vector2(horizontal, vertical)) > 1 ? Vector2.SqrMagnitude(new Vector2(horizontal, vertical).normalized) : Vector2.SqrMagnitude(new Vector2(horizontal, vertical));
            animator.SetFloat(_speedPar, sqrM, speedDamp, Time.deltaTime);
        }
        else if (plAtts.controlMode == ControlMode.Static)
        {
            Vector3 moveRefFw = new Vector3(userInput.moveReference.forward.x, playerT.position.y, userInput.moveReference.forward.z);
            float angleDif = Vector3.Angle(moveRefFw, playerT.forward);
            float dot = Vector3.Dot(Quaternion.Euler(0, -90, 0) * moveRefFw, playerT.forward);
            targetAngle = angleDif * dot;
            float angleSmooth = Mathf.Lerp(0, targetAngle, Time.deltaTime * rotationSpeed);

            rb.AddTorque(playerT.up * angleSmooth, ForceMode.VelocityChange);
        }

        animator.SetFloat(_velXPar, moveDirection.x, controlDamp, Time.deltaTime);
        animator.SetFloat(_velYPar, 1, controlDamp, Time.deltaTime);
        animator.SetBool(_groundedPar, groundedManager.IsGrounded);

    }

    public override void OnStateMachineExit(Animator animator, int stateMachinePathHash)
    {
        animator.SetFloat(_speedPar, 1);
    }

    public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Check whether the character is grounded.
        groundedManager.CheckGroundedWithVelocity();
    }

}
