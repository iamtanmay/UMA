using UnityEngine;
using System.Collections;

public class CustomSMB : StateMachineBehaviour
{
    [HideInInspector]
    public SetupAndUserInput userInput;   // For referencing input from a MonoBehaviour

    public virtual void Init(Animator anim)
    { }

}

