using UnityEngine;

public class WeaponReadySMB : CustomSMB
{
    #region Animator States-Paramaters...
    // for better performance get hash vars
    private readonly int _climbTag = Animator.StringToHash("Climb");
    private readonly int _rollState = Animator.StringToHash("Roll");
    private readonly int _footOnGroundState = Animator.StringToHash("FootOnGround");
    private readonly int _handOnGroundState = Animator.StringToHash("HandOnGround");
    private readonly int _pivotTag = Animator.StringToHash("Pivot");
    private readonly int _holsterState = Animator.StringToHash("Holster");
    private readonly int _reloadState = Animator.StringToHash("Reload");
    private readonly int _speedPar = Animator.StringToHash("Speed");
    private readonly int _coverLocomotion = Animator.StringToHash("CoverLocomotion");
    #endregion

    #region Private Variables
    private PlayerAtts plAtts;
    private GunAtt gunAtt;
    private Transform cGunT;
    private float nextFireTimer = -1;

    private float tempMuzzleEnTime;
    private float tempMuzzleEnTimeSecFire;
    private float nextSecondaryFireTimer = -1;
    private bool isSecFire = false;
    private TargetLogic targetLogic;
    public float onNoAimLowerTime = 2f;
    private float _noAimLowerTime;
    #endregion

    public override void Init(Animator anim)
    {
        plAtts = userInput.GetComponent<PlayerAtts>();
        targetLogic = GameObject.FindGameObjectWithTag("TargetSphere").GetComponent<TargetLogic>();
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _noAimLowerTime = (userInput.FirePress && userInput.Fire2Press) ? _noAimLowerTime : -1;
        nextSecondaryFireTimer = -1;
        nextFireTimer = -1;
        gunAtt = plAtts.cGun.GetComponent<GunAtt>();

        if (userInput.Fire2Press || plAtts.debugFire2Press)
        {
            plAtts.weaponIKBase.localPosition = gunAtt.onAimPositionRotation.localPosition;
            plAtts.weaponIKBase.localRotation = gunAtt.onAimPositionRotation.localRotation;
        }
        else
        {
            plAtts.weaponIKBase.localPosition = gunAtt.onNoAimPositionRotation.localPosition;
            plAtts.weaponIKBase.localRotation = gunAtt.onNoAimPositionRotation.localRotation;
        }
    }
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (userInput.FlashLightDown && cGunT && cGunT.GetComponent<WeaponParts>())
        {
            plAtts.WeaponFlashLightONOFF(cGunT.GetComponent<WeaponParts>());
            userInput.m_FlashLightDown = false; // you can delete this according to your fixed delta time 
        }

        if (!plAtts.debugWeaponPositions)
        {
            if (userInput.Fire2Press || plAtts.debugFire2Press)
            {
                plAtts.weaponIKBase.localPosition = gunAtt.onAimPositionRotation.localPosition;
                plAtts.weaponIKBase.localRotation = gunAtt.onAimPositionRotation.localRotation;
            }
            else if (userInput.FirePress || _noAimLowerTime > 0)
            {
                plAtts.weaponIKBase.localPosition = gunAtt.onNoAimPositionRotation.localPosition;
                plAtts.weaponIKBase.localRotation = gunAtt.onNoAimPositionRotation.localRotation;
            }
        }


        if (userInput.FirePress && !userInput.Fire2Press)
            _noAimLowerTime = onNoAimLowerTime;
        else if (userInput.Fire2Press)
            _noAimLowerTime = -1;
        else if (userInput.SprintPress && !userInput.FirePress)
            _noAimLowerTime = -1;
        else
            _noAimLowerTime -= Time.deltaTime;

        bool isNextStateHolster = animator.GetNextAnimatorStateInfo(layerIndex).tagHash == _holsterState;
        bool isNextStateReload = animator.GetNextAnimatorStateInfo(layerIndex).tagHash == _reloadState;

        if ((!userInput.Fire2Press && !plAtts.debugFire2Press && !(userInput.FirePress || (_noAimLowerTime > 0))) || isNextStateHolster || !CanAim(animator.GetCurrentAnimatorStateInfo(0)) || isNextStateReload)
        {
            animator.SetBool("Aim", false);
            if (plAtts.cGun.GetComponent<GunAtt>().muzzleFlash)
                plAtts.cGun.GetComponent<GunAtt>().muzzleFlash.SetActive(false);
            plAtts.isAiming = false;
            return;
        }

        gunAtt = plAtts.cGun.GetComponent<GunAtt>();
        cGunT = plAtts.cGun.transform;

        #region FIRE
        // Fire
        GameObject holdFireSound = null;
        GameObject holdmuzzleFlash = null;
        GameObject curProjectile = null;

        if ((userInput.FirePress && (nextFireTimer < 0 || userInput.Fire1Down) && gunAtt.currentClipCapacity > 0 && !animator.IsInTransition(1) && nextSecondaryFireTimer < 0) ||
            (userInput.SecondaryFireDown && gunAtt.secFire && nextFireTimer < 0 && plAtts.currentMagz[gunAtt.secFire.GetComponent<SecondaryFireGP>().bulletStyle] > 0 && nextSecondaryFireTimer < 0))
        {
            if (userInput.Fire1Down)
                userInput.m_Fire1Down = false; // fix fixed delta time, otherwise dont delete

            if (userInput.SecondaryFireDown && gunAtt.secFire)
            {
                isSecFire = true;
                nextSecondaryFireTimer = 1;
            }
            else
            {
                nextSecondaryFireTimer = -1;
                isSecFire = false;
            }

            #region Normal Fire
            if (!isSecFire) // normal gun fire
            {
                animator.SetTrigger("Fire");

                nextFireTimer = 1;

                gunAtt.currentClipCapacity -= 1;

                if (gunAtt.IsAnimated)
                    gunAtt.animatorObjectIfAnimated.SetTrigger("Fire");

                // fire sound
                if (!gunAtt.barrel || (gunAtt.barrel && gunAtt.barrel.GetComponent<BarrelGp>().fireSounds.Count <= 0)) // normal fire sounds
                {
                    if (gunAtt.sounds.fireSounds.Count > 0)
                    {
                        int randFireSound = Random.Range(0, gunAtt.sounds.fireSounds.Count);
                        holdFireSound = Instantiate(gunAtt.sounds.fireSounds[randFireSound], gunAtt.transform.position, gunAtt.transform.rotation) as GameObject;
                    }
                }
                else  // barrel sounds if there is
                {
                    BarrelGp barrelGp = gunAtt.barrel.GetComponent<BarrelGp>();
                    int randFireSound = Random.Range(0, barrelGp.fireSounds.Count);
                    holdFireSound = Instantiate(barrelGp.fireSounds[randFireSound], gunAtt.transform.position, gunAtt.transform.rotation) as GameObject;
                }

                // muzzle flash
                if (gunAtt.muzzleFlash)
                {
                    gunAtt.muzzleFlash.SetActive(true);
                    tempMuzzleEnTime = gunAtt.muzzleFlashTimePerBullet;
                }

                // empty shell
                if (gunAtt.emptyShellPosition && gunAtt.EmptyShellPrefab)
                {
                    Vector3 randomEmptyShellForce = new Vector3(Random.Range(gunAtt.emptyShellMinForce.x, gunAtt.emptyShellMaxForce.x), Random.Range(gunAtt.emptyShellMinForce.y, gunAtt.emptyShellMaxForce.y), Random.Range(gunAtt.emptyShellMinForce.z, gunAtt.emptyShellMaxForce.z));
                    GameObject goEmptyShell = Instantiate(gunAtt.EmptyShellPrefab, gunAtt.emptyShellPosition.position, gunAtt.emptyShellPosition.rotation) as GameObject;
                    goEmptyShell.GetComponent<Rigidbody>().AddForce(gunAtt.emptyShellPosition.rotation * randomEmptyShellForce, ForceMode.Impulse);
                }

                // 2d canvas crosshair 
                float gripAndSightcrosshairCenterSpaceDecrease = 0;
                if (gunAtt.sight)
                    gripAndSightcrosshairCenterSpaceDecrease = gunAtt.sight.GetComponent<SightGP>().crosshairCenterOnFireDecreaseAmount;
                if (gunAtt.grip)
                    gripAndSightcrosshairCenterSpaceDecrease += gunAtt.grip.GetComponent<GripGP>().crosshairCenterOnFireDecreaseAmount;
                if (plAtts.crosshair)
                    plAtts.crosshair.currentSpace = plAtts.crosshair.currentSpace + gunAtt.crosshairCenterIncreaseOnFire - gripAndSightcrosshairCenterSpaceDecrease;

                // change ammo text
                if (plAtts.hud)
                    plAtts.hud.PrintAmmoText(gunAtt.currentClipCapacity);

                // Hits-Fx-Sound fx-Piercing-BulletHoles-FireType
                if (!gunAtt.isFiringProjectile)
                {
                    if (!gunAtt.isShotGun)
                    {
                        RaycastHit[] hits;  // raycast array to hold hits to colliders
                        float barrelBulletPowerIncrease = 0;
                        if (gunAtt.barrel)
                            barrelBulletPowerIncrease = gunAtt.barrel.GetComponent<BarrelGp>().bulletPowerIncrease;
                        float thisBulletPower = gunAtt.bulletPower + barrelBulletPowerIncrease; // this bullet's power to calculate how many wall it can pierce

                        if (targetLogic.isHit)
                        {
                            // realistic ray & but not good for games(spawn is where the bullet exits from gun)
                            //hits = Physics.RaycastAll(spawn.position, plAtts.fireReference.position - spawn.position, gunAtt.maxBulletDistance, plAtts.bulletLayerMask); // raycastAll - get hits
                            // proper raycast way
                            hits = Physics.RaycastAll(userInput.mainCamera.position + userInput.mainCamera.forward * targetLogic.fixOffset, plAtts.fireReference.position - userInput.mainCamera.position, gunAtt.maxBulletDistance, plAtts.bulletLayerMask); // raycastAll - get hits

                            SortList(ref hits); // sort raycast hits by distance for calculations
                            ManageHits(hits, thisBulletPower);
                        }
                        else
                        {
                            // Use this to Instantiate effects even when ray hits nothing, like bullet trail effect
                            ManageNoTargetLogicRayHit();
                        }

                    }
                    else
                    {
                        // shotgun type fire
                        for (int i = 0; i < gunAtt.shrapnelCount; i++)
                        {
                            if (targetLogic.isHit)
                            {
                                RaycastHit[] hits;  // raycast array to hold hits to colliders
                                float thisBulletPower = gunAtt.bulletPower; // this bullet's power to calculate how many wall it can pierce
                                float randomSpreadX = (float)(Random.value - 0.5) * (gunAtt.maxshrapnelSpread);
                                float randomSpreadY = (float)(Random.value - 0.5) * (gunAtt.maxshrapnelSpread);
                                float randomSpreadZ = (float)(Random.value - 0.5) * (gunAtt.maxshrapnelSpread);
                                Vector3 dir = Quaternion.Euler(new Vector3(randomSpreadX, randomSpreadY, randomSpreadZ)) * (plAtts.fireReference.position - userInput.mainCamera.position + userInput.mainCamera.forward * targetLogic.fixOffset);

                                hits = Physics.RaycastAll(userInput.mainCamera.position + userInput.mainCamera.forward * targetLogic.fixOffset, dir, gunAtt.maxBulletDistance, plAtts.bulletLayerMask); // raycastAll - get hits
                                SortList(ref hits); // sort raycast hits by distance for calculations
                                ManageHits(hits, thisBulletPower);
                            }
                            else
                            {
                                // Use this to Instantiate effects even when ray hits nothing, like bullet trail effect
                                ManageNoTargetLogicRayHit();
                            }
                        }
                    }

                }

                else   // fire projectile
                {
                    if (gunAtt.firesCurClipObject)
                    {
                        // get position to intantiate new curClip(projectile) after player reloads weapon
                        gunAtt.clipDefLocalPos = gunAtt.curClipObject.localPosition;
                        gunAtt.clipDefLocalRot = gunAtt.curClipObject.localRotation;

                        gunAtt.curClipObject.SetParent(null);
                        gunAtt.curClipObject.LookAt(plAtts.fireReference.position);
                        gunAtt.curClipObject.GetComponent<Rigidbody>().isKinematic = false;
                        gunAtt.curClipObject.GetComponent<Collider>().enabled = true;
                        if (gunAtt.curClipObject.GetComponent<AudioSource>())
                            gunAtt.curClipObject.GetComponent<AudioSource>().Play();
                        if (gunAtt.curClipObject.GetComponent<Projectile>().afterBurner)
                            gunAtt.curClipObject.GetComponent<Projectile>().afterBurner.SetActive(true);
                        curProjectile = gunAtt.curClipObject.gameObject;
                        plAtts.addForceToProjectile = true;
                        plAtts.projectile = curProjectile;
                        plAtts.gunAtt = gunAtt;

                    }
                }
            }
            #endregion
            #region Secondary fire weapon part logic
            else if (isSecFire) // secondary part fire
            {
                SecondaryFireGP secFireGP = gunAtt.secFire.GetComponent<SecondaryFireGP>();
                nextSecondaryFireTimer = 1;
                plAtts.currentMagz[secFireGP.bulletStyle] -= 1;

                // fire sound
                if (secFireGP.fireSound)
                {
                    holdFireSound = Instantiate(secFireGP.fireSound, gunAtt.transform.position, gunAtt.transform.rotation) as GameObject;
                }

                // muzzle flash
                if (secFireGP.muzzleFlash)
                {
                    secFireGP.muzzleFlash.SetActive(true);
                    tempMuzzleEnTimeSecFire = secFireGP.muzzleFlashTime;
                }

                // crosshair space increase
                if (plAtts.crosshair)
                    plAtts.crosshair.currentSpace += secFireGP.crosshairCenterIncreaseOnFire;

                if (plAtts.hud)
                    plAtts.hud.PrintSecondaryAmmoText(plAtts.currentMagz[secFireGP.bulletStyle]);

                // Hits-Fx-Sound fx-Piercing-BulletHoles-FireType
                if (!secFireGP.firingProjectile)
                {
                    if (!secFireGP.isShotGun)
                    {
                        if (targetLogic.isHit)
                        {
                            RaycastHit[] hits;  // raycast array to hold hits to colliders
                            float thisBulletPower = secFireGP.bulletPower; // this bullet's power to calculate how many wall it can pierce
                            hits = Physics.RaycastAll(userInput.mainCamera.position + userInput.mainCamera.forward * targetLogic.fixOffset, plAtts.fireReference.position - userInput.mainCamera.position, gunAtt.maxBulletDistance, plAtts.bulletLayerMask); // raycastAll - get hits
                            SortList(ref hits); // sort raycast hits by distance for calculations
                            // fire
                            ManageHits(hits, thisBulletPower, true);
                        }
                        else
                        {

                        }

                    }
                    else
                    {
                        // shotgun type fire
                        for (int i = 0; i < secFireGP.sharapnelCount; i++)
                        {

                            if (targetLogic.isHit)
                            {
                                RaycastHit[] hits;  // raycast array to hold hits to colliders
                                float thisBulletPower = secFireGP.bulletPower; // this bullet's power to calculate how many wall it can pierce
                                float randomSpreadX = (float)(Random.value - 0.5) * (secFireGP.maxSharapnelSpread);
                                float randomSpreadY = (float)(Random.value - 0.5) * (secFireGP.maxSharapnelSpread);
                                float randomSpreadZ = (float)(Random.value - 0.5) * (secFireGP.maxSharapnelSpread);
                                Vector3 dir = Quaternion.Euler(new Vector3(randomSpreadX, randomSpreadY, randomSpreadZ)) * (plAtts.fireReference.position - userInput.mainCamera.position + userInput.mainCamera.forward * targetLogic.fixOffset);

                                hits = Physics.RaycastAll(userInput.mainCamera.position + userInput.mainCamera.forward * targetLogic.fixOffset, dir, gunAtt.maxBulletDistance, plAtts.bulletLayerMask); // raycastAll - get hits
                                SortList(ref hits); // sort raycast hits by distance for calculations
                                ManageHits(hits, thisBulletPower);
                            }
                            else
                            {
                                // Use this to Instantiate effects even when ray hits nothing, like bullet trail effect
                                ManageNoTargetLogicRayHit();
                            }

                        }
                    }
                }
                else   // fire projectile
                {
                    SecondaryFireGP secFireGp = gunAtt.secFire.GetComponent<SecondaryFireGP>();
                    plAtts.secFireGp = secFireGp;
                    GameObject projectileGo = Instantiate(secFireGp.projectilePrefab, secFireGp.projectilePosition.position, Quaternion.LookRotation(plAtts.fireReference.position - secFireGp.projectilePosition.position)
                        /*secFireGp.projectilePosition.rotation*/) as GameObject;
                    plAtts.projectile = projectileGo;
                    plAtts.addForceToProjectile = true;
                    plAtts.isSecFireProjectile = true;
                    plAtts.gunAtt = gunAtt;
                }
            }
            #endregion

            // spread
            if (!isSecFire) // normal spread
            {
                float spreadModifiers = 0;
                if (gunAtt.sight)
                    spreadModifiers = gunAtt.sight.GetComponent<SightGP>().spreadDecreaseAmount;
                if (gunAtt.grip)
                    spreadModifiers += gunAtt.grip.GetComponent<GripGP>().spreadDecreaseAmount;
                if (userInput.FirePress)
                    spreadModifiers -= gunAtt.additionalSpreadNoAimAmount;

                plAtts._weaponBodyBob = gunAtt.bodySpread;
                plAtts._randomHandTwistSign = Random.Range(-1, 1);

                float speedParam = animator.GetFloat(_speedPar);

                float xSpread = (float)(Random.value - 0.5) * (gunAtt.spreadAmount - spreadModifiers + gunAtt.speedSpreadAmount * Mathf.Clamp01(speedParam));
                // not realistic
                //float ySpread = (float)(Random.value - 0.5) * (gunAtt.spreadAmount + gunAtt.speedSpreadAmount * animator.GetFloat(_speedPar));
                // proper spread
                float ySpread = (float)(Random.Range(.35f, 1f) - 0.5) * (gunAtt.spreadAmount - spreadModifiers + gunAtt.speedSpreadAmount * Mathf.Clamp01(speedParam));
                plAtts.target.localPosition = new Vector3(xSpread, ySpread, 0) + plAtts.target.localPosition;

            }
            else   // sec fire spread
            {
                SecondaryFireGP secFireGP = gunAtt.secFire.GetComponent<SecondaryFireGP>();
                plAtts._weaponBodyBob = gunAtt.bodySpread;
                float xSpread = (float)(Random.value - 0.5) * (secFireGP.spreadAmount);
                float ySpread = (float)(Random.Range(.35f, 1f) - 0.5) * (secFireGP.spreadAmount);

                plAtts._randomHandTwistSign = Random.Range(-1, 1);
                plAtts.target.localPosition = new Vector3(xSpread, ySpread, 0) + plAtts.target.localPosition;
            }
        } // end fire

        // dry sound 
        if (userInput.FirePress && nextFireTimer < 0 && gunAtt.currentClipCapacity <= 0)
        {
            nextFireTimer = 1;
            if (gunAtt.sounds.dry)
                holdFireSound = Instantiate(gunAtt.sounds.dry, gunAtt.transform.position, gunAtt.transform.rotation) as GameObject;
        }

        if (!gunAtt.secFire)
        {
            nextSecondaryFireTimer = -1;
        }
        else
        {
            SecondaryFireGP secFireGP = gunAtt.secFire.GetComponent<SecondaryFireGP>();
            nextSecondaryFireTimer -= gunAtt.secFire.GetComponent<SecondaryFireGP>().fireSpeed * Time.deltaTime;
            // muzzle flash disable
            if (secFireGP.muzzleFlash && tempMuzzleEnTimeSecFire < 0)
                secFireGP.muzzleFlash.SetActive(false);
            tempMuzzleEnTimeSecFire -= Time.deltaTime;
        }

        // muzzle flash disable
        if (gunAtt.muzzleFlash && tempMuzzleEnTime < 0)
            gunAtt.muzzleFlash.SetActive(false);
        tempMuzzleEnTime -= Time.deltaTime;


        nextFireTimer -= Time.deltaTime * gunAtt.fireSpeed;

        // sounds & muzzle must follow gun position
        if (holdFireSound && plAtts.cGun && !gunAtt.isFiringProjectile)
            holdFireSound.transform.parent = cGunT;

        if (holdmuzzleFlash)
            holdmuzzleFlash.transform.parent = cGunT;

        #endregion

        /* shotgun sharapnel spread debug
        for (int i = 0; i < 10; i++)
        {
            float randomSpreadX = (float)(Random.value - 0.5) * (10);
            float randomSpreadY = (float)(Random.value - 0.5) * (10);
            float randomSpreadZ = (float)(Random.value - 0.5) * (10);
            Vector3 dir2 = Quaternion.Euler(new Vector3(randomSpreadX, randomSpreadY, randomSpreadZ)) * (plAtts.fireReference.position - gunAtt.transform.position);
            Debug.DrawRay(gunAtt.transform.position, dir2, Color.green);
        }
         */

    }

    // Manage hits for decal-effect-piercing etc...
    private void ManageHits(RaycastHit[] hits, float thisBulletPower, bool isSecFire = false)
    {
        if (hits.Length == 0)
        {
            if (gunAtt.bulletTrailExitPosition && gunAtt.bulletTrailPrefab && !isSecFire)
            {
                Vector3 exitPos = gunAtt.bulletTrailExitPosition.position;
                GameObject trail;
                trail = Instantiate(gunAtt.bulletTrailPrefab, exitPos, Quaternion.LookRotation(-plAtts.target.position + exitPos)) as GameObject;
                if (gunAtt.bulletTrailPrefab.GetComponent<Rigidbody>())
                    trail.GetComponent<Rigidbody>().velocity = -trail.transform.forward * gunAtt.bulletTrailExitForce;
            }
            return;
        }

        if (thisBulletPower <= 0)
            thisBulletPower = .01f;
        int hitIndex = 0;
        GameObject tempBulletTrail = null;
        foreach (RaycastHit hit in hits)    // all raycast hits
        {


            if (thisBulletPower <= 0)   // if bullet have no power to hit next collider - exit foreach
            {
                // Find the time needed to get to last hit point
                float timeToTravel = Vector3.Distance(hits[hitIndex - 1].point, plAtts.cGun.position) / gunAtt.bulletSpeed;
                // Destroy objects after this time like bulletrail
                plAtts.StartCoroutine(plAtts.DestroyCoroutine(timeToTravel, tempBulletTrail));

                break;
            }

            //if (hit.transform.tag == "Enemy") // can be used to calc. something special
            //{

            //}
            //else
            {


                GameObject decal = null, fx = null, hitSound = null; float thisPierceDecrease = 0;
                plAtts.decalsScript.getAllNormalShot(hit.transform.tag, ref decal, ref fx, ref hitSound, ref thisPierceDecrease);

                // to look realistic Instantiate them delayed
                float timeToTravel = Vector3.Distance(hit.point, plAtts.cGun.position) / gunAtt.bulletSpeed;
                if (decal != null)
                    plAtts.StartCoroutine(plAtts.InstantiateCoroutine(
                        timeToTravel, decal, hit.point + (hit.normal * .04f), Quaternion.LookRotation(hit.normal) * Quaternion.Euler(new Vector3(0, 0, Random.Range(0, 360))), hit.transform));

                if (fx != null)
                    plAtts.StartCoroutine(plAtts.InstantiateCoroutine(
                        timeToTravel, fx, hit.point + (hit.normal * .07f), Quaternion.LookRotation(hit.normal) * Quaternion.Euler(new Vector3(0, 0, Random.Range(0, 360))), hit.transform));

                if (hitSound != null)
                    plAtts.StartCoroutine(plAtts.InstantiateCoroutine(timeToTravel, hitSound, hit.point, Quaternion.identity, hit.transform));

                // bullet trail
                if (gunAtt.bulletTrailExitPosition && gunAtt.bulletTrailPrefab && hitIndex == 0)
                {
                    Vector3 exitPos = gunAtt.bulletTrailExitPosition.position;
                    GameObject trail;
                    trail = Instantiate(gunAtt.bulletTrailPrefab, exitPos, Quaternion.LookRotation(hits[0].point - exitPos)) as GameObject;
                    if (gunAtt.bulletTrailPrefab.GetComponent<Rigidbody>())
                        trail.GetComponent<Rigidbody>().velocity = trail.transform.forward * gunAtt.bulletTrailExitForce;
                    tempBulletTrail = trail;
                }

                // add force to objects that bullet hit, if they have rigidbody
                plAtts.StartCoroutine(plAtts.AddBulletForceToRigidbodys(timeToTravel, hit, thisBulletPower, gunAtt.bulletForceToRigidbodys));

                thisBulletPower -= thisPierceDecrease;
            }

            hitIndex++;
        }
    }

    private void ManageNoTargetLogicRayHit()
    {
        if (gunAtt.bulletTrailExitPosition && gunAtt.bulletTrailPrefab)
        {
            Vector3 exitPos = gunAtt.bulletTrailExitPosition.position;
            GameObject trail;
            trail = Instantiate(gunAtt.bulletTrailPrefab, exitPos, Quaternion.LookRotation(-exitPos + plAtts.target.position)) as GameObject;
            if (gunAtt.bulletTrailPrefab.GetComponent<Rigidbody>())
                trail.GetComponent<Rigidbody>().velocity = trail.transform.forward * gunAtt.bulletTrailExitForce;
        }
    }

    // Sort objects by distance
    private void SortList(ref RaycastHit[] hits)
    {
        RaycastHit tempRcH;

        for (int i = 0; i <= hits.Length - 1; i++)
        {
            for (int j = 1; j <= hits.Length - 1; j++)
            {
                if (hits[j - 1].distance > hits[j].distance)
                {
                    tempRcH = hits[j - 1];
                    hits[j - 1] = hits[j];
                    hits[j] = tempRcH;
                }
            }
        }
    }

    // states that we cant aim - lower the gun
    private bool CanAim(AnimatorStateInfo baseStateInfo)
    {
        bool canAim = !(
     baseStateInfo.tagHash == _climbTag ||
     baseStateInfo.shortNameHash == _rollState ||
     baseStateInfo.shortNameHash == _footOnGroundState ||
     baseStateInfo.shortNameHash == _handOnGroundState ||
     baseStateInfo.shortNameHash == _coverLocomotion ||
     baseStateInfo.tagHash == _pivotTag ||
     plAtts.isGunCollidingWall);
        return canAim;
    }
}
