using UnityEngine;
using System.Collections;

public class GrabSMB : CustomSMB
{
    PlayerAtts plAtts;
    Transform playerT;
    //float defAnimatorSpeed;
    float layerWeight, velocity;

    public override void Init(Animator anim)
    {
        plAtts = userInput.GetComponent<PlayerAtts>();
        playerT = userInput.transform;
        //defAnimatorSpeed = plAtts.GetComponent<Animator>().speed;
    }


    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //defAnimatorSpeed = animator.speed;

    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // you can uncomment Lines to use slow motion grab (animation curves required)
        //animator.speed = defAnimatorSpeed + defAnimatorSpeed *( animator.GetFloat("TimeScaleCurve") *.5f);

        // player turns to item(movement is disabled until player exits from grab animation)
        Vector3 targetFw = new Vector3(plAtts.itemFocusPos.x, plAtts.transform.position.y, plAtts.itemFocusPos.z) - plAtts.transform.position;
        playerT.forward = Vector3.Lerp(playerT.forward, targetFw, Time.deltaTime * 15f);
    }



    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetInteger("Grab", 0);
        userInput.movementInputEnabled = true;
        //animator.speed = defAnimatorSpeed;

        animator.SetLayerWeight(4, 0);
    }

    public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        AnimatorStateInfo _sI4Next = animator.GetNextAnimatorStateInfo(4);
        if (_sI4Next.IsName("Empty"))
            animator.SetLayerWeight(4, (Mathf.Lerp(animator.GetLayerWeight(4), 0, Time.deltaTime * .7f)));
        else
            animator.SetLayerWeight(4, (Mathf.Lerp(animator.GetLayerWeight(4), 1, Time.deltaTime * .7f)));
    }

}
