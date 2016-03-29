using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(PartHolderIndex))]
public class BarrelGp : MonoBehaviour
{
    public float bulletPowerIncrease;   // bulletPowerWillBeIncreased
    public List<GameObject> fireSounds; // (Set this list's size to 0 if you want to use normal gunshot sounds)if this part is attached to a gun this shot sounds will be played instead of default shot sounds
    //public float damageIncrease;  // not used in this version
}
