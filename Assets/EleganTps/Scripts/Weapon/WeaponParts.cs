using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum PartType1 { Null, Flashlight, ExtraClip, SecondaryFire, Grip, Sight, Barrel }

public class WeaponParts : MonoBehaviour
{
    public void AddThisPart(Transform obj)
    {
        Transform newObj = Instantiate(obj, obj.position, obj.rotation) as Transform;
        Vector3 locScale = obj.localScale;
        DeleteSameType(newObj, transform);
        DeleteSameHolder(newObj, transform);
        newObj.SetParent(transform);
        newObj.localScale = locScale; // when parent is set, localscale changes, dont let it
    }



    private string GetThisPartsName(Transform partObjTransform)
    {
        string name = "noname";
        name = partObjTransform.GetComponent<PartHolderIndex>().partName;
        return name;
    }

    private void DeleteSameType(Transform partObjTransform, Transform thisWeapon)
    {
        PartType1 newPartsType = GetPartType(partObjTransform);
        switch (newPartsType)
        {
            case PartType1.Flashlight:
                FlashLightGP[] allChildsFL = thisWeapon.GetComponentsInChildren<FlashLightGP>();
                foreach (FlashLightGP child in allChildsFL)
                {
                    if (!Application.isPlaying)
                        DestroyImmediate(child.gameObject, true);
                    else
                        Destroy(child.gameObject);
                }
                break;

            case PartType1.ExtraClip:
                ExtraClipGp[] allChildsEC = thisWeapon.GetComponentsInChildren<ExtraClipGp>();
                foreach (ExtraClipGp child in allChildsEC)
                {
                    if (!Application.isPlaying)
                        DestroyImmediate(child.gameObject, true);
                    else
                        Destroy(child.gameObject);
                }
                break;

            case PartType1.SecondaryFire:
                SecondaryFireGP[] allChildsSG = thisWeapon.GetComponentsInChildren<SecondaryFireGP>();
                foreach (SecondaryFireGP child in allChildsSG)
                {
                    if (!Application.isPlaying)
                        DestroyImmediate(child.gameObject, true);
                    else
                        Destroy(child.gameObject);
                }

                break;
            case PartType1.Grip:
                GripGP[] allChildsG = thisWeapon.GetComponentsInChildren<GripGP>();
                foreach (GripGP child in allChildsG)
                {
                    if (!Application.isPlaying)
                        DestroyImmediate(child.gameObject, true);
                    else
                        Destroy(child.gameObject);
                }

                break;
            case PartType1.Sight:
                SightGP[] allChildsight = thisWeapon.GetComponentsInChildren<SightGP>();
                foreach (SightGP child in allChildsight)
                {
                    if (!Application.isPlaying)
                        DestroyImmediate(child.gameObject, true);
                    else
                        Destroy(child.gameObject);
                }
                break;
            case PartType1.Barrel:
                BarrelGp[] allChildsBarrel = thisWeapon.GetComponentsInChildren<BarrelGp>();
                foreach (BarrelGp child in allChildsBarrel)
                {
                    if (!Application.isPlaying)
                        DestroyImmediate(child.gameObject, true);
                    else
                        Destroy(child.gameObject);
                }
                break;
            default:
                break;
        }
    }

    private void DeleteSameHolder(Transform partObjTransform, Transform thisWeapon)
    {
        List<Transform> partsOfThisGun = new List<Transform>();
        Transform[] allChildsOfThisGun = thisWeapon.GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildsOfThisGun)
            if (child.GetComponent<ExtraClipGp>() || child.GetComponent<GripGP>() || child.GetComponent<SecondaryFireGP>() || child.GetComponent<FlashLightGP>() || child.GetComponent<SightGP>() || child.GetComponent<BarrelGp>())
                partsOfThisGun.Add(child);

        int index = partObjTransform.GetComponent<PartHolderIndex>().partHolderIndex;
        foreach (Transform child in partsOfThisGun)
            if (child.GetComponent<PartHolderIndex>().partHolderIndex == index)
            {
                if (!Application.isPlaying)
                    DestroyImmediate(child.gameObject, true);
                else
                    Destroy(child.gameObject);

            }

    }

    public PartType1 GetPartType(Transform partObjTransform)
    {
        if (!partObjTransform)
            return PartType1.Null;
        if (partObjTransform.GetComponent<ExtraClipGp>())
        {
            return PartType1.ExtraClip;
        }
        else if (partObjTransform.GetComponent<SecondaryFireGP>())
        {
            return PartType1.SecondaryFire;
        }
        else if (partObjTransform.GetComponent<FlashLightGP>())
        {
            return PartType1.Flashlight;
        }
        else if (partObjTransform.GetComponent<GripGP>())
        {
            return PartType1.Grip;
        }
        else if (partObjTransform.GetComponent<SightGP>())
            return PartType1.Sight;
        else if (partObjTransform.GetComponent<BarrelGp>())
            return PartType1.Barrel;

        return PartType1.Null;
    }

    public FlashLightGP GetFlashLight(Transform go)
    {
        Transform[] allChildsOfThisGun = go.GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildsOfThisGun)
            if (child.GetComponent<FlashLightGP>())
                return child.GetComponent<FlashLightGP>();
        return null;
    }

    public GripGP GetGrip()
    {
        Transform go = transform;
        Transform[] allChildsOfThisGun = go.GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildsOfThisGun)
            if (child.GetComponent<GripGP>())
                return child.GetComponent<GripGP>();
        return null;
    }
    public ExtraClipGp GetExtraClip(Transform go)
    {
        Transform[] allChildsOfThisGun = go.GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildsOfThisGun)
            if (child.GetComponent<ExtraClipGp>())
                return child.GetComponent<ExtraClipGp>();
        return null;
    }
    public SecondaryFireGP GetSecFire()
    {
        Transform go = transform;
        Transform[] allChildsOfThisGun = go.GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildsOfThisGun)
            if (child.GetComponent<SecondaryFireGP>())
                return child.GetComponent<SecondaryFireGP>();
        return null;
    }
    public SightGP GetSight()
    {
        Transform go = transform;
        Transform[] allChildsOfThisGun = go.GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildsOfThisGun)
            if (child.GetComponent<SightGP>())
                return child.GetComponent<SightGP>();
        return null;
    }
    public BarrelGp GetBarrel()
    {
        Transform go = transform;
        Transform[] allChildsOfThisGun = go.GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildsOfThisGun)
            if (child.GetComponent<BarrelGp>())
                return child.GetComponent<BarrelGp>();
        return null;
    }

    public void GetAll(ref SecondaryFireGP secFireGp, ref GripGP gripGp, ref FlashLightGP flashlightGp, ref ExtraClipGp extraclipGP, ref SightGP sightGP, ref BarrelGp barrelGP)
    {
        secFireGp = GetSecFire();
        flashlightGp = GetFlashLight(transform);
        gripGp = GetGrip();
        extraclipGP = GetExtraClip(transform);
        sightGP = GetSight();
        barrelGP = GetBarrel();
    }

    public List<Transform> GetPartsForThisIndexThatPlayerKnows(List<Transform> knownGunParts, int partHolderIndex, ref int curFocusedIndex, ref Transform oldObject) // returns all comptible parts for this weapon and for this index
    {
        GunAtt gunAtt = GetComponent<GunAtt>();
        List<Transform> thisIndexPartsForThisWeapon = new List<Transform>();
        foreach (Transform knownPart in knownGunParts)
        {
            string compatibleWeapon = knownPart.GetComponent<PartHolderIndex>().compatibleWeaponName;
            int holderIndex = knownPart.GetComponent<PartHolderIndex>().partHolderIndex;
            if (compatibleWeapon == gunAtt.weaponName && holderIndex == partHolderIndex)
                thisIndexPartsForThisWeapon.Add(knownPart);
        }

        // sort and find current attached (if there is) part index in thisIndexPartsForThisWeapon and return its index - return -1 if no part is attached
        thisIndexPartsForThisWeapon.OrderBy(part => part.GetComponent<PartHolderIndex>().partName);

        Transform partInThisIndex = GetPartInThisIndex(partHolderIndex);
        if (partInThisIndex)
        {
            for (int i = 0; i < thisIndexPartsForThisWeapon.Count; i++)
            {
                // same name part
                if (thisIndexPartsForThisWeapon[i].GetComponent<PartHolderIndex>().partName == partInThisIndex.GetComponent<PartHolderIndex>().partName)
                {
                    curFocusedIndex = i;
                }
                // set the pard using this index
                oldObject = partInThisIndex;

            }
        }
        else
            curFocusedIndex = -1;

        return thisIndexPartsForThisWeapon;
    }

    private Transform GetPartInThisIndex(int index)
    {
        Transform[] allChildsOfThisGun = gameObject.GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildsOfThisGun)
        {
            if (child.GetComponent<PartHolderIndex>() && child.GetComponent<PartHolderIndex>().partHolderIndex == index)
                return child;
        }
        return null;
    }

    public List<Vector3> GetGunPartPositions()
    {
        GunAtt gunAtt = GetComponent<GunAtt>();
        List<Vector3> list = new List<Vector3>();
        foreach (Transform trnsfrm in gunAtt.gunPartHolders)
            list.Add(trnsfrm.position);
        return list;
    }

    public List<Transform> GetUnKnownPartsInWeapon(List<Transform> knownGunParts, GameObject weapon)
    {
        List<Transform> unKnownParts = new List<Transform>();
        List<Transform> allPartsOfthisWeapon = new List<Transform>();

        // get all parts
        Transform[] allChildsOfThisGun = weapon.GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildsOfThisGun)
            if (child.GetComponent<PartHolderIndex>())
                allPartsOfthisWeapon.Add(child);

        // look for parts we dont know
        for (int i = 0; i < allPartsOfthisWeapon.Count; i++)
        {
            // pick a part from this weapon
            GameObject prefabOfThisPart = allPartsOfthisWeapon[i].GetComponent<PartHolderIndex>().prefabOfThisPart;
            for (int j = 0; j < knownGunParts.Count; j++)
            {
                // pick a part player knows
                GameObject prefabOfKnownPart = knownGunParts[j].GetComponent<PartHolderIndex>().prefabOfThisPart;
                if (prefabOfThisPart == prefabOfKnownPart) // player knows
                {
                    break;
                }
                else if (j == knownGunParts.Count - 1) // player dont know this part
                    unKnownParts.Add(prefabOfThisPart.transform);
            }

        }
        return unKnownParts;
    }

}
