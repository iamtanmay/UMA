using UnityEngine;
using System.Collections;

public class FootAlign : MonoBehaviour
{
    // Foot placing states
    private readonly int _LocomotionState = Animator.StringToHash("Locomotion");
    private readonly int _coverLocomotionState = Animator.StringToHash("CoverLocomotion");
    private readonly int _sprintState = Animator.StringToHash("Sprint");
    private readonly int _crouchState = Animator.StringToHash("CrouchLocomotion");
    private readonly int _jogPivotRightState = Animator.StringToHash("JogPivotRight");
    private readonly int _jogPivotLeftState = Animator.StringToHash("JogPivotLeft");
    private readonly int _stopLeftState = Animator.StringToHash("StopLeftFront");
    private readonly int _idlePivotLeft = Animator.StringToHash("IdlePivotLeft");
    private readonly int _idlePivotRight = Animator.StringToHash("IdlePivotRight");
    private readonly int _stopRightState = Animator.StringToHash("StopRightFront");

    public LayerMask footRayLayer;
    Animator anim;
    Transform leftFoot;
    Transform rightFoot;
    PlayerAtts plAtts;
    private float headAim, headAimV;
    float lcurAim, ltargAim, laimV, rcurAim, rtargAim, raimV;
    float lcurAimRot, ltargAimRot, laimVRot, rcurAimRot, rtargAimRot, raimVRot;
    Vector3 ltargPos, rtargPos;
    Quaternion ltargRot, rtargRot;
    float curPosY_L, targPosY_L, curPosYV_L;
    float curPosY_R, targPosY_R, curPosYV_R;
    private Quaternion lerpRotL, lerpRotR;
    CapsuleCollider m_CapCollider;
    Vector3 capColDefaultCenter;
    float capColDefaultHeight;

    public float curveEffector = .225f;
    public float footGroundFixOffset = .15f;
    public float rayDistFromFootPosition = 1.5f;
    public float rayDistFromFootRotation = .4f; // must be lower than rayDistFromFootPosition
    public Vector3 leftFootRotationFix;
    public Vector3 rightFootRotationFix;
    public float posSmooth = .1f;
    public float rotSmooth = .4f;
    public float rayDifToUp = .2f;
    public float footHeightChangeSpeed = 4;
    public float footHeightLerpPeriod = 2.5f;
    public float footRotChangeSpeed = 35;
    public float footRotationLerpPeriod = 2.5f;
    public bool slopeEnabled = true;
    public float slopeColliderSmooth = 15f;
    public float slopeDistance = .7f;
    public float slopeDownRayDist = 1f;
    public float slopeUpRayDist = 2f;
    void Start()
    {
        plAtts = GetComponent<PlayerAtts>();
        anim = GetComponent<Animator>();
        leftFoot = anim.GetBoneTransform(HumanBodyBones.LeftFoot).transform;
        rightFoot = anim.GetBoneTransform(HumanBodyBones.RightFoot).transform;
        m_CapCollider = anim.GetComponent<CapsuleCollider>();
        capColDefaultHeight = m_CapCollider.height;
        capColDefaultCenter = m_CapCollider.center;
    }

    void Update()
    {
        if (slopeEnabled)
        {
            if (canSlope() && anim.GetBool("Grounded") && anim.GetFloat("Speed") > .1f)
            {
                RaycastHit hit;
                Vector3 capsuleCurGround = Vector3.zero;
                Vector3 yAmount = Vector3.zero;
                if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, 2.0f, footRayLayer))
                    capsuleCurGround = hit.point;

                if (Physics.Raycast(transform.position + (((transform.forward * (m_CapCollider.radius))) + (m_CapCollider.attachedRigidbody.velocity * Time.deltaTime)) + Vector3.up * slopeUpRayDist, Vector3.down, out hit, slopeDownRayDist, footRayLayer))
                {
                    yAmount = hit.point - capsuleCurGround;
                }
                if (yAmount.y < slopeDistance && yAmount.y > -0f)
                {
                    m_CapCollider.center = new Vector3(0, Mathf.Lerp(m_CapCollider.center.y, capColDefaultCenter.y + yAmount.y, Time.deltaTime * slopeColliderSmooth), 0);
                    m_CapCollider.height = Mathf.Lerp(m_CapCollider.height, capColDefaultHeight - (yAmount.y / 2), Time.deltaTime * slopeColliderSmooth);
                }
                else
                {
                    m_CapCollider.center = new Vector3(0, Mathf.Lerp(m_CapCollider.center.y, capColDefaultCenter.y, Time.deltaTime * slopeColliderSmooth * 2), 0);
                    m_CapCollider.height = Mathf.Lerp(m_CapCollider.height, capColDefaultHeight, Time.deltaTime * slopeColliderSmooth);
                    Debug.DrawRay(transform.position + (((transform.forward * (m_CapCollider.radius))) + (m_CapCollider.attachedRigidbody.velocity * Time.deltaTime)) + Vector3.up * slopeUpRayDist, Vector3.down * slopeDownRayDist, Color.green);
                }
            }
            else
            {
                m_CapCollider.center = new Vector3(0, Mathf.Lerp(m_CapCollider.center.y, capColDefaultCenter.y, Time.deltaTime * slopeColliderSmooth * 2), 0);
                m_CapCollider.height = Mathf.Lerp(m_CapCollider.height, capColDefaultHeight, Time.deltaTime * slopeColliderSmooth);
            }
        }
    }
    void OnAnimatorIK(int layerNo)
    {
        // Get base layer's stateinfo to determine which state
        AnimatorStateInfo si1 = anim.GetCurrentAnimatorStateInfo(0);

        if (layerNo != 0)
            return;

        // foot IK
        // Get position (ground distance) curves from animation
        float curveL = anim.GetFloat("LFootG"); curveL = curveL > 0 ? curveL : 0;
        float curveR = anim.GetFloat("RFootG"); curveR = curveR > 0 ? curveR : 0;

        //Debug.DrawRay(leftFoot.position + Vector3.up * rayDifToUp, Vector3.down * (rayDistFromFootPosition + rayDifToUp));
        //Debug.DrawRay(rightFoot.position + Vector3.up * rayDifToUp, Vector3.down * (rayDistFromFootPosition + rayDifToUp));

        // foot raycasts
        Ray rayL = new Ray(leftFoot.position + Vector3.up * rayDifToUp, Vector3.down * (rayDistFromFootPosition + rayDifToUp));
        Ray rayR = new Ray(rightFoot.position + Vector3.up * rayDifToUp, Vector3.down * (rayDistFromFootPosition + rayDifToUp));
        RaycastHit hitL, hitR;

        // can we align foot in this state
        bool canAlign = CanAlignFoot(si1);

        // if can't align in this state roll back to normal smoothly
        if (!canAlign)
        {
            ltargAim = 0; rtargAim = 0; ltargAimRot = 0; rtargAimRot = 0;
        }

        // if we can align in this state raycast check for left foot / for positioning
        if (canAlign && Physics.Raycast(rayL, out hitL, rayDistFromFootPosition, footRayLayer))
        {
            ltargPos = new Vector3(leftFoot.position.x, hitL.point.y, leftFoot.position.z) + new Vector3(0, curveL * curveEffector + footGroundFixOffset, 0);
            ltargAim = 1;
            targPosY_L = ltargPos.y;

            // for ratation, we don't need another ray - just see if we approached minimum distance that we can rotate foot for this ground.(rayDistanceFromFootPosition must be bigger than rayDistFromFootRotation)
            if (canAlign && Vector3.Distance(ltargPos, leftFoot.position) < rayDistFromFootRotation)
            {
                ltargRot = Quaternion.FromToRotation(transform.up, hitL.normal) * leftFoot.rotation * Quaternion.Euler(leftFootRotationFix);
                ltargAimRot = 1;
            }
            else
            {
                ltargAimRot = 0;
                ltargRot = leftFoot.rotation;
            }
        }
        else
        {
            ltargAim = 0;
            ltargAimRot = 0;
            ltargPos = leftFoot.position;
            targPosY_L = leftFoot.position.y;
        }

        // if we can align in this state raycast check for right foot / for positioning
        if (canAlign && Physics.Raycast(rayR, out hitR, rayDistFromFootPosition, footRayLayer))
        {
            rtargPos = new Vector3(rightFoot.position.x, hitR.point.y, rightFoot.position.z) + new Vector3(0, curveR * curveEffector + footGroundFixOffset, 0);
            targPosY_R = rtargPos.y;
            rtargAim = 1;

            // for ratation, we don't need another ray - just see if we approached minimum distance that we can rotate foot for this ground.(rayDistanceFromFootPosition must be bigger than rayDistFromFootRotation)
            if (canAlign && Vector3.Distance(rtargPos, rightFoot.position) < rayDistFromFootRotation)
            {
                rtargRot = Quaternion.FromToRotation(transform.up, hitR.normal) * rightFoot.rotation * Quaternion.Euler(rightFootRotationFix);
                rtargAimRot = 1;
            }
            else
            {
                rtargAimRot = 0;
                rtargRot = rightFoot.rotation;
            }
        }
        else
        {
            rtargAim = 0;
            rtargAimRot = 0;
            rtargPos = rightFoot.position;
            targPosY_R = leftFoot.position.y;
        }

        // Calculate parameter for smoothing on ground height changes
        float amount = footHeightChangeSpeed * Mathf.Sin(Time.deltaTime / footHeightLerpPeriod);

        curPosY_L = Mathf.SmoothDamp(curPosY_L, targPosY_L, ref curPosYV_L, amount);
        ltargPos = new Vector3(ltargPos.x, curPosY_L, ltargPos.z);
        curPosY_R = Mathf.SmoothDamp(curPosY_R, targPosY_R, ref curPosYV_R, amount);
        rtargPos = new Vector3(rtargPos.x, curPosY_R, rtargPos.z);

        amount = footRotChangeSpeed * Mathf.Sin(Time.deltaTime / footRotationLerpPeriod);
        lerpRotL = Quaternion.Slerp(lerpRotL, ltargRot, amount);
        lerpRotR = Quaternion.Slerp(lerpRotR, rtargRot, amount);

        // position
        // smooth foot position weight
        lcurAim = Mathf.SmoothDamp(lcurAim, ltargAim, ref laimV, posSmooth);
        rcurAim = Mathf.SmoothDamp(rcurAim, rtargAim, ref raimV, posSmooth);

        anim.SetIKPosition(AvatarIKGoal.LeftFoot, ltargPos);
        anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, lcurAim);

        anim.SetIKPosition(AvatarIKGoal.RightFoot, rtargPos);
        anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, rcurAim);

        // rotation
        // smooth foot rotation weight
        lcurAimRot = Mathf.SmoothDamp(lcurAimRot, ltargAimRot, ref laimVRot, posSmooth);
        rcurAimRot = Mathf.SmoothDamp(rcurAimRot, rtargAimRot, ref raimVRot, posSmooth);

        anim.SetIKRotation(AvatarIKGoal.LeftFoot, lerpRotL);
        anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, lcurAimRot);

        anim.SetIKRotation(AvatarIKGoal.RightFoot, lerpRotR);
        anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, rcurAimRot);

        // Head-Body IK
        // look at camera's forward
        if ((si1.shortNameHash == _sprintState || si1.shortNameHash == _crouchState || si1.shortNameHash == _LocomotionState) && !anim.GetBool("Aim"))
        {
            headAim = Mathf.SmoothDamp(headAim, 1, ref headAimV, 2f);
        }
        else
            headAim = Mathf.SmoothDamp(headAim, 0, ref headAimV, 1f);

        anim.SetLookAtPosition(plAtts.target.position);

        anim.SetLookAtWeight(headAim, bodyWeight, headWeight, 1, clampWeight);

    }
    public float bodyWeight, headWeight, clampWeight;

    private bool CanAlignFoot(AnimatorStateInfo baseStateInfo)
    {
        // foot placing states
        bool canAlign = (baseStateInfo.shortNameHash == _LocomotionState || baseStateInfo.shortNameHash == _sprintState || baseStateInfo.shortNameHash == _crouchState || baseStateInfo.shortNameHash == _coverLocomotionState ||
            baseStateInfo.shortNameHash == _jogPivotLeftState || baseStateInfo.shortNameHash == _jogPivotRightState || baseStateInfo.shortNameHash == _stopLeftState || baseStateInfo.shortNameHash == _stopRightState ||
            baseStateInfo.shortNameHash == _idlePivotLeft || baseStateInfo.shortNameHash == _idlePivotRight
            );
        return canAlign;
    }
    bool canSlope()
    {
        AnimatorStateInfo si0 = anim.GetCurrentAnimatorStateInfo(0);
        bool canSlope = (si0.shortNameHash == _LocomotionState || si0.shortNameHash == _sprintState || si0.shortNameHash == _crouchState
            );
        return canSlope;
    }
}
