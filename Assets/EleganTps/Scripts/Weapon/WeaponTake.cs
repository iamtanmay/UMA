using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]
public class WeaponTake : MonoBehaviour
{
    SetupAndUserInput userInput;
    PlayerAtts plAtts;
    Animator anim;

    void Start()
    {
        userInput = GameObject.FindGameObjectWithTag("Player").GetComponent<SetupAndUserInput>();
        plAtts = userInput.GetComponent<PlayerAtts>();
        anim = plAtts.GetComponent<Animator>();
    }

    void OnTriggerExit(Collider col)
    {
        if (!col.CompareTag("Player"))
            return;
        plAtts.weaponsPickable.Remove(gameObject);
    }


    void OnTriggerStay(Collider col)
    {
        if (!col.CompareTag("Player"))
            return;

        AnimatorStateInfo _sI0 = anim.GetCurrentAnimatorStateInfo(0);
        AnimatorStateInfo _sI1 = anim.GetCurrentAnimatorStateInfo(1);
        if (!((_sI0.IsName("Locomotion") || _sI0.IsName("CrouchLocomotion")) && ((_sI1.IsTag("LowIdle") && plAtts.cGun) || !plAtts.cGun)))
            return;

        if (!plAtts.weaponsPickable.Contains(gameObject))
            plAtts.weaponsPickable.Add(gameObject);
        if (plAtts.GetClosestPickable() != gameObject)
            return;

        // debug grabbing mid-top-bot positions
        //Debug.DrawRay(plAtts.transform.position + new Vector3(0, plAtts.grabMidCenterY + plAtts.grabUpDownDist, 0), plAtts.transform.forward, Color.green);
        //Debug.DrawRay(plAtts.transform.position + new Vector3(0, plAtts.grabMidCenterY - plAtts.grabUpDownDist, 0), plAtts.transform.forward, Color.green);
        //Debug.DrawRay(plAtts.transform.position + new Vector3(0, plAtts.grabMidCenterY, 0), plAtts.transform.forward, Color.yellow);

        if (plAtts.useReachToGrab)
        {
            // don't reach to grab weapon if weapon is player's behind
            Vector3 gunModelCenter = transform.TransformPoint(GetComponent<BoxCollider>().center);
            float angleBetween = Vector3.Angle(plAtts.transform.forward, new Vector3(gunModelCenter.x, plAtts.transform.position.y, gunModelCenter.z) - plAtts.transform.position);
            if (plAtts.useReachToGrab && angleBetween > 90)
                return;
            plAtts.itemFocusPos = gunModelCenter;
        }

        plAtts.hud.ChangeInfoText("Press Use button to take " + GetComponent<GunAtt>().weaponName, .2f);



        if (userInput.UseDown)
        {

            if (plAtts.hud.AddGoToMenu(gameObject) != -1)
            {
                plAtts.Guns.Add(gameObject);
                plAtts.weaponsPickable.Remove(gameObject);
                if (gameObject.GetComponent<GunAtt>())
                {
                    //
                    if (plAtts.useReachToGrab)
                    {
                        userInput.movementInputEnabled = false;
                        plAtts.itemToTake = transform;
                        float posYofWeapon = transform.position.y;
                        float upGrabMidLimitY = plAtts.transform.position.y + plAtts.grabMidCenterY + plAtts.grabUpDownDist;
                        float downGrabMidLimitY = plAtts.transform.position.y + plAtts.grabMidCenterY - plAtts.grabUpDownDist;
                        if (posYofWeapon < downGrabMidLimitY)
                            anim.SetInteger("Grab", 1);
                        else if (posYofWeapon > upGrabMidLimitY)
                            anim.SetInteger("Grab", 3);
                        else
                            anim.SetInteger("Grab", 2);
                        gameObject.GetComponent<Rigidbody>().isKinematic = true;
                        gameObject.GetComponent<BoxCollider>().enabled = false;
                        gameObject.GetComponent<SphereCollider>().enabled = false;
                        gameObject.transform.position = new Vector3(0, -300, 0);
                    }
                    else
                    {
                        gameObject.GetComponent<Rigidbody>().isKinematic = true;
                        gameObject.GetComponent<BoxCollider>().enabled = false;
                        gameObject.GetComponent<SphereCollider>().enabled = false;
                        gameObject.transform.position = new Vector3(0, -300, 0);
                    }
                }
            }

            // player learns parts
            if (plAtts.canModifyWeaponParts && GetComponent<WeaponParts>())
            {
                plAtts.CheckForPartsUnknown();
            }

        }
    }
}
