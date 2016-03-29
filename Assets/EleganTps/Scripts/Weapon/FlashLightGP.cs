using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PartHolderIndex))]
public class FlashLightGP : MonoBehaviour
{
    public GameObject lightGo; // light to activate-deactivate
    public GameObject turnOnSound; // turn on sound prefab
    public GameObject turnOffSound; // turn off sound prefab
}
