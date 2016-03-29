using UnityEngine;

public class JumpTrigger : CustomSMB
{
    private LayerMask groundLayer;
    public float maxDistanceCanJumpToClimb = 4f;
    private GroundedManager m_GroundedManager = new GroundedManager();

    private readonly int _jumpPar = Animator.StringToHash("Jump");
    private readonly int _groundedPar = Animator.StringToHash("Grounded");
    private readonly int _midAirTag = Animator.StringToHash("Midair");
    private readonly int _locomotionState = Animator.StringToHash("Locomotion");
    private readonly int _climbTag = Animator.StringToHash("Climb");

    private PlayerAtts plAtts;
    private Transform playerT;
    public bool alreadyJumping = false;
    public bool alreadyClimbing = false;
    public override void Init(Animator anim)
    {
        groundLayer = userInput.groundLayer;
        m_GroundedManager.Init(anim, groundLayer);
        plAtts = userInput.GetComponent<PlayerAtts>();
        playerT = plAtts.transform;
    }


    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Vector3 closePoint = Vector3.zero; float a = 0;
        if (plAtts.spline != null)
        {
            plAtts.spline.GetClosePointToPlayer(ref a, ref plAtts.climbPoint);
            closePoint = plAtts.climbPoint;
        }
        // Jump
        if (plAtts.spline == null || plAtts.cGun || Mathf.Abs(plAtts.climbPoint.y - plAtts.transform.position.y) > maxDistanceCanJumpToClimb
            || Vector3.Angle(playerT.forward, (new Vector3(closePoint.x, playerT.position.y, closePoint.z) - playerT.position).normalized) > 45
            )
        {
            alreadyJumping = animator.GetNextAnimatorStateInfo(layerIndex).tagHash == _midAirTag;
            if (userInput.JumpDown && m_GroundedManager.IsGrounded && !alreadyJumping && !alreadyClimbing)
            {
                animator.SetTrigger(_jumpPar);
                animator.SetBool(_groundedPar, false);
            }
        }
        // Climb
        else if (plAtts.spline != null && stateInfo.shortNameHash == _locomotionState && !animator.IsInTransition(0) && canClimb(stateInfo))
        {
            alreadyJumping = animator.GetNextAnimatorStateInfo(layerIndex).tagHash == _midAirTag;
            alreadyClimbing = animator.GetNextAnimatorStateInfo(layerIndex).tagHash == _climbTag;
            if (userInput.JumpDown && m_GroundedManager.IsGrounded && !alreadyClimbing && !alreadyJumping)
            {
                float closeStep = 0;
                plAtts.spline.GetClosePointToPlayer(ref closeStep, ref plAtts.climbPoint);
                float climbYDist = Mathf.Abs(plAtts.climbPoint.y - plAtts.transform.position.y);
                if (Mathf.Abs(plAtts.climbPoint.y - plAtts.transform.position.y) > maxDistanceCanJumpToClimb)
                {
                    return;
                }

                animator.SetFloat("ClimbYDistance", climbYDist);
                animator.SetTrigger("Climb");
                playerT.transform.rotation = Quaternion.LookRotation(new Vector3(plAtts.climbPoint.x, playerT.position.y, plAtts.climbPoint.z) - playerT.position);
            }
        }

    }

    bool canClimb(AnimatorStateInfo stateInfo)
    {
        bool canAim = !(plAtts.cGun);
        return canAim;
    }

    override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_GroundedManager.CheckGroundedWithVelocity();
    }
}
