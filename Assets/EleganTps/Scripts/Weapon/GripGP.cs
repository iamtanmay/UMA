using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PartHolderIndex))]
public class GripGP : MonoBehaviour
{
    public Transform newLeftHandle;   // new left handle transform that player will hold when this part is attached to a weapon
    public float spreadDecreaseAmount;  // decrease amount that will be subtracted from weapon's normal spreadAmount variable
    public float crosshairCenterOnFireDecreaseAmount; // decrease amount that will be subtracted from weapon's 2d crosshair's crosshairCenterIncreaseOnFire variable
}
