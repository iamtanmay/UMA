using UnityEngine;
using System.Collections;

public class CoverTrigger : CustomSMB
{
    public float maxDistToSphereCast = 5;
    public LayerMask lineCastMask;

    BezierSpline curSpline;
    PlayerAtts plAtts;
    Transform playerT;
    float nearStep;
    bool isNormalFound;
    Vector3 coverEnterNormal;
    float crouchStand;
    Vector3 coverEnterPos;

    public override void Init(Animator anim)
    {
        plAtts = userInput.GetComponent<PlayerAtts>();
        playerT = plAtts.transform;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            curSpline = null;
            float lessDist = 999;
            RaycastHit[] hits = Physics.SphereCastAll(playerT.position, 180, playerT.forward, maxDistToSphereCast);
            if (hits.Length > 0)
            {
                foreach (RaycastHit hit1 in hits)
                {
                    if (!hit1.transform.CompareTag("CoverPosition"))
                        continue;
                    else
                    {
                        float thisStep = 0;
                        BezierSpline spline = hit1.transform.GetComponent<BezierSpline>();
                        float thisDist = spline.GetClosePointToPlayer(ref thisStep);

                        if (thisDist < lessDist)
                        {
                            curSpline = hit1.transform.GetComponent<BezierSpline>();
                            nearStep = thisStep;
                            lessDist = thisDist;
                        }
                        //animator.SetBool("Test", true);
                    }
                }
            }

            Vector3 coverPoint = curSpline.GetPoint(nearStep);

            CheckWallVertical(coverPoint);



            if (curSpline && !Physics.Linecast(coverPoint + Vector3.up, playerT.position + Vector3.up, lineCastMask) && Vector3.Distance(playerT.position, coverPoint) < 1.5f)
            {
                if (!isNormalFound)
                {
                    Debug.Log("Normal cant be found,try changing wall height(must be bigger than characterheight/2)");
                    return;
                }
                isNormalFound = false;
                plAtts.coverVars.coverNormal = coverEnterNormal;
                animator.SetBool("IsLeft", nearStep > .5f ? false : true);
                float dot = Vector3.Dot(playerT.forward, Quaternion.Euler(0, 90, 0) * coverEnterNormal);

                if (dot > 0)
                    animator.SetFloat("LookLeftRight", 1);
                else
                    animator.SetFloat("LookLeftRight", -1);
                plAtts.coverVars.lookTarget = dot > 0 ? 1 : -1;

                animator.SetFloat("CrouchStand", crouchStand);
                plAtts.coverVars.crouchStand = crouchStand;
                animator.SetBool("Cover", true);
                plAtts.coverVars.curSpline = curSpline;
                plAtts.coverVars.nearStep = nearStep;
                plAtts.coverVars.coverPosition = coverEnterPos;
                plAtts.coverVars.coverPoint = coverPoint;

            }
        }
    }


    void CheckWallVertical(Vector3 coverPoint)
    {
        RaycastHit hit;
        Ray ray = new Ray(coverPoint + Vector3.up * .2f, Vector3.down);
        if (Physics.Raycast(ray, out hit, 2f))
        {
            coverPoint = hit.point;
            coverEnterPos = hit.point; // coverEnterPosition's y found here // x-y later
        }
        else
        {
            Debug.Log("Make sure spline curve's points are above and close to ground");
            return;
        }

        // Find cover enter position
        crouchStand = 0;
        ray.origin = coverPoint;
        Vector3 rayDir = Quaternion.Euler(0, 90, 0) * -curSpline.GetDirection(nearStep);

        float characterHeight = 1.8f;
        int rayCount = 4;/*-1*/ float rayOffset = characterHeight / 2;
        int highHitIndex = 0;
        ray.direction = rayDir;

        for (int i = 0; i < rayCount + 1; i++)
        {
            rayOffset = (characterHeight / 2) + (((characterHeight / 2) / (rayCount))) * i;
            ray.origin = coverPoint + Vector3.up * rayOffset;
            if (Physics.Raycast(ray, out hit, 3f))
            {
                highHitIndex = i;

                if (!isNormalFound)
                {
                    coverEnterNormal = hit.normal;
                    isNormalFound = true;
                    coverEnterPos = new Vector3(hit.point.x, coverEnterPos.y, hit.point.z);
                }
            }
        }

        // 0:statnd 1:crouch  //
        crouchStand = (1 / (float)(rayCount)) * (highHitIndex);

    }


}
