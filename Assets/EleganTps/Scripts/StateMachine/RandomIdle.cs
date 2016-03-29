using UnityEngine;
using System.Collections;

public class RandomIdle : CustomSMB
{

    public int numberOfStates = 4;          // The number of random states to choose between.
    private readonly int m_HashRandomIdlePara = Animator.StringToHash("RandomIdle");    // For referencing the RandomIdle animator parameter.


    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // exit from random state if a button is pressed
        if (Time.time - userInput.LastInputAt < .3f || Mathf.Abs(Input.GetAxis("Mouse X")) > .001 || Mathf.Abs(Input.GetAxis("Mouse Y")) > .001)
            animator.SetBool("IdleBool", false);

        // Set RandomIdle based on state count
        if (!animator.IsInTransition(0))
        {
            int randomSelection = Random.Range(0, numberOfStates);
            animator.SetInteger(m_HashRandomIdlePara, randomSelection);
        }
    }
}
