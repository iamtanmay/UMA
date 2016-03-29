using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum ControlMode
{
    Free,
    Static
}
[System.Serializable]
public class FootStepSounds
{
    [HideInInspector]
    public Animator m_Animator = null;
    //private FootPlanting m_FootPlanting = null;
    public float m_BackFootOffset = -0.0275f;
    public float m_FrontFootOffset = +0.200f;

    // play steps sound
    public System.Action OnLeftFootPlantAction = null;
    public System.Action OnRightFootPlantAction = null;

    public bool useFootStepSound = true;
    [HideInInspector]
    public bool useBodyPositionDamping;
    public LayerMask footFallSoundsLayerMask;
    // The different surfaces and their sounds.
    public AudioSurface[] surfaces;
}
[System.Serializable]
public class CoverVars
{
    [System.NonSerialized]
    public float nearStep;
    [System.NonSerialized]
    public BezierSpline curSpline;
    [System.NonSerialized]
    public Vector3 coverNormal;
    [System.NonSerialized]
    public float crouchStand;
    [System.NonSerialized]
    public Animator animator;
    [System.NonSerialized]
    public Vector3 coverPoint;
    [System.NonSerialized]
    public Vector3 coverPosition;
    [System.NonSerialized]
    public Transform playerT;
    [System.NonSerialized]
    public float lookTarget;
    [System.NonSerialized]
    public float dot;
    [System.NonSerialized]
    public bool canEdgePeek;
    [System.NonSerialized]
    public bool canUpPeak;
    [System.NonSerialized]
    public float angle;
    [System.NonSerialized]
    public bool isNormalFound;
    [System.NonSerialized]
    public float angleOffsetEdgePeek = 30;
    [Range(0, 1)]
    public float crouchUpPeakLimit = .7f;

    public float wallOffset;
    public float stopOffsetCheck;
    public float movingNormalCheckOffset; // movingOffsetCheck must be lower than stopOffsetCheck
    public float standWalkAccelerate = 2f;
    public float crouchWalkAccelerate = 1f;
    public float standMaxSpeed = 5;
    public float crouchMaxSpeed = 4;

    public CoverVars(Animator anim)
    {
        isNormalFound = false;
        this.animator = anim;
    }

    public bool CheckWallAtPosition(Vector3 point, ref Vector3 foundNormal, Vector3 lastNormal, float offset, ref float standAmount)
    {
        point = point + Quaternion.Euler(0, -90, 0) * lastNormal * offset;

        Ray ray = new Ray(); RaycastHit hit;
        standAmount = 0;
        ray.origin = point;
        // get direction for wall calculation
        Vector3 rayDir = -lastNormal;

        Debug.DrawRay(point, rayDir, Color.black);
        float characterHeight = 1.8f;
        int rayCount = 8;/*-1*/ float rayOffset = characterHeight / 2;
        int highHitIndex = 0;
        ray.direction = rayDir;
        bool normalFound = false;
        // send rays and get new normal and wall's height
        int hitcount = 0;
        for (int i = 0; i < rayCount + 1; i++)
        {
            rayOffset = (characterHeight / 2) + (((characterHeight / 2) / (rayCount))) * i;
            ray.origin = point + Vector3.up * rayOffset;
            if (Physics.Raycast(ray, out hit, 1f))
            {
                if (Mathf.Abs(offset) > 0)
                    Debug.DrawRay(ray.origin, ray.direction, Color.red);
                else
                    Debug.DrawRay(ray.origin, ray.direction, Color.black);
                highHitIndex = i;

                if (!normalFound)
                {
                    normalFound = true;
                    foundNormal = hit.normal;
                }

                hitcount++;
            }
        }
        if (normalFound)
            standAmount = (1 / (float)(rayCount)) * (highHitIndex);
        return normalFound;
    }
}
public class PlayerAtts : MonoBehaviour
{
    public CoverVars coverVars;

    #region Player Attributes
    public ControlMode controlMode = ControlMode.Free;
    public bool useReachToGrab = true;
    public bool useSuddenStopAnimation = true;
    // Weapon Atts
    public List<GameObject> Guns;
    //public List<Transform> weaponHolders; // not used atm
    public int[] currentMagz = { 100, 100, 100 };   // player's current ammo
    public int[] maxMagz = { 100, 100, 100 };   // max ammo player can carry
    public List<Transform> knownGunParts; // if you want player to learn parts as he picks weapons leave this list empty
    public float idleJumpUpForceUp = 5;   // jump up force when idle
    public float runJumpUpForceUp = 4.9f;    // jump up force when not idle
    public float airControlAmount = 3.5f; // air control speed
    public float airRotationSpeed = 3;
    public float airDownForce = 15f;   // Force applied character to make him fall faster
    public float airDownForceStartVelocity = -.5f;   // Force starts when Y velocity of player reaches this value
    public float meleeControlX = .5f;
    public float meleeControlZ = .1f;
    public float walkSpeedLocomotion = .5f;
    public float coverWalkSpeed = 1f;  // walk speed
    public float coverWalkSpeedCrouch = .8f;  // walk speed
    public float rotateToTargetSpeed = 4.5f;
    public float vectorAngleWTargEpsilon = 20f; // When raising weapon, player also turning to target( to crosshair), this value will determine if he turned enough or not
    public float minVectorAngleWTargToRaiseWeapon = 34f;
    public float minVectorAngleWTargToLookIK = 30f; // when player is turning to target if the angle is less then this value Body/Head Ik will start
    public float minVectorAngleWTargToRightHandIK = 37f; // when player is turning to target if the angle is less then this value right hand Ik will start 
    public LayerMask bulletLayerMask;    // mask is used to determine which layers can be effected from bullet hit effects(raycast mask)
    public GameObject changeFocusedPartHolderSoundPrefab;
    public bool canModifyWeaponParts = true;
    public FootStepSounds footStepSounds;
    public Vector3 handsToWallRayCPos = new Vector3(.35f, 1.35f, 0); //experimental-not finished
    public float handsToWallDistLR = .35f; //experimental-not finished
    public LayerMask handsToWallLayerMask; //experimental-not finished
    public float handDistFromWall = .1f; //experimental-not finished
    public Vector3 handsToWallFix; //experimental-not finished
    public float grabMidCenterY = 1.16f; // used when collecting a weapon, in WeaponTake script. You can debug this with a weapon
    public float grabUpDownDist = .34f; // used when collecting a weapon, in WeaponTake script. You can debug this with a weapon
    public bool debugFire2Press = false; // you can debug and test variables for camera-gun position etc.
    public bool debugWeaponPositions = false;
    private float weaponIkHeadWeight = 1;
    private float weaponIkBodyWeight = 1;
    private float weaponIKClamp = .5f;
    public float toAimRotationSpeed = 25f;
    public float toAimRotationForce = 20f;
    public LayerMask gunColliderCheckMask;

    #endregion

    #region Hidden/Nonserializable/Private Player data
    private Transform mainCamera;
    [HideInInspector]
    public Decals decalsScript; // script in the hierarchy
    [HideInInspector]
    public Hud hud; // child of 3d canvas, menu-info etc.
    [HideInInspector]
    public Crosshair crosshair; // child of 2d canvas, 2d crosshair - always middle of screen
    [HideInInspector]
    public Transform lastGun;   // used to draw gun with a button and holster-draw
    [System.NonSerialized]
    public bool canMove = true;
    [HideInInspector]
    public Transform target;    // used to calc. weapon calculations when firing etc.
    [HideInInspector]
    public Transform cGun;  // used to hold current gun when gun is not holstered
    [HideInInspector]
    public Transform weaponToDraw;  // used to change gun
    private Animator animator;
    [HideInInspector]
    public float maxTorq;
    [HideInInspector]
    FootPlanting footPlanting;
    //[System.NonSerialized]
    public bool isWalking = false;
    [HideInInspector]
    public bool isAiming = false;   // if player is holding right mouse with a gun
    [System.NonSerialized]
    public Transform itemToTake;
    [HideInInspector]
    public List<GameObject> weaponsPickable;
    [System.NonSerialized]
    public Vector3 itemFocusPos;
    // Weapon
    [HideInInspector]
    public Transform fireReference; // this is used to calculate fire raycast
    [HideInInspector]
    public Transform weaponIKBase;
    [HideInInspector]
    public bool turningWithGun = false; // used to calculate weapon turning when holding right button
    //[System.NonSerialized]
    public float curVectorAngleWTarget;
    [HideInInspector]
    public bool isHandOnGun = false;    // used to determine the moment which player gets his hand to gun in draw animations 
    [HideInInspector]
    public bool isHandAwayFromGun = false;  // used to determine the moment which player gets his hand away from gun in holster animations 
    [HideInInspector]
    public Vector3 curLookAt;   // var to change rotation smootly when aiming w gun
    [System.NonSerialized]
    public float _randomHandTwistSign;
    // Other
    [HideInInspector]
    public BezierSpline spline;   // used to calculate climb 
    [HideInInspector]
    public Vector3 climbPoint;  //  used to calculate climb 
    [HideInInspector]
    public bool isNewClipInLeftHand = false;
    [HideInInspector]
    public bool isNewClipOffLeftHand = false;
    [System.NonSerialized]
    public bool addForceToProjectile = false;
    [System.NonSerialized]
    public bool isSecFireProjectile = false;
    [System.NonSerialized]
    public SecondaryFireGP secFireGp;
    [System.NonSerialized]
    public GameObject projectile;
    [System.NonSerialized]
    public GunAtt gunAtt;
    [System.NonSerialized]
    public float splineSplit = 25;  //  used to calculate cover's close point to player - higher better
    [System.NonSerialized]
    public bool isGunCollidingWall = false;
    #endregion

    #region WeaponIK Handler Variables
    // Weapon IK Handler
    #region Animator Hash
    private readonly int _holsterTag = Animator.StringToHash("Holster");
    private readonly int _reloadTag = Animator.StringToHash("Reload");
    private readonly int _emptyState = Animator.StringToHash("Empty");
    private readonly int _lowIdleTag = Animator.StringToHash("LowIdle");
    private readonly int _lookAtWeaponTag = Animator.StringToHash("LookAtWeapon");
    private readonly int _grabTag = Animator.StringToHash("Grab");
    private readonly int _readyIdleTag = Animator.StringToHash("ReadyIdle");
    private readonly int _drawTag = Animator.StringToHash("Draw");
    #endregion

    [System.NonSerialized]
    public float _weaponBodyBob = 0;
    private float _weaponBodyBobV;
    [System.NonSerialized]
    public Vector3 _weaponArmSpread = Vector3.zero;
    private Vector3 _weaponArmSpreadV;
    private float headAim = 0, headAim_V, headAimTarget;
    private float rHandAim = 0, rHandAim_V, rHandAimTarget;
    private float rHandAimRot = 0, rHandAimRot_V, rHandAimRotTarget;
    private float lHandAim = 0, lHandAim_V, lHandAimTarget;
    #endregion

    void Awake()
    {
        target = GameObject.FindGameObjectWithTag("TargetSphere").transform;
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").transform;
        fireReference = target.FindChild("FireReference");
        decalsScript = GameObject.FindGameObjectWithTag("Decals").GetComponent<Decals>();
        hud = GameObject.FindGameObjectWithTag("Hud").GetComponent<Hud>();
        crosshair = GameObject.FindGameObjectWithTag("CrossHair").GetComponent<Crosshair>();
        weaponIKBase = GameObject.FindGameObjectWithTag("WeaponIK").transform;
        ChooseAGunIfThereNoGunToDraw();
        Guns.RemoveAll(gun => gun == null); // remove null guns
        knownGunParts.RemoveAll(part => part == null); // remove null parts
        footStepSounds.m_Animator = GetComponent<Animator>();
        footPlanting = new FootPlanting();//(foot offset may not be serializable,change them in this class)
        animator = GetComponent<Animator>();
        fireReference = target.FindChild("FireReference");
        coverVars.animator = animator; coverVars.playerT = transform;
    }

    void Start()
    {
        if (canModifyWeaponParts)
            CheckForPartsUnknown();
        hud.ChooseAWeaponSpriteOnStart();
        footPlanting.FootPlantStart(footStepSounds);
        // set control mode for animator
        switch (controlMode)
        {
            case ControlMode.Free:
                GetComponent<Animator>().SetInteger("ControlMode", 0);
                break;
            case ControlMode.Static:
                GetComponent<Animator>().SetInteger("ControlMode", 1);
                break;
        }
        //Cursor.visible = false;
    }

    void Update()
    {
        ChooseAGunIfThereNoGunToDraw();
#if !MOBILE_INPUT
        if (Input.GetKeyDown(KeyCode.P))
        {
            Application.LoadLevel(Application.loadedLevelName);
        }
#endif
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

    }
    void FixedUpdate()
    {
        // Physics related codes
        if ((addForceToProjectile && projectile && gunAtt) || (addForceToProjectile && projectile && secFireGp))
        {
            if (!isSecFireProjectile)
            {
                AddForceToProjectile(projectile, gunAtt);
            }
            else
            {
                Debug.DebugBreak();
                AddForceToProjectile(secFireGp, gunAtt, projectile);
            }
            addForceToProjectile = false;
            isSecFireProjectile = false;

        }
        else
        {
            addForceToProjectile = false;
            isSecFireProjectile = false;
        }
    }

    void OnAnimatorIK(int layerIndex)
    {
        // determine footstep audio
        footPlanting.FootPlantOnAnimatorIK(layerIndex);

        // Weapon Layer & Weapon IK Handler 
        HandleWeaponIK();
    }

    void OnTriggerStay(Collider col)
    {
        isGunCollidingWall = false;
        isGunCollidingWall = checkMask(col.gameObject.layer, gunColliderCheckMask) ? true : false;
    }
    void OnTriggerExit(Collider col)
    {
        isGunCollidingWall = false;
    }

    #region Player Logic Functions
    public float headSmooth = .4f, handSmooth = .5f;
    // Handle Weapon Ik based on States
    private void HandleWeaponIK()
    {
        AnimatorStateInfo _sI1 = animator.GetCurrentAnimatorStateInfo(1);
        AnimatorStateInfo _sI4 = animator.GetCurrentAnimatorStateInfo(4);
        AnimatorStateInfo _sI1Next = animator.GetNextAnimatorStateInfo(1);
        AnimatorStateInfo _sI4Next = animator.GetNextAnimatorStateInfo(4);

        // Set layer weight based on other layers
        if ((_sI1.shortNameHash == _emptyState && _sI1Next.tagHash != _drawTag) || _sI1Next.shortNameHash == _emptyState || _sI1.tagHash == _holsterTag || _sI4Next.tagHash == _grabTag || _sI4.shortNameHash == _grabTag)
            animator.SetLayerWeight(1, (Mathf.Lerp(animator.GetLayerWeight(1), 0, Time.deltaTime * 1.5f)));
        else
            animator.SetLayerWeight(1, (Mathf.Lerp(animator.GetLayerWeight(1), 1, Time.deltaTime * 1f)));

        if (!cGun || _sI1.shortNameHash == _emptyState || (cGun && cGun.GetComponent<GunAtt>().isMelee))
        {
            rHandAim = 0; lHandAim = 0; headAim = 0; rHandAimTarget = 0; lHandAimTarget = 0; headAimTarget = 0;
            return;
        }
        gunAtt = cGun.GetComponent<GunAtt>();

        if (turningWithGun)
        {
            rHandAimTarget = 1;
            lHandAimTarget = 1;
            headAimTarget = 1;
        }
        else if (_sI1.tagHash == _lowIdleTag)                                       // Weapon lower Idle state
        {
            //IK logic in this state(all possible transitions)
            if (_sI1Next.tagHash == _holsterTag || _sI1Next.tagHash == _reloadTag || _sI1Next.tagHash == _lookAtWeaponTag || _sI4Next.tagHash == _grabTag || _sI4.tagHash == _grabTag)
            {
                lHandAimTarget = 0;

            }
            else if (_sI1Next.tagHash == _readyIdleTag)     // Next State is weapon ready
            {
                lHandAimTarget = 1;
                rHandAimTarget = 1;
                headAimTarget = 1;
            }
            else
            {
                lHandAimTarget = 1;
            }
        }
        else if (_sI1.tagHash == _readyIdleTag)                                // Weapon Ready State
        {
            //IK logic in this state(all possible transitions)
            if (_sI1Next.tagHash == _reloadTag || _sI4Next.tagHash == _grabTag)
            {
                lHandAimTarget = 0;
                rHandAimTarget = 0;
                headAimTarget = 0;
            }
            else
            {
                lHandAimTarget = 1;
                rHandAimTarget = 1;
            }
        }
        else
        {
            headAimTarget = 0;
            rHandAimTarget = 0;
        }

        float headAimSmooth = headSmooth;
        // Body&Head IK & body bob-spread
        if (headAimTarget < .5f)
            headAimSmooth = .3f; // speed up lowering gun

        if (curVectorAngleWTarget > minVectorAngleWTargToLookIK)
            headAimTarget = 0;
        headAim = Mathf.SmoothDamp(headAim, headAimTarget, ref headAim_V, headAimSmooth);

        _weaponBodyBob = Mathf.SmoothDamp(_weaponBodyBob, 0, ref _weaponBodyBobV, gunAtt.bodyRecoverSpeedInverse * Time.deltaTime);

        Vector3 defTargetPosition = mainCamera.TransformPoint(0, 0, target.localPosition.z);
        Vector3 defTargetPosToCurTargetPos = (-defTargetPosition + target.position).normalized;

        // body rotation for this weapon
        Vector3 lookPos = defTargetPosition + defTargetPosToCurTargetPos * _weaponBodyBob + (target.right * gunAtt.bodyFixRight) + (target.up * gunAtt.bodyFixUp);
        animator.SetLookAtPosition(lookPos);

        animator.SetLookAtWeight(headAim, weaponIkBodyWeight, weaponIkHeadWeight, 0, weaponIKClamp);

        if (rHandAimTarget > .5f)
            rHandAimTarget = curVectorAngleWTarget < minVectorAngleWTargToRightHandIK ? 1 : 0;

        //RightHand IK&weapon spread logic in this state(we modify right hand IK only in readyIdle states)
        Vector3 targetLocalPos = target.localPosition;
        float spreadZ = (Mathf.Abs(target.localPosition.x) + Mathf.Abs(target.localPosition.y)) * _randomHandTwistSign * .5f;
        Quaternion weaponSpread = Quaternion.Euler(new Vector3(targetLocalPos.y * gunAtt.spreadAxisMultipliers.x, targetLocalPos.x * gunAtt.spreadAxisMultipliers.y, spreadZ * gunAtt.spreadAxisMultipliers.z));

        // Right hand position smoothing variables
        float rHandAimSmooth = rHandAimTarget > .5f ? handSmooth : .3f;
        // Right hand rotation smoothing variables
        float rhanAimRotSmooth = rHandAimTarget > .5f ? handSmooth : .3f;
        rHandAim = Mathf.SmoothDamp(rHandAim, rHandAimTarget, ref rHandAim_V, rHandAimSmooth);
        rHandAimRot = Mathf.SmoothDamp(rHandAimRot, rHandAimTarget, ref rHandAimRot_V, rhanAimRotSmooth);

        if (debugFire2Press)
        {
            animator.SetIKPosition(AvatarIKGoal.RightHand, weaponIKBase.position);
            animator.SetIKRotation(AvatarIKGoal.RightHand, weaponIKBase.rotation * weaponSpread);
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, rHandAim);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, rHandAimRot);
        }
        else
        {
            animator.SetIKPosition(AvatarIKGoal.RightHand, weaponIKBase.position);
            animator.SetIKRotation(AvatarIKGoal.RightHand, weaponIKBase.rotation * weaponSpread);
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, rHandAim);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, rHandAimRot);
        }

        lHandAim = Mathf.SmoothDamp(lHandAim, lHandAimTarget, ref lHandAim_V, .3f);

        animator.SetIKPosition(AvatarIKGoal.LeftHand, gunAtt.leftHandle.transform.position);
        animator.SetIKRotation(AvatarIKGoal.LeftHand, gunAtt.leftHandle.transform.rotation);
        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, lHandAim);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, lHandAim);
    }

    //// Calculate Angular Velocity 
    //public Vector3 CalculateV()
    //{
    //    // You can simulate physics with this like hitting walls/angular velocity etc., after unfreezing rotation Y axis of rigidbody-not used atm

    //    Vector3 currentDir = transform.forward;
    //    Vector3 newDir = new Vector3(target.position.x, transform.position.y, target.position.z) - transform.position;
    //    var x = Vector3.Cross(currentDir.normalized, newDir.normalized);
    //    float theta = Mathf.Asin(x.magnitude);
    //    var w = x.normalized * theta / Time.deltaTime;
    //    var rotSpeed = rotateToTargetSpeed;
    //    Quaternion q = transform.rotation * rb.inertiaTensorRotation;
    //    var T = q * Vector3.Scale(rb.inertiaTensor, (Quaternion.Inverse(q) * (w * rotateToTargetSpeed)));
    //    return T;

    //    //rb.angularVelocity = T;
    //    //rb.AddTorque(T, ForceMode.VelocityChange);
    //    //rb.maxAngularVelocity = T.magnitude;
    //}

    // Manage rigidbodyOfProjectile after Instantiate
    public void AddForceToProjectile(GameObject curProjectile, GunAtt gunAtt)
    {
        if (curProjectile.GetComponent<Rigidbody>())
        {
            curProjectile.GetComponent<Rigidbody>().velocity = curProjectile.transform.TransformDirection(Vector3.forward * gunAtt.projectileFireForce);
        }
    }

    public GameObject GetClosestPickable()
    {
        int closestIndex = 0, i = 0; float closestDist = 999;
        foreach (GameObject go in weaponsPickable)
        {
            if (Vector3.Distance(transform.position, go.transform.position) < closestDist)
                closestIndex = i;
            i++;
        }
        return weaponsPickable[closestIndex];
    }
    private bool checkMask(int thisObjectsLayer, LayerMask layermask)
    {
        return ((layermask >> thisObjectsLayer) & 1) == 1;
    }

    public IEnumerator AddBulletForceToRigidbodys(float waitForSecs, RaycastHit hit, float thisBulletPower, float bulletForceToRigidbodys)
    {
        yield return new WaitForSeconds(waitForSecs);
        // addforce if it has rigidbody
        if (hit.collider && hit.collider.GetComponent<Rigidbody>())
        {
            hit.collider.GetComponent<Rigidbody>().AddForce((hit.point - mainCamera.position).normalized * thisBulletPower * bulletForceToRigidbodys);
        }
    }

    // Manage rigidbodyOfProjectile after Instantiate
    public void AddForceToProjectile(SecondaryFireGP secFireGp, GunAtt gunAtt, GameObject projectile)
    {

        //projectile.transform.LookAt(fireReference.position);
        projectile.GetComponent<Rigidbody>().isKinematic = false;
        projectile.GetComponent<Collider>().enabled = true;
        if (projectile.GetComponent<Rigidbody>())
        {
            projectile.GetComponent<Rigidbody>().velocity = projectile.transform.TransformDirection(Vector3.forward * secFireGp.projectileForce);
        }
    }

    // we always draw last gun var, so choose one 
    void ChooseAGunIfThereNoGunToDraw() // if last gun not assigned
    {
        if (Guns.Count > 0 && !lastGun)
        {
            lastGun = Guns[0].transform;

            hud.ChooseAWeaponSpriteOnStart();
        }

        if (Guns.Count == 0)
        {
            hud.PrintAmmoText(0);
            hud.PrintSecondaryAmmoText(0);
        }
    }

    // Reset locomotion parameter
    public IEnumerator ResetVelXVelYCoroutine(float waitForSecs)
    {
        yield return new WaitForSeconds(waitForSecs);
        animator.SetFloat("VelX", 0);
        animator.SetFloat("VelY", 0);
    }

    // used to intantiate gos delayed
    public IEnumerator InstantiateCoroutine(float waitForSecs, GameObject go, Vector3 pos)
    {
        yield return new WaitForSeconds(waitForSecs);
        Instantiate(go, pos, Quaternion.identity);
    }
    // used to intantiate gos delayed
    public IEnumerator InstantiateCoroutine(float waitForSecs, GameObject prefab, Vector3 pos, Quaternion rot, Transform parent, bool setAsParent = true)
    {
        yield return new WaitForSeconds(waitForSecs);
        GameObject newObj = Instantiate(prefab, pos, rot) as GameObject;
        if (setAsParent)
            newObj.transform.parent = parent;
    }

    public IEnumerator DestroyCoroutine(float waitForSecs, GameObject objToDestroy)
    {
        yield return new WaitForSeconds(waitForSecs);
        Destroy(objToDestroy);
    }

    // Player learns Which Gun Parts he know at start ( you can add gun part prefabs to let him know before game play starts )
    public void CheckForPartsUnknown()
    {
        List<Transform> unknownParts = new List<Transform>();
        if (Guns.Count == 0)
            return;
        foreach (GameObject gun in Guns)
        {
            if (!gun.GetComponent<WeaponParts>())
                continue;
            unknownParts = gun.GetComponent<WeaponParts>().GetUnKnownPartsInWeapon(knownGunParts, gun);

            foreach (Transform part in unknownParts)
                knownGunParts.Add(part);
        }

    }

    // used to turn on off flashlight on weapon
    public void WeaponFlashLightONOFF(WeaponParts gunParts)
    {
        // if there is a camera attached to this gun turn it on or off
        FlashLightGP flashLight = gunParts.GetFlashLight(cGun);
        if (!flashLight)
            return;

        if (flashLight.lightGo.activeSelf)
        {
            flashLight.lightGo.SetActive(false);
            if (flashLight.turnOffSound)
                Instantiate(flashLight.turnOffSound, flashLight.transform.position, Quaternion.identity);
        }
        else
        {
            flashLight.lightGo.SetActive(true);
            if (flashLight.turnOnSound)
                Instantiate(flashLight.turnOnSound, flashLight.transform.position, Quaternion.identity);
        }
    }

    public IEnumerator FixLocalPosRotScaleOfPart(Transform dummy, Transform oldObject, Vector3 pos, Quaternion rot, Vector3 scale)
    {
        yield return new WaitForSeconds(.1f);
        if (oldObject)
        {
            oldObject.localPosition = pos;
            oldObject.localRotation = rot;
            oldObject.localScale = scale;
        }
        if (dummy)
        {
            Destroy(dummy.gameObject);
        }
    }

    public IEnumerator Manage()
    {
        yield return new WaitForSeconds(.14f);
        GunAtt gunAtt = cGun.GetComponent<GunAtt>();
        WeaponParts gunParts = cGun.GetComponent<WeaponParts>();


        FlashLightGP flashlightGP = null;
        GripGP gripGp = null;
        ExtraClipGp extraClipGP = null;
        SecondaryFireGP secFireGP = null;
        SightGP sightGP = null;
        BarrelGp barrelGP = null;

        gunParts.GetAll(ref secFireGP, ref gripGp, ref flashlightGP, ref extraClipGP, ref sightGP, ref barrelGP);

        if (gripGp)
        {
            gunAtt.grip = gripGp.gameObject;

            if (gunAtt.defaultLeftHandle != gunAtt.leftHandle.gameObject)
                Destroy(gunAtt.leftHandle.gameObject);

            if (gripGp.newLeftHandle)
            {
                gunAtt.leftHandle = Instantiate(gripGp.newLeftHandle);
                gunAtt.leftHandle.SetParent(gunAtt.transform);
                gunAtt.leftHandle.localPosition = gripGp.newLeftHandle.localPosition;
                gunAtt.leftHandle.localRotation = gripGp.newLeftHandle.localRotation;
                gunAtt.defaultLeftHandle.SetActive(false);
            }
        }
        else
            gunAtt.grip = null;

        if (secFireGP)
        {
            gunAtt.secFire = secFireGP.gameObject;
            hud.PrintSecondaryAmmoText(currentMagz[gunAtt.secFire.GetComponent<SecondaryFireGP>().bulletStyle]);
        }
        else
        {
            hud.PrintSecondaryAmmoText(0);
            gunAtt.secFire = null;
        }

        if (extraClipGP)
        {
            if (gunAtt.isUsingClipReload)
            {
                gunAtt.curClipObject = extraClipGP.transform;
                foreach (Transform tr in knownGunParts)
                    if (tr.GetComponent<PartHolderIndex>().partName == gunAtt.curClipObject.GetComponent<PartHolderIndex>().partName &&
                        tr.GetComponent<PartHolderIndex>().compatibleWeaponName == gunAtt.curClipObject.GetComponent<PartHolderIndex>().compatibleWeaponName)
                        gunAtt.curClipPrefab = tr;
            }

            gunAtt.maxClipCapacity = extraClipGP.clipCapacity;
            //if (gunAtt.currentClipCapacity > gunAtt.maxClipCapacity)
            //    gunAtt.currentClipCapacity = gunAtt.maxClipCapacity;
            if (currentMagz[gunAtt.bulletStyle] <= gunAtt.maxClipCapacity)
            {
                gunAtt.currentClipCapacity = currentMagz[gunAtt.bulletStyle];
                currentMagz[gunAtt.bulletStyle] = 0;
            }
            else if (currentMagz[gunAtt.bulletStyle] > gunAtt.maxClipCapacity)
            {
                gunAtt.currentClipCapacity = gunAtt.maxClipCapacity;
                currentMagz[gunAtt.bulletStyle] -= gunAtt.maxClipCapacity;
            }
            hud.PrintAmmoText(gunAtt.currentClipCapacity);
            hud.PrintAmmoText(currentMagz[gunAtt.bulletStyle], false);
        }

        if (flashlightGP)
            gunAtt.flashLight = flashlightGP.gameObject;
        else
            gunAtt.flashLight = null;

        if (sightGP)
            gunAtt.sight = sightGP.gameObject;
        else
            gunAtt.sight = null;

        if (barrelGP)
            gunAtt.barrel = barrelGP.gameObject;
        else
            gunAtt.barrel = null;
    }

    // new clip in left hand fix after instantiate
    public IEnumerator FixClipInLeftHand(Transform clip)
    {
        yield return null;
        clip.localPosition = Vector3.zero;
        clip.localRotation = Quaternion.Euler(Vector3.zero);
    }
    #endregion

    #region Animation Triggered Functions
    // Animation Triggered Functions
    // reload animations need to have this event
    public void ReloadDone()
    {
        GunAtt gunAtt = cGun.GetComponent<GunAtt>();
        if (currentMagz[gunAtt.bulletStyle] < gunAtt.maxClipCapacity - gunAtt.currentClipCapacity)
        {
            gunAtt.currentClipCapacity += currentMagz[gunAtt.bulletStyle];
            currentMagz[gunAtt.bulletStyle] = 0;
        }
        else
        {
            currentMagz[gunAtt.bulletStyle] -= (gunAtt.maxClipCapacity - gunAtt.currentClipCapacity);
            gunAtt.currentClipCapacity = gunAtt.maxClipCapacity;
        }
        if (hud)
        {
            hud.PrintAmmoText(gunAtt.currentClipCapacity);
            hud.PrintAmmoText(currentMagz[gunAtt.bulletStyle], false);

        }
    }
    // draw animations need to have this event
    public void IsHandOnGun()
    {
        isHandOnGun = true;
    }
    // holster animations need to have this event
    public void IsHandAwayFromGun()
    {
        isHandAwayFromGun = true;
    }

    // reload animation with clip
    public void NewClipInLeftHand()
    {
        if (cGun && (cGun.GetComponent<GunAtt>().isUsingClipReload || cGun.GetComponent<GunAtt>().firesCurClipObject)) // dont set for launchers firing curClip(projectile)
            isNewClipInLeftHand = true;
    }

    // reload animation with clip
    public void NewClipOffLeftHand() // set for launchers to instantiate curClipPrefab(projectile)
    {
        if (cGun && (cGun.GetComponent<GunAtt>().isUsingClipReload || cGun.GetComponent<GunAtt>().firesCurClipObject))
            isNewClipOffLeftHand = true;
    }

    // Grab animations use this
    public void TakeItem()
    {
        if (itemToTake)
        {
            if (itemToTake.GetComponent<GunAtt>())
            {
                itemToTake.GetComponent<Rigidbody>().isKinematic = true;
                itemToTake.GetComponent<BoxCollider>().enabled = false;
                itemToTake.GetComponent<SphereCollider>().enabled = false;
                itemToTake.transform.position = new Vector3(0, -300, 0);
            }
        }
    }
    #endregion
}
