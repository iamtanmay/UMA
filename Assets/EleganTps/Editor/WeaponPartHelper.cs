using UnityEngine;
using System.Collections;
using UnityEditor;

public enum PartType { Null, Flashlight, ExtraClip, SecFire, Grip, Sight, Barrel }
public class GP
{
    public Transform partObjTrnsfrm;    // instantiated clone for test
}
[CustomEditor(typeof(WeaponParts))]
public class WeaponPartHelper : Editor
{
    public PartType typeToAdd = PartType.ExtraClip;
    GP partToAdd = new GP();
    GameObject partPrefab;
    private bool isPartInstantiated = false;
    //    private Transform newPArtPrefab;

    // this script is used for making guns with gun parts in "editor" - if player picks this gun in play mode or if you add this gun to player not in play mode, player can use this part on another weapon later
    // dont try to add same type part to different partHolder positions (it wont let you) ( eg: you can't use two flashlight (active) on same weapon )
    // if you add a gunpart to a already reserved gunpartholder position script will change it
    // a weapon's gun part is not compatible with another weapon, unless u make another prefab with that one intentionally ( if you duplicate gun part prefab and change its compatible weapon name )

    public override void OnInspectorGUI()
    {
        if (Application.isPlaying)
            return;

        DrawDefaultInspector();
        WeaponParts gunParts = (WeaponParts)target;
        GunAtt gunAtt = gunParts.GetComponent<GunAtt>();

        if (GUILayout.Button("Refresh Scene"))
        {
            EditorUtility.SetDirty(target);
            ResetTempVars();
        }
        GUILayout.Space(5);

        if (IsthereAGunPartAttached(gunAtt))
        {
            if (GUILayout.Button("Remove All Gun Parts"))
            {
                RemoveGunParts(gunAtt);
                isPartInstantiated = false;
                return;
            }
        }
        GUILayout.Space(10);

        if (gunAtt.gunPartHolders.Count < 1)
        {
            Debug.Log("You need to create gun Part holders first");
            return;
        }

        partToAdd.partObjTrnsfrm = EditorGUILayout.ObjectField("Drag gun part prefab here", partToAdd.partObjTrnsfrm, typeof(Transform), true) as Transform;
        if (!partToAdd.partObjTrnsfrm)
            return;

        typeToAdd = GetPartType(partToAdd.partObjTrnsfrm);
        if (typeToAdd == PartType.Null)
        {
            Debug.Log("There is not a gun part script attached to this prefab");
            partToAdd.partObjTrnsfrm = null;
            return;
        }
        else if (!isPartInstantiated)               // INSTANTIATE
        {
            partPrefab = partToAdd.partObjTrnsfrm.gameObject;
            partToAdd.partObjTrnsfrm = Instantiate(partToAdd.partObjTrnsfrm);
            partToAdd.partObjTrnsfrm.SetParent(gunAtt.transform);
            isPartInstantiated = true;
            //newPArtPrefab = partPrefab.transform;
            return;
        }



        partToAdd.partObjTrnsfrm.localRotation = partPrefab.transform.rotation;
        partToAdd.partObjTrnsfrm.localPosition = partPrefab.transform.position;
        partToAdd.partObjTrnsfrm.localScale = partPrefab.transform.localScale;

        //if (GUILayout.Button("Add Gun Part")) // 
        //{
        //    Debug.Log("Weapon Part Attached");
        //    partPrefab.GetComponent<PartHolderIndex>().prefabOfThisPart = partPrefab;
        //    partToAdd.partObjTrnsfrm.GetComponent<PartHolderIndex>().prefabOfThisPart = partPrefab;
        //    gunParts.AddThisPart(partToAdd.partObjTrnsfrm);

        //    ResetTempVars();
        //}


        if (partToAdd.partObjTrnsfrm && isPartInstantiated)
        {
            Debug.Log("Weapon Part Attached");
            partPrefab.GetComponent<PartHolderIndex>().prefabOfThisPart = partPrefab;
            partToAdd.partObjTrnsfrm.GetComponent<PartHolderIndex>().prefabOfThisPart = partPrefab;
            gunParts.AddThisPart(partToAdd.partObjTrnsfrm);

            ResetTempVars();
        }
    }

    private bool IsthereAGunPartAttached(GunAtt gunAtt)
    {
        Transform[] allChildsOfThisGun = gunAtt.GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildsOfThisGun)
            if (child.GetComponent<PartHolderIndex>() && (child.GetComponent<ExtraClipGp>() || child.GetComponent<GripGP>() || child.GetComponent<SecondaryFireGP>() || child.GetComponent<FlashLightGP>() || child.GetComponent<SightGP>() || child.GetComponent<BarrelGp>()))
                return true;

        return false;
    }
    private void RemoveGunParts(GunAtt gunAtt)
    {
        Transform[] allChildsOfThisGun = gunAtt.GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildsOfThisGun)
            if (child != null && (child.GetComponent<ExtraClipGp>() || child.GetComponent<GripGP>() || child.GetComponent<SecondaryFireGP>() || child.GetComponent<FlashLightGP>() || child.GetComponent<SightGP>() || child.GetComponent<BarrelGp>()))
                DestroyImmediate(child.gameObject, true);
    }

    void OnDisable()
    {
        //WeaponParts targObj = target as WeaponParts;

        if (partPrefab != partToAdd.partObjTrnsfrm)
            ResetTempVars();
    }

    private void ShowInfoLabels()
    {
        EditorGUILayout.LabelField("Clicking add will remove same type of parts");
    }

    private void ResetTempVars()
    {
        Transform trnsfrm = partToAdd.partObjTrnsfrm;
        if (partToAdd.partObjTrnsfrm && trnsfrm.Find(partToAdd.partObjTrnsfrm.name)) // if it is not in the hierarchy dont delete it (make sure we dont delete a prefab)
            DestroyImmediate(partToAdd.partObjTrnsfrm.gameObject, true);
        //partPrefab = null;
        typeToAdd = PartType.Null;
        isPartInstantiated = false;
    }

    private PartType GetPartType(Transform partObjTransform)
    {
        if (partObjTransform.GetComponent<ExtraClipGp>())
        {
            return PartType.ExtraClip;
        }
        else if (partObjTransform.GetComponent<SecondaryFireGP>())
        {
            return PartType.SecFire;
        }
        else if (partObjTransform.GetComponent<FlashLightGP>())
        {
            return PartType.Flashlight;
        }
        else if (partObjTransform.GetComponent<GripGP>())
        {
            return PartType.Grip;
        }
        else if (partObjTransform.GetComponent<SightGP>())
        {
            return PartType.Sight;
        }
        else if (partObjTransform.GetComponent<BarrelGp>())
        {
            return PartType.Barrel;
        }

        return PartType.Null;
    }


}
