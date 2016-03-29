using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TargetLogic : MonoBehaviour
{
    public float _distanceError = 1f; // If you modify TpsCamera's offSetAim's z value, you may encounter firing wall behind glitch-you can fix it by changing this
    [System.NonSerialized]
    public Vector3 targetLocalPos = Vector3.zero;
    private PlayerAtts plAtts;
    private SetupAndUserInput userInput;
    private GunAtt gunAtt;
    private Vector3 defLocalPos;
    private Transform fireRef;
    private Transform camTransform;
    public LayerMask fireRefRaycastLayerMask;
    TPSCamera tpsCam;
    Transform head;
    [System.NonSerialized]
    public bool isHit = false;
    [System.NonSerialized]
    public float fixOffset;

    void Start()
    {
        plAtts = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerAtts>();
        userInput = plAtts.GetComponent<SetupAndUserInput>();
        defLocalPos = transform.localPosition;
        fireRef = transform.FindChild("FireReference");
        camTransform = userInput.mainCamera.transform;
        head = userInput.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head);
        tpsCam = camTransform.GetComponent<TPSCamera>();
    }

    void Update()
    {


        if (plAtts.isAiming && plAtts.cGun != null)
        {
            gunAtt = plAtts.cGun.GetComponent<GunAtt>();
            transform.localPosition = Vector3.Lerp(transform.localPosition, defLocalPos, gunAtt.spreadRecoverSpeed);
        }

        if (plAtts.cGun != null)
            FixFireReferencePosition();
    }

    void FixFireReferencePosition()
    {
        // Find bullet raycast start position based on camera position
        // v1
        Vector3 camPosWoOffset = camTransform.position - camTransform.right * tpsCam.offSetAim.x;
        // v2
        Vector3 plHeadPos = head.position;
        //// v3
        //Vector3 camBotPos = new Vector3(camPosWoOffset.x, plHeadPos.y, camPosWoOffset.z);
        //Debug.DrawLine(camPosWoOffset, plHeadPos);
        //Debug.DrawLine(plHeadPos, camBotPos);
        //Debug.DrawLine(camBotPos, camPosWoOffset);

        fixOffset = Vector3.Distance(camPosWoOffset, plHeadPos);

        RaycastHit hit;
        if (Physics.Raycast(camTransform.position + camTransform.forward * fixOffset, (-camTransform.position + transform.position).normalized, out hit, 99999, fireRefRaycastLayerMask))
        {
            fireRef.position = hit.point;
            isHit = true;
        }
        else
        {
            fireRef.position = camTransform.position + camTransform.forward * 500f;
            isHit = false;
        }


    }
}
