using UnityEngine;
using System.Collections;

public class PartHolderIndex : MonoBehaviour
{

    public int partHolderIndex;

    [HideInInspector]
    public GameObject prefabOfThisPart; // editor will set this when you add a part, don't show this in inspector
    public string compatibleWeaponName; // comppatible weapon name-for player to find parts compatible with guns (in game) ( need to be same as GunAtt script's weaponName )
    public string partName;
    public string partInfo;
    public GameObject audioPrefab; // audio to be played when this part is instantiated
}

