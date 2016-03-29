using UnityEngine;
using System.Collections;

public class JumpToClimb : MonoBehaviour
{
    PlayerAtts plAtts;
    public BezierSpline spline;

    void Awake()
    {
        plAtts = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerAtts>();
        spline = transform.FindChild("Spline").GetComponent<BezierSpline>();
    }

    void OnTriggerStay(Collider col)
    {
        if (!col.CompareTag("Player"))
            return;

        plAtts.spline = spline;

    }

    void OnTriggerExit(Collider col)
    {
        plAtts.spline = null;
    }
}
