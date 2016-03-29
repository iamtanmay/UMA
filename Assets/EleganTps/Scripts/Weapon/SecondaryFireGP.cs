using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PartHolderIndex))]
public class SecondaryFireGP : MonoBehaviour
{

    //float damage; // not used in this version
    public float spreadAmount = 4;
    public float fireSpeed = 2;
    public int bulletStyle;
    public float bulletPower = .1f;

    public bool isShotGun = false;
    public int sharapnelCount; // bullet per shot
    public float maxBulletDistance;
    public float maxSharapnelSpread; // per shot sharapnel spread amount
    public float crosshairCenterIncreaseOnFire = 5;
    public float bodyBobAmount = .6f;

    public GameObject muzzleFlash;
    public float muzzleFlashTime = .15f;
    public GameObject fireSound;

    public bool firingProjectile = false; // set this to true if this is projectile launcher type of secondary fire
    public GameObject projectilePrefab; // projectile to instantiate new on projectilePosition
    public Transform projectilePosition;
    public float projectileForce = 30;

}
