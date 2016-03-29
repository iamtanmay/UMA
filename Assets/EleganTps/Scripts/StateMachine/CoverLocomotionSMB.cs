using UnityEngine;
using System.Collections;

public class CoverLocomotionSMB : CustomSMB
{
    public float velXDamp = .2f; public float velXDampStop = .03f;
    public float speedPos = 5;
    public float speedRot = 15;
    public float stickForce = .5f;

    private float horizontal;
    CoverVars cv;
    PlayerAtts plAtts;

    Transform playerT;
    Rigidbody rb;
    public override void Init(Animator anim)
    {
        plAtts = userInput.GetComponent<PlayerAtts>();
        playerT = plAtts.transform;
        rb = plAtts.GetComponent<Rigidbody>();
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        cv = plAtts.coverVars;
        //cv.lookTarget = animator.GetBool("IsLeft") ? 1 : -1;
        cv.canEdgePeek = false;
        animator.SetFloat("Speed", 0);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (userInput.CoverDown)
        {
            animator.SetBool("Cover", false);
        }
        horizontal = userInput.Horizontal;
        float maxSpeed = 0; bool isThereANormal = false;
        // Check player's behind and find normal to rotate player
        if (!animator.IsInTransition(0))
            isThereANormal = cv.CheckWallAtPosition(playerT.position, ref cv.coverNormal, cv.coverNormal, 0, ref cv.crouchStand);
        if ((!isThereANormal && !animator.IsInTransition(0)) || !animator.GetBool("Cover"))
        {
            animator.SetBool("Cover", false);
            return;
        }

        Vector3 vec = Vector3.zero; float crst = 0;
        bool isWallExistNext = cv.CheckWallAtPosition(playerT.position, ref vec, cv.coverNormal, cv.movingNormalCheckOffset * -cv.lookTarget/*(absHorizontal==0?0:Mathf.Sign(horizontal))*/, ref crst);

        if (isWallExistNext)
        {
            animator.SetFloat("VelX", -horizontal, velXDamp, Time.deltaTime);

            float acceleration = Mathf.Lerp(cv.crouchWalkAccelerate, cv.standWalkAccelerate, animator.GetFloat("CrouchStand"));
            maxSpeed = Mathf.Lerp(cv.crouchMaxSpeed, cv.standMaxSpeed, animator.GetFloat("CrouchStand"));
            if ((animator.GetFloat("LookLeftRight") > .9f || animator.GetFloat("LookLeftRight") < -.9f))
                rb.AddRelativeForce(Vector3.forward * -horizontal * animator.GetFloat("LookLeftRight") * acceleration, ForceMode.VelocityChange);

            cv.canUpPeak = animator.GetFloat("CrouchStand") < cv.crouchUpPeakLimit;
            cv.canEdgePeek = false; // if character is not in edge of cover he cant edge peek
        }
        else
        {
            animator.SetFloat("VelX", 0, velXDampStop, Time.deltaTime);
            cv.canEdgePeek = true;
            cv.canUpPeak = animator.GetFloat("CrouchStand") < cv.crouchUpPeakLimit;
        }

        #region angle & peeking
        Vector3 relationalDir = Quaternion.Euler(0, 90, 0) * (new Vector3(plAtts.target.position.x, playerT.position.y, plAtts.target.position.z) - playerT.position);
        cv.angle = Vector3.Angle(playerT.right, relationalDir);
        cv.dot = -Mathf.Sign(Vector3.Dot(playerT.right, Quaternion.Euler(0, -90, 0) * relationalDir));

        if (cv.canEdgePeek && cv.lookTarget < 0)
        {
            if (cv.dot > 0)
            {
                if (cv.angle > 90 + cv.angleOffsetEdgePeek && cv.angle < 180 && cv.canUpPeak)
                {
                    // go up peek
                    cv.canUpPeak = true;
                    cv.canEdgePeek = false;
                }
                else
                {
                    cv.canEdgePeek = true;
                    // keep edge peek
                }
            }
            else
            {
                // keep edge peek
                cv.canEdgePeek = true;
                cv.canUpPeak = false;
            }
        }
        else if (cv.canEdgePeek && cv.lookTarget > 0)
        {
            if (cv.dot < 0)
            {
                if (cv.angle > 90 + cv.angleOffsetEdgePeek && cv.angle < 180 && cv.canUpPeak)
                {
                    cv.canUpPeak = true;
                    cv.canEdgePeek = false;
                }
                else
                {
                    cv.canEdgePeek = true;
                }
            }
            else
            {
                cv.canEdgePeek = true;
                cv.canUpPeak = false;
            }
        }

        if (plAtts.cGun)
        {
            if (userInput.Fire2Press || plAtts.debugFire2Press)
            {
                if (cv.canEdgePeek && cv.canUpPeak)
                {
                    animator.SetBool("EdgePeek", true);
                    animator.SetBool("UpPeek", false);
                }
                else if (cv.canEdgePeek)
                {
                    animator.SetBool("EdgePeek", true);
                    animator.SetBool("UpPeek", false);
                }
                else if (cv.canUpPeak)
                {
                    animator.SetBool("EdgePeek", false);
                    animator.SetBool("UpPeek", true);
                }
            }
            else
            {
                animator.SetBool("EdgePeek", false);
                animator.SetBool("UpPeek", false);
            }
        }

        #endregion

        #region Limit Velocity
        // Limit max velocity for proper movement
        Vector2 velocityHorizontal = new Vector2(rb.velocity.x, rb.velocity.z);
        if (velocityHorizontal.magnitude > maxSpeed)
        {
            velocityHorizontal.Normalize();
            velocityHorizontal *= maxSpeed;
        }
        rb.velocity = new Vector3(velocityHorizontal.x, rb.velocity.y, velocityHorizontal.y);
        #endregion        

        #region Looking Left or Right & Stand amount
        if (horizontal > .1f)
            cv.lookTarget = -1;
        else if (horizontal < -.1f)
            cv.lookTarget = 1;
        animator.SetFloat("LookLeftRight", Mathf.Lerp(animator.GetFloat("LookLeftRight"), cv.lookTarget, Time.deltaTime * 7f));
        animator.SetFloat("CrouchStand", Mathf.Lerp(animator.GetFloat("CrouchStand"), cv.crouchStand, Time.deltaTime * 2.2f));
        #endregion

        #region Stick To Wall
        // stick to wall behind with a tiny amount of force (make sure that cover edges are sharp)
        Debug.DrawRay(playerT.position, playerT.right * cv.lookTarget);

        if (isThereANormal && !animator.IsInTransition(0) && (animator.GetFloat("LookLeftRight") > .9f || animator.GetFloat("LookLeftRight") < -.9f))
            rb.AddForce(playerT.right * animator.GetFloat("LookLeftRight") * stickForce, ForceMode.VelocityChange);
        Vector3 fw = Quaternion.Euler(0, 90 * animator.GetFloat("LookLeftRight"), 0) * cv.coverNormal;
        fw = new Vector3(fw.x, 0, fw.z);
        playerT.forward = Vector3.Lerp(playerT.forward, fw, Time.deltaTime * speedRot);

        #endregion


    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        cv.coverPoint = playerT.position;

    }

}
