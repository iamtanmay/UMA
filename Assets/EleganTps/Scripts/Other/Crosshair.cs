using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
    public float crosshairFadeSpeed = .3f;
    public float crosshairSize = 10f;
    PlayerAtts plAtts;
    Image ch1, ch2, ch3, ch4;
    GunAtt gunAtt;
    Animator animator;
    private float targetSpaceV;
    [HideInInspector]
    public float currentSpace = 1f;
    private float alpha, alphaV;
    [HideInInspector]
    public bool chExists = false;

    void Start()
    {
        plAtts = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerAtts>();
        animator = plAtts.GetComponent<Animator>();
        InstantiateChs();

    }

    void Update()
    {
        if (plAtts.cGun)
        {
            gunAtt = plAtts.cGun.GetComponent<GunAtt>();

            float moveEffector = 0;
            if (animator.GetFloat("Speed") > .1f || animator.GetCurrentAnimatorStateInfo(0).IsTag("Midair") || animator.IsInTransition(1))
                moveEffector = gunAtt.crosshairMoveEffector * animator.GetFloat("Speed");
            else
                moveEffector = 0;

            currentSpace = Mathf.SmoothDamp(currentSpace, gunAtt.crosshairCenterMinSpace + moveEffector, ref targetSpaceV, Time.deltaTime * gunAtt.crosshairBackSpeed);
            if (currentSpace > gunAtt.crosshairCenterMaxSpace)
                currentSpace = gunAtt.crosshairCenterMaxSpace;

            if (plAtts.isAiming)
                FadeIn();
            else
                FadeOut();

            PositionChs();
        }

    }

    void FadeIn()
    {
        alpha = Mathf.SmoothDamp(alpha, 1, ref alphaV, Time.deltaTime * crosshairFadeSpeed);
        Color color = ch1.color;
        color = new Color(color.r, color.g, color.b, alpha);
        ch1.color = color;
        ch2.color = color;
        ch3.color = color;
        ch4.color = color;
    }
    void FadeOut()
    {
        alpha = Mathf.SmoothDamp(alpha, 0, ref alphaV, Time.deltaTime * crosshairFadeSpeed);
        Color color = ch1.color;
        color = new Color(color.r, color.g, color.b, alpha);
        ch1.color = color;
        ch2.color = color;
        ch3.color = color;
        ch4.color = color;
    }

    private void InstantiateChs()
    {
        GameObject go1 = new GameObject(); go1.AddComponent<Image>();
        GameObject go2 = new GameObject(); go2.AddComponent<Image>();
        GameObject go3 = new GameObject(); go3.AddComponent<Image>();
        GameObject go4 = new GameObject(); go4.AddComponent<Image>();
        ch1 = go1.GetComponent<Image>(); go1.name = "Crosshair Up"; go1.transform.SetParent(transform);
        ch2 = go2.GetComponent<Image>(); go2.name = "Crosshair Right"; go2.transform.SetParent(transform);
        ch3 = go3.GetComponent<Image>(); go3.name = "Crosshair Down"; go3.transform.SetParent(transform);
        ch4 = go4.GetComponent<Image>(); go4.name = "Crosshair Left"; go4.transform.SetParent(transform);
        ch1.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        ch2.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 270));
        ch3.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 180));
        ch4.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 90));
        Color color = new Color(1, 1, 1, 0);
        ch1.color = color;
        ch2.color = color;
        ch3.color = color;
        ch4.color = color;
        StartCoroutine(SetScale());
    }

    public IEnumerator SetScale()
    {
        yield return 0;
        ch1.GetComponent<RectTransform>().sizeDelta = new Vector2(crosshairSize, crosshairSize);
        ch2.GetComponent<RectTransform>().sizeDelta = new Vector2(crosshairSize, crosshairSize);
        ch3.GetComponent<RectTransform>().sizeDelta = new Vector2(crosshairSize, crosshairSize);
        ch4.GetComponent<RectTransform>().sizeDelta = new Vector2(crosshairSize, crosshairSize);
        PositionChs();
    }

    public void ChangeSprites(Sprite sprite)
    {
        ch1.sprite = sprite;
        ch2.sprite = sprite;
        ch3.sprite = sprite;
        ch4.sprite = sprite;
    }

    private void PositionChs()
    {
        ch1.transform.localPosition = new Vector3(0, currentSpace, 0);
        ch2.transform.localPosition = new Vector3(currentSpace, 0, 0);
        ch3.transform.localPosition = new Vector3(0, -currentSpace, 0);
        ch4.transform.localPosition = new Vector3(-currentSpace, 0, 0);
    }


}
