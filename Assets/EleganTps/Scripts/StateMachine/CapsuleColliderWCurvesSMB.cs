using UnityEngine;
using System.Collections;

public class CapsuleColliderWCurvesSMB : CustomSMB
{
    private readonly int _capsizePar = Animator.StringToHash("CapSize");
    CapsuleCollider capCollider;
    private float capHeight;
    private float capY;

    public override void Init(Animator anim)
    {
        capCollider = userInput.GetComponent<CapsuleCollider>();
        capHeight = capCollider.height;
        capY = capCollider.center.y;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        float size = animator.GetFloat(_capsizePar);
        float newCenterY = .3f;
        float lerpCenter = Mathf.Lerp(capY, newCenterY, size);
        capCollider.center = new Vector3(capCollider.center.x, lerpCenter, capCollider.center.z);
        capCollider.height = Mathf.Lerp(capHeight, .5f, size);
    }

}
