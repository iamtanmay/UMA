using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeaponLookAtWeapon : CustomSMB
{
    private PlayerAtts plAtts;
    private TPSCamera tpsCam;
    private List<Transform> partsForThisIndex = new List<Transform>();
    private int curFocusedPartIndex;
    private int curCamFocusedPartHolderIndex, lastCamFocusedPartHolderIndex;
    private GunAtt gunAtt;
    private Transform oldObject;

    public override void Init(Animator anim)
    {
        plAtts = userInput.GetComponent<PlayerAtts>();
        tpsCam = userInput.mainCamera.GetComponent<TPSCamera>();        
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        gunAtt = plAtts.cGun.GetComponent<GunAtt>();

        curCamFocusedPartHolderIndex = tpsCam.focusedPartHolderIndex; lastCamFocusedPartHolderIndex = tpsCam.focusedPartHolderIndex;
        partsForThisIndex.Clear();
        partsForThisIndex = plAtts.cGun.GetComponent<WeaponParts>().GetPartsForThisIndexThatPlayerKnows(plAtts.knownGunParts, curCamFocusedPartHolderIndex, ref curFocusedPartIndex, ref oldObject);
        
        if (partsForThisIndex.Count == -1)
            oldObject = null;
    }


    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        curCamFocusedPartHolderIndex = tpsCam.focusedPartHolderIndex;
        if (lastCamFocusedPartHolderIndex != curCamFocusedPartHolderIndex)
        {
            //partsForThisIndex = plAtts.cGun.GetComponent<WeaponParts>().GetPartsForThisIndexThatPlayerKnows(plAtts.knownGunParts, curCamFocusedPartHolderIndex, ref curFocusedPartIndex, ref oldObject);
            
            //if (partsForThisIndex.Count == -1)
               oldObject = null;

            curCamFocusedPartHolderIndex = tpsCam.focusedPartHolderIndex; lastCamFocusedPartHolderIndex = tpsCam.focusedPartHolderIndex;

            partsForThisIndex.Clear();
            partsForThisIndex = plAtts.cGun.GetComponent<WeaponParts>().GetPartsForThisIndexThatPlayerKnows(plAtts.knownGunParts, curCamFocusedPartHolderIndex, ref curFocusedPartIndex, ref oldObject);

        }

        // curFocusedPartIndex = -1 means no part except clip
        if (tpsCam.camMode != 1)
        {
            animator.SetBool("LookAtWeapon", false);
            return;
        }

        if (userInput.MenuUpDown)
        {
            userInput.MenuUpDown = false;
            curFocusedPartIndex++;
            curFocusedPartIndex = curFocusedPartIndex >= partsForThisIndex.Count ? -1 : curFocusedPartIndex;
            if (curFocusedPartIndex == -1 && curCamFocusedPartHolderIndex == gunAtt.clipHolderIndex)    // dont let empty parts on clip index-we always need a clip...
                curFocusedPartIndex = 0;
            if (curFocusedPartIndex != -1)
            {
                if (oldObject)
                    Destroy(oldObject.gameObject);
                InstantiateAndManageNewPart();
            }
            else
            {
                if (oldObject)
                    Destroy(oldObject.gameObject);
                InstantiateAndManageNewPart();
            }
        }
        else if (userInput.m_MenuDownDown)
        {
            userInput.m_MenuDownDown = false;
            curFocusedPartIndex--;
            curFocusedPartIndex = curFocusedPartIndex < -1 ? partsForThisIndex.Count - 1 : curFocusedPartIndex;
            if (curFocusedPartIndex == -1 && curCamFocusedPartHolderIndex == gunAtt.clipHolderIndex)    // dont let empty parts on clip holder index, we always need a clip...
                curFocusedPartIndex = partsForThisIndex.Count - 1;
            if (curFocusedPartIndex != -1)
            {
                if (oldObject)
                    Destroy(oldObject.gameObject);
                InstantiateAndManageNewPart();
            }
            else
            {
                if (oldObject)
                    Destroy(oldObject.gameObject);
                InstantiateAndManageNewPart();
            }

        }
    }

    private void InstantiateAndManageNewPart()
    {

        if (curFocusedPartIndex != -1)  // not an empty part state
        {
            Vector3 lpos = partsForThisIndex[curFocusedPartIndex].localPosition;
            Quaternion lrot = partsForThisIndex[curFocusedPartIndex].localRotation;
            Vector3 lscale = partsForThisIndex[curFocusedPartIndex].localScale;
            GameObject dummy = new GameObject();
            dummy.transform.parent = plAtts.cGun;
            oldObject = Instantiate(partsForThisIndex[curFocusedPartIndex], new Vector3(0, 0, 0), Quaternion.identity) as Transform;
            oldObject.SetParent(plAtts.cGun);
            plAtts.StartCoroutine(plAtts.FixLocalPosRotScaleOfPart(dummy.transform, oldObject, lpos, lrot, lscale));
            DisplayInfoAboutPart(oldObject);
            if (oldObject.GetComponent<PartHolderIndex>().audioPrefab)
                Instantiate(oldObject.GetComponent<PartHolderIndex>().audioPrefab, gunAtt.transform.position, Quaternion.identity);
        }

        // reset weapon temp vars
        // grip reset
        gunAtt.defaultLeftHandle.SetActive(true);
        if (gunAtt.defaultLeftHandle != gunAtt.leftHandle.gameObject)
            Destroy(gunAtt.leftHandle.gameObject);
        gunAtt.leftHandle = gunAtt.defaultLeftHandle.transform;
        // flashlight reset
        gunAtt.flashLight = null;
        // sight reset 
        gunAtt.sight = null;
        // secFire reset
        gunAtt.secFire = null;
        // barrel reset
        gunAtt.barrel = null;
        // clip can't be null
        plAtts.currentMagz[gunAtt.bulletStyle] += gunAtt.currentClipCapacity;


        // manage after instantiate with coroutine(after new obj is instantiated and old one is destroyed)
        plAtts.StartCoroutine(plAtts.Manage());


    }

    private void DisplayInfoAboutPart(Transform partObj)
    {
        if (plAtts.hud)
        {
            plAtts.hud.ChangeInfoText(partObj.GetComponent<PartHolderIndex>().partInfo, 1);
        }
    }
}
