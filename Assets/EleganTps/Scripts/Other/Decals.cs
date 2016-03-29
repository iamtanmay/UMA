using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class DecalAndFx
{
    public string tagName;
    public List<GameObject> decals;
    public List<GameObject> fx;
    public float piercingPowerDecrease;
    public List<GameObject> hitSounds;
}

public class Decals : MonoBehaviour
{

    public List<DecalAndFx> tags;

    public GameObject getRandomDecal(string tag)
    {
        DecalAndFx list = tags.Find(x => x.tagName == tag);
        if (list == null || list.fx.Count == 0)
            return null;
        return list.fx[Random.Range(0, list.fx.Count)];

    }
    public GameObject getRandomFx(string tag)
    {
        DecalAndFx list = tags.Find(x => x.tagName == tag);
        if (list == null || list.decals.Count == 0)
            return null;
        return list.decals[Random.Range(0, list.decals.Count)];
    }
    public float getBulletPower(string tag)
    {
        DecalAndFx list = tags.Find(x => x.tagName == tag);
        if (list == null || list.decals.Count == 0)
            return 0;
        return list.piercingPowerDecrease;
    }

    public void getAllNormalShot(string tag, ref GameObject decal, ref GameObject fx, ref GameObject hitSound, ref float piercePower)
    {
        DecalAndFx list = tags.Find(x => x.tagName == tag);


        if (list == null || list.fx.Count == 0)
            fx = null;
        else
            fx = list.fx[Random.Range(0, list.fx.Count)];

        if (list == null || list.decals.Count == 0)
            decal = null;
        else
            decal = list.decals[Random.Range(0, list.decals.Count)];

        if (list == null || list.hitSounds.Count == 0)
            hitSound = null;
        else
            hitSound = list.hitSounds[Random.Range(0, list.hitSounds.Count)];

        if (list == null)
            piercePower = 0;
        else
            piercePower = list.piercingPowerDecrease;

    }
}
