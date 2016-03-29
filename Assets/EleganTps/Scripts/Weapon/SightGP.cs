using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PartHolderIndex))]
public class SightGP : MonoBehaviour
{
    public float cameraZoomAmount = 0; // camera fov decrease (camera zoom increase amount) when aiming
    public float spreadDecreaseAmount;  // decrease amount that will be subtracted from weapon's normal spreadAmount variable
    public float crosshairCenterOnFireDecreaseAmount; // decrease amount that will be subtracted from weapon's 2d crosshair's crosshairCenterIncreaseOnFire variable

}
