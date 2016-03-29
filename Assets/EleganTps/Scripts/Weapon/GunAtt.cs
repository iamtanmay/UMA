using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

[System.Serializable]
public class Sounds
{
    public List<GameObject> fireSounds;
    public GameObject dry, reload, holster, draw;
}

public class GunAtt : MonoBehaviour
{
    public string weaponName = "";   // This weapon's name
    public float fireSpeed = 10;   // This weapon's fire speed
    public float spreadAmount = 4;   // This weapon's spread amount
    public float additionalSpreadNoAimAmount = 4f;
    public float speedSpreadAmount = .5f;   // This weapon's additional spread amount when moving
    public float spreadRecoverSpeed = .08f;   // This weapon's spread overall spread recover speed
    public Vector3 spreadAxisMultipliers;   // Weapon rotation per shot(effected by spreadAmount) x:left,right / y:up,down / z:hand twist
    public float bodySpread = 1f;   // How much body will be effected with a single shot(effected by spreadAmount)
    public float bodyRecoverSpeedInverse = 55f;  // Body's returning aim position speed (inverse means:if this variable decreased body will recover faster)
    public float bodyFixRight = 5.38f;  // body lookfix right from Target(main camera's child) when holding gun
    public float bodyFixUp = -3.8f;  // body lookfix right from Target when holding gun
    public int currentClipCapacity;   // weapon current clip capacity
    public int maxClipCapacity;   // weapon's maximum bullet capacity
    public int gunStyle;   // 1, 2 or 3 ( Pistol - Rifle - Rocket Launcher ) // for animator Torso layer  (dont use weapon parts with gun style set to 3, it is not tested yet)
    public int bulletStyle;   //  which ammo type this weapon will decrease from ammunition of player
    public float maxBulletDistance = 200f;   // max distance this weapon's bullet goes
    public float bulletPower = .1f;  // can this weapon's bullet pierce so many walls?
    public float bulletForceToRigidbodys = 30f;   // how much force this weapon's bullet does to any rigidbody it hits
    public bool canSprintOnHand = true;   // If this is true player will be able to sprint when holding this weapon
                                          //public bool isUsingLeftHand = true;   // not currently available

    public bool isShotGun = false;   // if this is shotgun type weapon - shrapnel fire
    public int shrapnelCount = 6;   // how much bullet(shrapnel) this shotgun fires per single shot
    public float maxshrapnelSpread = 3f;   // shrapnel spread amount

    public bool isFiringProjectile = false;   // is this weapon fires projectiles like rocket launchers
    public float projectileFireForce;   // projectile's exit power ( projectile will go further if you increase this )
    public bool firesCurClipObject = false;   // fires reloaded clip (curClipObject must be set) Rpg type rocket firing
                                              //public bool firesInstantiatedProjectile = false; // grenade launcher type firing      // not used atm(Grenade Launcher type weapon)
                                              //public GameObject projectile; // projectile gameObject if firesInstantiatedProjectile = true      // not used atm(Grenade Launcher type weapon)
                                              //public Transform projectileInstantiatePosition; // projectile Instantiate position if firesInstantiatedProjectile = true  // not used atm(Grenade Launcher type weapon)

    public Sprite crosshairSprite;  // weapon crosshair sprite
    public float crosshairCenterMinSpace = 5f;   // crosshair stick's mid space from screen's center
    public float crosshairCenterMaxSpace = 26f;   // crosshair stick's max space from screen's center
    public float crosshairCenterIncreaseOnFire = 8f;   // amount to increase center space when shoot
    public float crosshairBackSpeed = 7f;   // amount of time to recover from increased center space // lower = faster
    public float crosshairMoveEffector = 15f;   // amount of space when player is running
    public float bulletSpeed = 600;   // You don't have to chage this - used to calculate the time amount to Instantiate effects-logics etc... (600 means 600 unit per sec)
    public Sprite hudImage;   // weapon's 3d hud screen image
    public GameObject muzzleFlash;   // muzzle flash object for this weapon
    public float muzzleFlashTimePerBullet = .16f;   // muzzleflash's lifetime per shot
    public Transform onAimPositionRotation;   // weapon position rotation for this weapon when aiming with right click
    public Transform onNoAimPositionRotation;   // weapon position rotation for this weapon when not aiming with right click, only firing with left click
    public Transform leftHandle;   // left handle position rotation for this weapon
    public Transform rightHandPositionRotation;   // weapon in right hand position rotation
    public Sounds sounds;   // Weapon sounds
    public bool IsAnimated;   // ignore this if you dont want animation or you have your own animation system
    public Animator animatorObjectIfAnimated;   // ignore this if you dont want animation or you have your own animation system
    public GameObject EmptyShellPrefab;   // Empty shell to be instantiated on emptyshell position when firing
    public Transform emptyShellPosition;   // Epmty shell instantiate position
    public Vector3 emptyShellMinForce;
    public Vector3 emptyShellMaxForce;   // empty shell max exit force on instantiate
    public GameObject bulletTrailPrefab;   // trail prefab to Instantiate
    public Transform bulletTrailExitPosition;   // trail Instantiate position
    public float bulletTrailExitForce = 450;   // trail Exit force if it has rigidbody(this variable shouldnt be too low to look real)

    public Transform curClipObject;   // if you use clip prefabs or rpg type weapons that fires current clip set this object(prefab)
    public Transform curClipPrefab;   // weapons current clip is need to be set to use new clip prefabs on reload or on fire for rpg type weapons
    //public Transform clipPosition;   // left hand will be here in last seconds of reload animation ( not used atm )
    public float droppedClipDestroyTime = 20f;   // After reload, dropped clip object will be destroyed after this time
    public Vector3 lHandClipPosFix;   // If you use reloading with a prefab in left hand u can fix its position with this value(set debug to true in weapon layer, reload animation's ReloadSMB)
    public Vector3 lHandClipRotFix;   // If you use reloading with a prefab in left hand u can fix its rotation with this value(set debug to true in weapon layer, reload animation's ReloadSMB)
    public List<Transform> gunPartHolders;   // Camera will be looking at these positions if you use modifiable weapon
    public int clipHolderIndex = -99;   // if you want to use modifiable clips, set this according to your part holder positions ( in clip partholder position, dont use another part ) // we can't have empty clip part
    public bool isMelee = false;   // Is this a melee weapon(melee weapon's are still in development, they may be using another script later)

    #region Hidden system data
    [System.NonSerialized]
    public GameObject defaultLeftHandle;
    [System.NonSerialized]
    public GameObject sight;
    [System.NonSerialized]
    public GameObject secFire;
    [System.NonSerialized]
    public GameObject flashLight;
    [System.NonSerialized]
    public GameObject grip;
    [System.NonSerialized]
    public GameObject barrel;
    [System.NonSerialized]
    public Vector3 clipDefLocalPos;
    [System.NonSerialized]
    public Quaternion clipDefLocalRot;
    [System.NonSerialized]
    public bool isUsingClipReload = false;
    [System.NonSerialized]
    public GameObject hasProjectile;
    [HideInInspector] // code numbers start no
    private static int codeNumberStart = 100;
    [HideInInspector] // code number for this partic. gun 
    public int codeNumber;
    #endregion

    void Awake()
    {
        codeNumber++;
        codeNumber = ++codeNumberStart;
        ManagePartsStart();
        if (curClipObject && !isFiringProjectile && GetComponent<WeaponParts>())
        {
            isUsingClipReload = true;
            curClipPrefab = curClipObject.GetComponent<PartHolderIndex>().prefabOfThisPart.transform;
        }
        else if (curClipObject && !isFiringProjectile)
        {
            isUsingClipReload = true;
        }
        if (isFiringProjectile && firesCurClipObject)
        {
            currentClipCapacity = 1;
            maxClipCapacity = 1;
        }

    }
    private void ManagePartsStart()
    {
        if (GetComponent<WeaponParts>())    // Manage some weapon parts on start if weapon parts are used in this weapon
        {
            // Extra clip Logic
            ExtraClipGp extraClip = GetComponent<WeaponParts>().GetExtraClip(transform);
            if (extraClip)
            {
                curClipObject = extraClip.transform;
                maxClipCapacity = extraClip.clipCapacity;
                if (currentClipCapacity > maxClipCapacity)
                    currentClipCapacity = maxClipCapacity;
            }

            // Grip Logic
            GripGP gripGp = GetComponent<WeaponParts>().GetGrip();
            defaultLeftHandle = this.leftHandle.gameObject;
            if (gripGp)
            {
                grip = gripGp.gameObject;
                if (gripGp.newLeftHandle)
                {
                    defaultLeftHandle = leftHandle.gameObject;
                    leftHandle = Instantiate(gripGp.newLeftHandle);
                    leftHandle.SetParent(transform);
                    leftHandle.localPosition = gripGp.newLeftHandle.localPosition;
                    leftHandle.localRotation = gripGp.newLeftHandle.localRotation;
                    defaultLeftHandle.SetActive(false);
                }
            }

            // Sight Logic
            SightGP sightGp = GetComponent<WeaponParts>().GetSight();
            if (sightGp)
            {
                sight = sightGp.gameObject;
            }
            else
                sight = null;

            // Flashlight Logic
            FlashLightGP flashlightGP = GetComponent<WeaponParts>().GetFlashLight(transform);
            if (flashlightGP)
            {
                flashLight = flashlightGP.gameObject;
            }
            else
                flashLight = null;

            // SecondFire Logic
            SecondaryFireGP secfire = GetComponent<WeaponParts>().GetSecFire();
            if (secfire)
                secFire = secfire.gameObject;
            else
                secfire = null;

            // Barrel Logic
            BarrelGp barrelGp = GetComponent<WeaponParts>().GetBarrel();
            if (barrelGp)
                barrel = barrelGp.gameObject;
            else
                barrel = null;
        }
    }


}
