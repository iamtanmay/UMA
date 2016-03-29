using UnityEngine;
using System.Collections;

public class MeleeControlSMB : CustomSMB
{

    private readonly int _speedPar = Animator.StringToHash("Speed");

    private Transform playerT;
    private PlayerAtts plAtts;
    private float yatay;
    private float dikey;
    private Rigidbody rb;

    public override void Init(Animator anim)
    {
        playerT = userInput.transform;
        rb = userInput.GetComponent<Rigidbody>();
        plAtts = userInput.GetComponent<PlayerAtts>();
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        yatay = userInput.Horizontal;
        dikey = userInput.Vertical;
        animator.SetFloat(_speedPar, Vector2.SqrMagnitude(new Vector2(yatay, dikey)), .25f, Time.deltaTime);

        Vector3 moveDirection = Vector3.zero;
        float targetAngle = 0f;
        userInput.CalculateRefs(ref moveDirection, ref targetAngle);

        float angleSmooth = Mathf.Lerp(0, targetAngle, moveDirection.magnitude * Time.deltaTime * 1);

        if (moveDirection != Vector3.zero)
            rb.AddTorque(playerT.up * angleSmooth, ForceMode.VelocityChange);

        if (plAtts.isAiming && plAtts.target && !plAtts.turningWithGun)
        {
            Vector3 defTargetPosition = userInput.mainCamera.TransformPoint(0, 0, plAtts.target.localPosition.z);
            playerT.transform.LookAt(new Vector3(defTargetPosition.x, userInput.transform.position.y, defTargetPosition.z));
        }

        Vector3 directionMultiplied = new Vector3(moveDirection.x * plAtts.meleeControlX, 0, moveDirection.z * plAtts.meleeControlZ);

        playerT.Translate(directionMultiplied * Time.deltaTime);

    }
}
