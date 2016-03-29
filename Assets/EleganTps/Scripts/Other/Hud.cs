using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
[System.Serializable]
public class ObjectsAlign
{
    public Transform objT;
    public Vector3 position;

    ObjectsAlign()
    {
        position = new Vector3(.2f, .3f, 190f);
    }
}
public class Hud : MonoBehaviour
{

    public Camera cam;
    public List<ObjectsAlign> objects;
    public Text infoText;
    public Image infoBackGround;
    public Text totalAmmoText;
    public Text currentClipAmmoText;
    public Text secondaryFireTotalAmmoText;
    public Image weaponImg;
    public Sprite emptyWeaponSprite;
    public float zoomFocusItemAmount = 4f;

    private Vector3 weaponHudImgTestedLocalPosition = new Vector3(-.9f, 15f, 0f);
    private float targetInfoAlpha = 0, targetInfoAlphaV, curInfoAlpha = 0;
    private float targetMenuAlpha = 0, targetMenuAlphaV, curMenuAlpha = 0;
    PlayerAtts plAtts;
    private float appearTimeInfo;
    private bool transiting;
    public List<GameObject> itemMenuGos;
    [HideInInspector]
    public MenuItemHolder clickedMenuItem = null;

    private Color defButonColor;
    private SetupAndUserInput userInput;
#if !MOBILE_INPUT
    private int chooseIndex;

    private float defItemSizeD;
#endif
    //private GameObject weaponHudIndicatorGo;
    void Start()
    {
        plAtts = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerAtts>();
        userInput = plAtts.GetComponent<SetupAndUserInput>();
        ChooseAWeaponSpriteOnStart(); // weapon hud bot right
        ManageMenuOnStartup();  // item menu
        //transform.FindChild("Menu").gameObject.SetActive(false);
#if !MOBILE_INPUT
        if (itemMenuGos.Count > 0)
            defItemSizeD = itemMenuGos[0].GetComponent<RectTransform>().sizeDelta.x;
#endif
        // align objects to edge of screen
        if (!cam)
            return;
        if (objects.Count <= 0)
            return;
        foreach (ObjectsAlign obj in objects)
        {
            obj.objT.position = cam.ViewportToWorldPoint(obj.position);
        }

        defButonColor = itemMenuGos[0].GetComponent<Image>().color;
    }


    void Update()
    {
        if (!cam)
            return;

        // info text
        curInfoAlpha = Mathf.SmoothDamp(curInfoAlpha, targetInfoAlpha, ref targetInfoAlphaV, 15f * Time.deltaTime);
        if (curInfoAlpha > .99f)
        {
            StartCoroutine(FadeOutInfo(appearTimeInfo));
            targetInfoAlpha = .98f;
        }
        infoText.color = new Color(infoText.color.r, infoText.color.g, infoText.color.b, curInfoAlpha);
        infoBackGround.color = new Color(infoBackGround.color.r, infoBackGround.color.g, infoBackGround.color.b, curInfoAlpha);


        // item Menu
        curMenuAlpha = Mathf.SmoothDamp(curMenuAlpha, targetMenuAlpha, ref targetMenuAlphaV, 15 * Time.deltaTime);
        ChangeAlphaOfMenu();
        if (userInput.MenuDown || menuTouch)
        {
#if MOBILE_INPUT
            menuTouch = false;
#endif
            if (curMenuAlpha < .01f)
            {
                targetMenuAlpha = 1;
            }
            else if (curMenuAlpha > .09f && !transiting)
            {
                targetMenuAlpha = 0; transiting = true;
            }
        }
        if (curMenuAlpha < .01f && transiting && transform.FindChild("Menu").gameObject.activeSelf)
        {
            transiting = false;
        }

        // choose item
        if (transform.FindChild("Menu").gameObject.activeSelf && curMenuAlpha > .88f)
        {
#if !MOBILE_INPUT
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetAxisRaw("Mouse ScrollWheel") < 0) // next item
            {
                chooseIndex++;
                if (chooseIndex == itemMenuGos.Count)
                    chooseIndex = 0;
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetAxisRaw("Mouse ScrollWheel") > 0) // prev item
            {
                chooseIndex--;
                if (chooseIndex < 0)
                    chooseIndex = itemMenuGos.Count - 1;
            }
            for (int i = 0; i < itemMenuGos.Count; i++)
            {
                if (i == chooseIndex)
                    itemMenuGos[chooseIndex].GetComponent<RectTransform>().sizeDelta = new Vector2(defItemSizeD + zoomFocusItemAmount, defItemSizeD + zoomFocusItemAmount);
                else
                    itemMenuGos[i].GetComponent<RectTransform>().sizeDelta = new Vector2(defItemSizeD, defItemSizeD);
            }
            if (Input.GetKeyDown(KeyCode.Mouse2))
            {
                if (itemMenuGos[chooseIndex].GetComponent<MenuItemHolder>().go)
                {
                    clickedMenuItem = itemMenuGos[chooseIndex].GetComponent<MenuItemHolder>();
                    if (plAtts.cGun && plAtts.cGun == clickedMenuItem.go.transform || plAtts.GetComponent<Animator>().GetCurrentAnimatorStateInfo(1).IsTag("Draw") || plAtts.GetComponent<Animator>().IsInTransition(1)) // if it is the same weapon, don't bother
                        clickedMenuItem = null;
                }
            }
#else
            if (touchedItem)
            {
                if (touchedItem.GetComponent<MenuItemHolder>().go)
                {
                    clickedMenuItem = touchedItem.GetComponent<MenuItemHolder>();
                    if (plAtts.cGun && plAtts.cGun == clickedMenuItem.go.transform || plAtts.GetComponent<Animator>().GetCurrentAnimatorStateInfo(1).IsTag("Draw") || plAtts.GetComponent<Animator>().IsInTransition(1)) // if it is the same weapon, don't bother
                        clickedMenuItem = null;
                }
                touchedItem = null;
            }

#endif

        }
        if (!transform.FindChild("Menu").gameObject.activeSelf)
        {
            clickedMenuItem = null;
#if !MOBILE_INPUT
            itemMenuGos[chooseIndex].GetComponent<RectTransform>().sizeDelta = new Vector2(defItemSizeD + zoomFocusItemAmount, defItemSizeD + zoomFocusItemAmount);
#endif
        }

        // empty bullet indicator hud if no gun
        if (plAtts.Guns.Count <= 0)
        {
            EmptyBulletIndicator();
        }
        else
        {
            //if (plAtts.lastGun)   // no need for now
            //{
            //    GunAtt gunAtt = plAtts.lastGun.GetComponent<GunAtt>();
            //    ChangeWeaponImg(gunAtt.hudImage);
            //    PrintAmmoText(gunAtt.currentClipCapacity);
            //    PrintAmmoText(plAtts.currentMagz[]);
            //}

        }

    }
    private bool menuTouch = false;
    [System.NonSerialized]
    public GameObject touchedItem;
#if MOBILE_INPUT
    public void IndicatorTouch()
    {
        menuTouch = true;
    }
    public void ItemTouch(int itemIndex)
    {
        touchedItem = itemMenuGos[itemIndex];
    }
#endif

    private void ChangeAlphaOfMenu()
    {

        Color color = transform.FindChild("Menu").FindChild("BackGround").GetComponent<Image>().color;
        transform.FindChild("Menu").FindChild("BackGround").GetComponent<Image>().color = new Color(color.r, color.g, color.b, curMenuAlpha);
        foreach (GameObject menuGo in itemMenuGos)
        {
            color = menuGo.GetComponent<Image>().color;
            menuGo.GetComponent<Image>().color = new Color(color.r, color.g, color.b, curMenuAlpha);
            color = menuGo.transform.FindChild("Text").GetComponent<Text>().color;
            menuGo.transform.FindChild("Text").GetComponent<Text>().color = new Color(color.r, color.g, color.b, curMenuAlpha);
        }
    }

    private void ManageMenuOnStartup()
    {
        int i = 0;
        foreach (GameObject goItem in itemMenuGos)
        {
            if (i < plAtts.Guns.Count && plAtts.Guns[i])
            {
                goItem.GetComponent<Image>().sprite = plAtts.Guns[i].GetComponent<GunAtt>().hudImage;
                goItem.transform.FindChild("Text").GetComponent<Text>().text = plAtts.Guns[i].GetComponent<GunAtt>().weaponName;
                goItem.GetComponent<MenuItemHolder>().go = plAtts.Guns[i];
            }
            else
            {
                Color color = goItem.GetComponent<Image>().color;
                color = new Color(color.r, color.g, color.b, .2f);
                goItem.GetComponent<Image>().color = color;
                goItem.GetComponent<Image>().sprite = emptyWeaponSprite;
                goItem.transform.FindChild("Text").GetComponent<Text>().text = "";
            }
            i++;
        }
    }

    void EmptyBulletIndicator()
    {
        ChangeWeaponImg(emptyWeaponSprite);
        PrintAmmoText(0);
        PrintAmmoText(0, false);
    }

    public int AddGoToMenu(GameObject goToAdd) // goToAdd needs to have GunAtt script
    {
        foreach (GameObject goItem in itemMenuGos)
        {
            if (goItem.GetComponent<MenuItemHolder>().go == null)
            {
                GunAtt gunAtt = goToAdd.GetComponent<GunAtt>();

                // change sprite of this item
                goItem.GetComponent<Image>().sprite = gunAtt.hudImage;
                goItem.GetComponent<MenuItemHolder>().go = goToAdd;
                goItem.transform.FindChild("Text").GetComponent<Text>().text = gunAtt.weaponName;
                return 0;
            }
        }
        ChangeInfoText("No empty slot");
        return -1;
    }
    public void DeleteFromGoMenu(GameObject goToDelete)
    {
        foreach (GameObject goItem in itemMenuGos)
        {
            if (goItem.GetComponent<MenuItemHolder>().go != null && goItem.GetComponent<MenuItemHolder>().go == goToDelete)
            {
                goItem.GetComponent<Image>().sprite = emptyWeaponSprite;
                goItem.GetComponent<Image>().color = defButonColor;
                goItem.GetComponent<MenuItemHolder>().go = null;
                goItem.transform.FindChild("Text").GetComponent<Text>().text = "";

                return;
            }
        }
    }

    public IEnumerator FadeOutInfo(float secs)
    {
        yield return new WaitForSeconds(secs);
        targetInfoAlpha = 0;
    }

    public void ChangeInfoText(string text, float appearTime = 2f)
    {
        if (clickedMenuItem)
            return;
        infoText.text = text;
        targetInfoAlpha = 1;
        appearTimeInfo = appearTime;
    }

    public void ChooseAWeaponSpriteOnStart()
    {
        if (!plAtts)
            return;
        if (plAtts.Guns.Count <= 0)
        {
            weaponImg.sprite = emptyWeaponSprite;
            weaponImg.rectTransform.localPosition = weaponHudImgTestedLocalPosition;
            //weaponHudIndicatorGo = null;
        }
        else if (plAtts.lastGun)
        {
            weaponImg.sprite = plAtts.lastGun.GetComponent<GunAtt>().hudImage;
            weaponImg.rectTransform.localPosition = weaponHudImgTestedLocalPosition;
            if (!plAtts.lastGun.GetComponent<WeaponParts>())
            {
                PrintAmmoText(plAtts.lastGun.GetComponent<GunAtt>().currentClipCapacity);
                PrintAmmoText(plAtts.currentMagz[plAtts.lastGun.GetComponent<GunAtt>().bulletStyle], false);
                //weaponHudIndicatorGo = plAtts.lastGun.gameObject;
            }
            else
            {
                PrintAmmoText(plAtts.lastGun.GetComponent<GunAtt>().currentClipCapacity);
                PrintAmmoText(plAtts.currentMagz[plAtts.lastGun.GetComponent<GunAtt>().bulletStyle], false);
            }

        }
        else
        {
            foreach (GameObject go in plAtts.Guns)
            {
                if (go == null || go.GetComponent<GunAtt>().isMelee)
                    continue;
                else
                {
                    weaponImg.sprite = go.GetComponent<GunAtt>().hudImage;
                    weaponImg.rectTransform.localPosition = weaponHudImgTestedLocalPosition;
                    PrintAmmoText(go.GetComponent<GunAtt>().currentClipCapacity);
                    PrintAmmoText(plAtts.currentMagz[go.GetComponent<GunAtt>().bulletStyle], false);
                    //weaponHudIndicatorGo = go;
                    break;
                }
            }
        }
    }

    public void ChangeWeaponImg(Sprite sprite)
    {
        weaponImg.sprite = sprite;
        weaponImg.rectTransform.localPosition = weaponHudImgTestedLocalPosition;
    }

    public void PrintAmmoText(float count, bool isCurrentAmmo = true)
    {
        if (isCurrentAmmo)
        {
            if (count < 10)
                currentClipAmmoText.text = "00" + count;
            else if (count > 99)
                currentClipAmmoText.text = "" + count;
            else
                currentClipAmmoText.text = "0" + count;
        }
        else
        {
            if (count < 10)
                totalAmmoText.text = "00" + count;
            else if (count > 99)
                totalAmmoText.text = "" + count;
            else
                totalAmmoText.text = "0" + count;
        }
    }

    public void PrintSecondaryAmmoText(float count)
    {
        if (count < 10)
            secondaryFireTotalAmmoText.text = "0" + count;
        else
            secondaryFireTotalAmmoText.text = "" + count;
    }



    public void OnDrawGizmos()
    {

        if (!cam)
            return;
        if (objects.Count <= 0)
            return;
        foreach (ObjectsAlign obj in objects)
        {
            // uncomment if u want to debug hud objects' positions in scene editor
            obj.objT.position = cam.ViewportToWorldPoint(obj.position); // this just makes it compatible for all screen resolutions
        }

    }


}
