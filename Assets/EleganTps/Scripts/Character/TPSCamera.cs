using UnityEngine;
using System.Collections.Generic;

public class TPSCamera : MonoBehaviour
{
    #region Animation Variables
    private readonly int _upPeek = Animator.StringToHash("UpPeek");
    private readonly int _edgePeek = Animator.StringToHash("EdgePeek");
    private readonly int _coverLocomotion = Animator.StringToHash("CoverLocomotion");
    private readonly int _coverEnter = Animator.StringToHash("CoverEnter");
    private readonly int _locomotionState = Animator.StringToHash("Locomotion");
    private readonly int _crouchlocomotionState = Animator.StringToHash("CrouchLocomotion");
    #endregion

    #region General vars
    AnimatorStateInfo stateInfo;
    PlayerAtts plAtts;
    private Animator anim;
    float rotLeftRight, rotUpDown;
    Vector3 currentOffset;   // current Camera Distance From Player
    float currentLeftRightRotation;     // current Camera Rotation  
    float offSetChangeSpeed = 2f;  // used to smoothly change distance from player
    [HideInInspector]
    public Vector3 targetOffset;   // target distance when changing distV
    private SetupAndUserInput user_input;   // controls
    private float zoomLevel = 1;    // used to change fov of camera
    private float zoomLevel_V;
    private Transform playerF;
    [System.NonSerialized]
    public Transform moveReference;
    [System.NonSerialized]
    public int focusedPartHolderIndex;
    private List<Vector3> gunPartPositions;
    #endregion

    #region Behaviour vars
    public Vector3 cameraFocusOffset = Vector3.up;
    public Vector3 offSetNormal = Vector3.zero;
    public Vector3 offSetAim = new Vector3(.58f, 1.56f, -2.21f);
    public Vector3 offSetCoverNormal = new Vector3(.79f, 1.87f, -3.36f);
    public Vector3 offsetEdgePeek = new Vector3(.79f, 1.87f, -3.36f);
    public Vector3 offsetUpPeek = new Vector3(.79f, 1.87f, -3.36f);
    public Vector3 fpsLookCamPlayerOffset = new Vector3(0, 1.5f, 0);

    // x,y Mouse Speed
    public float xSpeed = 250;
    public float ySpeed = 120;

    // Default rotation to set on start and on camera reset to player's back
    public float defaultLeftRightRot = 0;
    public float defaultUpDownRot = 10;

    // Clamping mouse rotations
    public float rotVerticalUpLimit = 30;
    public float rotVerticalDownLimit = 55;
    public float rotVerticalUpLimitAiming = 30;
    public float rotVerticalDownLimitAiming = 55;
    public float rotVerticalUpLimitFpsLook = 30;
    public float rotVerticalDownLimitFpsLook = 55;
    public float rotHorizontalLimitFpsLook = 55;

    // Right Click Zoom Vars
    public float zoomFov = 24;
    public float zoomSpeedInverse = .15f;
    private float targetFov, defFov;

    public int camMode = 0; // 0 = normal mode,  1 = looking at gun 
    public Vector3 lookAtWeaponPos = new Vector3(.06f, 1.57f, .41f); // fixed distance from player's position when looking at weapon
    public float lookRotAngle = 3f;
    public bool moving = false;
    public float changePosSpeed = 7f;
    public float changeRotSpeed = 11f;
    public float fovOnModifyMode = 40f;
    public float resetToBehindSmooth = 5f;
    private float fovOnModifyMode_V;

    // camera Wall detect vars
    public float onWallDetectMoveAmount = 0;
    public float onWallHitFollowTargetY = 3;
    public LayerMask wallDetectLayerMask;
    public float smoothPosOnWallDetect = 2f;
    private CapsuleCollider plCapCollider;

    // smoothing vars
    public float smoothPosition = 5f;
    public float smoothR = 5f;
    private Vector3 posSmoothVelocity;

    // Shake vars
    public bool useShakeOnSprint = true;
    public Vector3 maxShakeRotation = new Vector3(0, 0, 15);
    public float shakeSmooth = 5f;
    private Vector3 curShakeRot;
    private float shakeMultiplier = 0;
    public float fovChangeAmount = 10f;
    private float fovOnSprint;
    #endregion

    void Start()
    {
        anim = GameObject.FindGameObjectWithTag("Player").GetComponent<Animator>();
        plAtts = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerAtts>();
        plCapCollider = plAtts.GetComponent<CapsuleCollider>();
        user_input = anim.GetComponent<SetupAndUserInput>();
        Vector3 angles = transform.eulerAngles;
        rotLeftRight = angles.y;
        rotUpDown = angles.x;
        targetFov = Camera.main.fieldOfView;
        defFov = targetFov;
        currentOffset = offSetNormal;
        moveReference = transform.parent.FindChild("MoveReference");
        rotLeftRight = defaultUpDownRot; currentLeftRightRotation = defaultUpDownRot; rotUpDown = defaultUpDownRot;
        if (!playerF)
            playerF = GameObject.FindGameObjectWithTag("Player").transform;

        transform.position = plAtts.transform.position;
    }

    void LateUpdate()
    {
        stateInfo = anim.GetCurrentAnimatorStateInfo(0);

        #region LookAtGunMode
        Vector3 fixedPosLookAtGun = playerF.position + playerF.forward * lookAtWeaponPos.z + playerF.right * lookAtWeaponPos.x + playerF.up * lookAtWeaponPos.y;
        //Debug.DrawRay(fixedPosLookAtGun, Vector3.up * .1f, Color.red);
        if (moving && camMode == 1)
        {
            if (camMode == 1)   // moving to look at gun
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(fixedPosLookAtGun - (playerF.position + Vector3.up * lookRotAngle)), Time.deltaTime * changeRotSpeed);
                transform.position = Vector3.Lerp(transform.position, fixedPosLookAtGun, Time.deltaTime * changePosSpeed);

                if (Vector3.Distance(transform.position, fixedPosLookAtGun) < .01f)    // move fw to look at gun
                    moving = false;
            }
            else if (camMode == 0)  // moving to tps mode
                moving = false;
            return;
        }
        else if (camMode == 1) // looking at gun mode
        {
            gunPartPositions = plAtts.cGun.GetComponent<WeaponParts>().GetGunPartPositions();
            int partCount = gunPartPositions.Count;
            if (user_input.MenuRightDown)
            {
                focusedPartHolderIndex++;
                focusedPartHolderIndex = focusedPartHolderIndex >= partCount ? 0 : focusedPartHolderIndex;
                user_input.MenuRightDown = false;
                if (plAtts.changeFocusedPartHolderSoundPrefab)
                    Instantiate(plAtts.changeFocusedPartHolderSoundPrefab, plAtts.cGun.position, Quaternion.identity);
            }
            else if (user_input.MenuLeftDown)
            {
                focusedPartHolderIndex--;
                focusedPartHolderIndex = focusedPartHolderIndex < 0 ? partCount - 1 : focusedPartHolderIndex;
                user_input.MenuLeftDown = false;
                if (plAtts.changeFocusedPartHolderSoundPrefab)
                    Instantiate(plAtts.changeFocusedPartHolderSoundPrefab, plAtts.cGun.position, Quaternion.identity);
            }

            Vector3 focusedPartPos = gunPartPositions[focusedPartHolderIndex];
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(-fixedPosLookAtGun + focusedPartPos), Time.deltaTime * changeRotSpeed);
            transform.position = Vector3.Lerp(transform.position, fixedPosLookAtGun, Time.deltaTime * changePosSpeed);
            GetComponent<Camera>().fieldOfView = Mathf.SmoothDamp(GetComponent<Camera>().fieldOfView, fovOnModifyMode, ref fovOnModifyMode_V, 20f * Time.deltaTime);
            if (user_input.ChangeGunPartsDown || !(stateInfo.shortNameHash == _locomotionState/* || stateInfo.shortNameHash == _crouchlocomotionState*/))
            {
                camMode = 0;
                anim.SetBool("LookAtWeapon", false);
                moving = true;
                gunPartPositions.Clear();
                user_input.movementInputEnabled = true;
            }
            return;
        }
        #endregion

        #region TpsCam
        else if (camMode == 0) // tps cam mode
        {
            GetMouse();
            GetTargetOffsetAndClampedRotation();
            // Clamp UpDown Camera movement
            rotUpDown = !plAtts.isAiming ? ClampRotation(rotUpDown, -rotVerticalUpLimit, rotVerticalDownLimit) : rotUpDown = ClampRotation(rotUpDown, -rotVerticalUpLimitAiming, rotVerticalDownLimitAiming);
            ClampRotation(rotLeftRight, 0, 360);
            // lerp Offset to make sure no sudden changes or flickering
            currentOffset = Vector3.Lerp(currentOffset, targetOffset, Time.deltaTime * offSetChangeSpeed);
            // lerp Left Right Rotation to make sure no sudden changes or flickering
            currentLeftRightRotation = Mathf.Lerp(currentLeftRightRotation, rotLeftRight, Time.deltaTime * 20f);
            // Shake Camera 
            if (useShakeOnSprint) ShakeCamera();

            // Set rot-pos to lerp to
            Quaternion rotation = Quaternion.Euler(rotUpDown + curShakeRot.x, currentLeftRightRotation + curShakeRot.y, curShakeRot.z);
            Vector3 position = rotation * currentOffset + (playerF.position + cameraFocusOffset);

            // wall detect if there is
            bool wallDetect = false;
            position = CheckForWalls(position, ref wallDetect);
            transform.position = Vector3.SmoothDamp(transform.position, position, ref posSmoothVelocity, smoothPosition * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, smoothR * Time.deltaTime);

            // change move reference to control character based on camera position without distance LeftRight from player
            moveReference.position = transform.position - new Vector3(currentOffset.x, 0, 0);
            moveReference.rotation = transform.rotation;

            // fov change based on right click
            float zoomFovToSet = zoomFov;
            ChangeFov(ref zoomFovToSet);

            // Reset camera to player's back(start position)
            if (user_input.CameraResetDown && !plAtts.isAiming && plAtts.controlMode == ControlMode.Free && (stateInfo.shortNameHash == _locomotionState || stateInfo.shortNameHash == _crouchlocomotionState))
            {
                moving = true;
                camMode = 2;
                currentLeftRightRotation = transform.rotation.eulerAngles.y;
                //user_input.InputEnabled = false;
            }
            // First person look camera
            else if (user_input.FirstPersonLookDown && !plAtts.isAiming && (stateInfo.shortNameHash == _locomotionState || stateInfo.shortNameHash == _crouchlocomotionState))
            {
                moving = true;
                camMode = 3;
                user_input.movementInputEnabled = false;
            }
        }
        #endregion

        #region MoveBehindCam
        else if (camMode == 2) // moving To Back Of Target
        {
            currentLeftRightRotation = Mathf.Lerp(currentLeftRightRotation, playerF.rotation.eulerAngles.y, Time.deltaTime * resetToBehindSmooth);
            rotUpDown = Mathf.Lerp(rotUpDown, 10, Time.deltaTime * resetToBehindSmooth);

            Quaternion rotation = Quaternion.Euler(rotUpDown, currentLeftRightRotation, 0);
            Vector3 position = rotation * currentOffset + (playerF.position + cameraFocusOffset);

            // wall detect if there is
            bool wallDetect = false;
            position = CheckForWalls(position, ref wallDetect);
            transform.position = Vector3.SmoothDamp(transform.position, position, ref posSmoothVelocity, smoothPosition * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, smoothR * Time.deltaTime);

            if (Mathf.Abs(currentLeftRightRotation - (defaultLeftRightRot + playerF.rotation.eulerAngles.y)) < 4f)
            {
                moving = false;
                camMode = 0;
                rotLeftRight = currentLeftRightRotation;
            }
        }
        #endregion

        #region First Person Look Cam
        else if (camMode == 3)
        {
            if (moving)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, Time.deltaTime * 55f);
                transform.position = Vector3.Lerp(transform.position, playerF.TransformPoint(fpsLookCamPlayerOffset), Time.deltaTime * 10);
                rotUpDown = 0;
                rotLeftRight = 0;
                if (Vector3.Distance(transform.position, playerF.TransformPoint(fpsLookCamPlayerOffset)) < .009f)
                {
                    moving = false;
                    rotUpDown = 0;
                    rotLeftRight = 0;
                }
            }
            else
            {
                if (user_input.FirstPersonLookDown)
                {
                    moving = false;
                    camMode = 0;
                    user_input.movementInputEnabled = true;
                    rotLeftRight = 0;
                    rotUpDown = 0;
                }

                GetMouse();
                rotUpDown = ClampRotation(rotUpDown, -rotVerticalUpLimitFpsLook, rotVerticalDownLimitFpsLook);
                rotLeftRight = ClampRotation(rotLeftRight, rotHorizontalLimitFpsLook, -rotHorizontalLimitFpsLook);
                transform.rotation = Quaternion.Euler(rotUpDown, rotLeftRight, 0);
            }
        }
        #endregion
    }
    private void ShakeCamera()
    {
        float shake = 0;
        if (stateInfo.IsName("Sprint"))
        {
            shake = anim.GetFloat("Shake");
            shakeMultiplier = Mathf.Lerp(shakeMultiplier, 1, Time.deltaTime * .5f);
            fovOnSprint = Mathf.Lerp(fovOnSprint, fovChangeAmount, Time.deltaTime * 4f);
        }
        else
        {
            shakeMultiplier = Mathf.Lerp(shakeMultiplier, 0, Time.deltaTime * 1f);
            fovOnSprint = Mathf.Lerp(fovOnSprint, 0, Time.deltaTime * 4f);
        }
        curShakeRot = Vector3.Lerp(Vector3.zero, maxShakeRotation * shake * shakeMultiplier, Time.deltaTime * shakeSmooth);
    }
    private void GetTargetOffsetAndClampedRotation()
    {
        if (stateInfo.shortNameHash == _coverEnter || stateInfo.shortNameHash == _coverLocomotion)
        {
            targetOffset = new Vector3(offSetCoverNormal.x * -anim.GetFloat("LookLeftRight"), offSetCoverNormal.y, offSetCoverNormal.z);

        }
        else if (stateInfo.shortNameHash == _edgePeek)
        {
            targetOffset = new Vector3(offsetEdgePeek.x * -anim.GetFloat("LookLeftRight"), offsetEdgePeek.y, offsetEdgePeek.z);
        }
        else if (stateInfo.shortNameHash == _upPeek)
        {
            targetOffset = new Vector3(offsetUpPeek.x * -anim.GetFloat("LookLeftRight"), offsetUpPeek.y, offsetUpPeek.z);
        }
        else  // general states
        {
            if (plAtts.cGun && (user_input.Fire2Press || plAtts.debugFire2Press))
                targetOffset = offSetAim;
            else
                targetOffset = offSetNormal;

        }
        currentOffset = Vector3.Lerp(currentOffset, targetOffset, Time.deltaTime * offSetChangeSpeed);
    }
    private void ChangeFov(ref float zoomFovToSet)
    {
        if (plAtts.cGun && (user_input.Fire2Press || plAtts.debugFire2Press))
        {
            zoomLevel = Mathf.SmoothDamp(zoomLevel, 0, ref zoomLevel_V, zoomSpeedInverse);

            if (plAtts.cGun && plAtts.cGun.GetComponent<GunAtt>().sight)
            {
                zoomFovToSet = zoomFov - plAtts.cGun.GetComponent<GunAtt>().sight.GetComponent<SightGP>().cameraZoomAmount;
            }
        }
        else
        {
            zoomLevel = Mathf.SmoothDamp(zoomLevel, 1, ref zoomLevel_V, zoomSpeedInverse);
        }
        Camera.main.fieldOfView = Mathf.Lerp(zoomFovToSet, defFov - fovOnSprint, zoomLevel);

    }
    private float ClampRotation(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
    private Vector3 CheckForWalls(Vector3 position, ref bool wallDetect)
    {
        Vector3 upDist = new Vector3(0, plCapCollider.height * onWallHitFollowTargetY / 4, 0);
        Vector3 plPos = playerF.position + upDist + (position - playerF.position).normalized * plCapCollider.radius / 2f;
        Vector3 camPos = position;
        RaycastHit wallHit = new RaycastHit();
        if (Physics.Linecast(plPos, camPos, out wallHit) /*&& !anim.GetBool("Aim")*/)
        {
            if (wallHit.transform.tag != "Player")
                return position = new Vector3(wallHit.point.x, position.y, wallHit.point.z) + (transform.position - playerF.position).normalized * onWallDetectMoveAmount;
        }
        return position;
    }
    private void GetMouse()
    {
        // get mouse movement
        rotLeftRight += user_input.MouseX * xSpeed * 0.02f;
        rotUpDown -= user_input.MouseY * ySpeed * 0.02f;
    }
}

