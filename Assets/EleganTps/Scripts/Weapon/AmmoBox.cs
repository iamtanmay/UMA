using UnityEngine;
using System.Collections;


[RequireComponent(typeof(AudioSource))]
public class AmmoBox : MonoBehaviour
{
    SetupAndUserInput userInput;
    AudioSource ammoSound;
    PlayerAtts plAtts;
    public int[] magzInBox = { 100, 100, 100 }; // bullet type 1-2-3

    public void Start()
    {
        userInput = GameObject.FindGameObjectWithTag("Player").GetComponent<SetupAndUserInput>();
        plAtts = userInput.GetComponent<PlayerAtts>();
        ammoSound = GetComponent<AudioSource>();
    }
    public void OnTriggerStay(Collider col)
    {
        if (!col.CompareTag("Player"))
            return;

        if (userInput.UseDown)
        {
            bool gotAmmo = false;
            for (int i = 0; i < plAtts.currentMagz.Length; i++)
            {
                if (magzInBox[i] == 0)
                    continue;
                if (plAtts.currentMagz[i] < plAtts.maxMagz[i] && plAtts.maxMagz[i] - plAtts.currentMagz[i] >= magzInBox[i])
                {
                    plAtts.currentMagz[i] += magzInBox[i];
                    magzInBox[i] = 0;
                    ammoSound.Play();
                    gotAmmo = true;
                }
                else if (plAtts.currentMagz[i] < plAtts.maxMagz[i] && plAtts.maxMagz[i] - plAtts.currentMagz[i] < magzInBox[i])
                {
                    plAtts.currentMagz[i] += plAtts.maxMagz[i] - plAtts.currentMagz[i];
                    magzInBox[i] -= plAtts.maxMagz[i] - plAtts.currentMagz[i];
                    ammoSound.Play();
                    gotAmmo = true;
                }
            }
            if (plAtts.hud && !gotAmmo)
            {
                plAtts.hud.ChangeInfoText("You can't take more ammo from this box.");
            }
        }
    }
}
