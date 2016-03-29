using UnityEngine;
using System.Collections;

public class ReloadSMB : CustomSMB
{
    PlayerAtts plAtts;
    Transform leftHandHold;
    private Transform tempNewClip = null;
    public bool debug = false; // set this to true with inspector and fix pos, rot of new clip prefab in left hand with GunAtt script's clip fix values

    public override void Init(Animator anim)
    {
        plAtts = userInput.GetComponent<PlayerAtts>();
        leftHandHold = GameObject.FindGameObjectWithTag("LeftHandHold").transform;

    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        // instantiate new clip in left hand
        if (plAtts.isNewClipInLeftHand && ((plAtts.cGun.GetComponent<GunAtt>().isUsingClipReload && plAtts.cGun.GetComponent<GunAtt>().curClipPrefab) || plAtts.cGun.GetComponent<GunAtt>().firesCurClipObject) /* && !plAtts.cGun.GetComponent<GunAtt>().isFiringProjectile*/)
        {
            plAtts.isNewClipInLeftHand = false;
            GunAtt gunAtt = plAtts.cGun.GetComponent<GunAtt>();

            // instantiate new clip
            tempNewClip = Instantiate(gunAtt.curClipPrefab);
            tempNewClip.SetParent(leftHandHold);
            leftHandHold.localPosition = gunAtt.lHandClipPosFix;
            leftHandHold.localRotation = Quaternion.Euler(gunAtt.lHandClipRotFix);

            tempNewClip.GetComponent<Rigidbody>().isKinematic = true;
            tempNewClip.GetComponent<Collider>().enabled = false;

            plAtts.StartCoroutine(plAtts.FixClipInLeftHand(tempNewClip));

            if (debug)
                Time.timeScale = .002f;
            return;
        }

        // new clip goes to weapon
        if (tempNewClip && plAtts.isNewClipOffLeftHand)
        {
            plAtts.isNewClipOffLeftHand = false;
            //if (debug)
            //    Time.timeScale = 1;

            GunAtt gunAtt = plAtts.cGun.GetComponent<GunAtt>();
            tempNewClip.SetParent(gunAtt.transform);
            tempNewClip.localPosition = gunAtt.clipDefLocalPos;
            tempNewClip.localRotation = gunAtt.clipDefLocalRot;

            gunAtt.curClipObject = tempNewClip;
        }

        // you can fix newclip position-rotation in left hand with this code
        if (tempNewClip)
        {
            GunAtt gunAtt = plAtts.cGun.GetComponent<GunAtt>();
            leftHandHold.localPosition = gunAtt.lHandClipPosFix;
            leftHandHold.localRotation = Quaternion.Euler(gunAtt.lHandClipRotFix);
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        tempNewClip = null;
        //if (debug)
        //    Time.timeScale = 1;

    }

}
