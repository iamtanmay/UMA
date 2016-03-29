using UnityEngine;
using System;
public enum SplineMode
{
    Once,
    Loop,
    PingPong
}
public class BezierSpline : MonoBehaviour
{
    [HideInInspector]
    public float splineLength;
    [HideInInspector]
    public Vector3 leftEdge;
    [HideInInspector]
    public Vector3 rightEdge;

    private PlayerAtts plAtts;

    [SerializeField]
    public Vector3[] points;

    [SerializeField]
    private BezierControlPointMode[] modes;

    [SerializeField]
    private bool loop;


    public void StickToGround()
    {
        int i = 0; float offsetY = .1f;
        foreach (Vector3 point in points)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.TransformPoint(point), Vector3.down, out hit, 10f))
            {
                SetControlPoint(i, transform.InverseTransformPoint(hit.point) + Vector3.up * offsetY);
            }
            i++;
        }
    }
    [System.NonSerialized]
    public bool isScene = false;
    void OnDrawGizmos()
    {
        if (isScene)
            return;
        float i = 0;

        while (i * step < 1)
        {
            Debug.DrawLine(this.GetPoint(step * i), this.GetPoint(step * (i + 1)), Color.red);
            i++;
        }
    }
    float step = .01f;

    void Start()
    {
        plAtts = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerAtts>();
        splineLength = GetLength();
        rightEdge = GetPoint(0);
        leftEdge = GetPoint(1);
    }

    public void GetClosePointToVector(ref float closeStep, ref Vector3 closePoint, Vector3 vector)
    {
        float lowestDist = 999999;
        float step = 0;
        while (step <= 1)
        {
            float thisDist = Vector3.Distance(vector, this.GetPoint(step));
            if (thisDist < lowestDist)
            {
                lowestDist = thisDist;
                closePoint = this.GetPoint(step);
                closeStep = step;
            }
            step += 1 / plAtts.splineSplit;
        }
    }
    Transform player;

    public void GetClosePointToPlayer(ref float closeStep, ref Vector3 closePoint)
    {
        float lowestDist = 999999;
        float step = 0;
        while (step <= 1)
        {
            float thisDist = Vector3.Distance(plAtts.transform.position, this.GetPoint(step));
            if (thisDist < lowestDist)
            {
                lowestDist = thisDist;
                closePoint = this.GetPoint(step);
                closeStep = step;
            }
            step += 1 / plAtts.splineSplit;
        }
    }
    public float GetClosePointToPlayer(ref float closeStep)
    {
        float lowestDist = 999999;
        float step = 0;
        while (step <= 1)
        {
            float thisDist = Vector3.Distance(plAtts.transform.position, this.GetPoint(step));
            if (thisDist < lowestDist)
            {
                lowestDist = thisDist;
                closeStep = step;
            }
            step += 1 / plAtts.splineSplit;
        }
        return lowestDist;
    }
    public Vector3 GetClosePointToPlayer()
    {
        player = GameObject.Find("Player").transform;
        if (!Application.isPlaying)
            return Vector3.zero;
        Vector3 closePoint = Vector3.zero;
        float lowestDist = 999999;
        float step = 0;
        while (step <= 1)
        {
            float thisDist = Vector3.Distance(player.position, this.GetPoint(step));
            if (thisDist < lowestDist)
            {
                lowestDist = thisDist;
                closePoint = this.GetPoint(step);
            }
            step += (1 / plAtts.splineSplit);
        }
        return closePoint;
    }

    public float GetLength()
    {
        float step = 0, lastStep = 0;
        float split = plAtts.splineSplit;
        float length = 0;
        while (step <= 1)
        {
            step += 1 / split;
            length += Vector3.Distance(this.GetPoint(lastStep), this.GetPoint(step));
            lastStep = step;
        }

        return length;
    }

    public bool Loop
    {
        get
        {
            return loop;
        }
        set
        {
            loop = value;
            if (value == true)
            {
                modes[modes.Length - 1] = modes[0];
                SetControlPoint(0, points[0]);
            }
        }
    }

    public int ControlPointCount
    {
        get
        {
            return points.Length;
        }
    }

    public Vector3 GetControlPoint(int index)
    {
        return points[index];
    }

    public void SetControlPoint(int index, Vector3 point)
    {
        if (index % 3 == 0)
        {
            Vector3 delta = point - points[index];
            if (loop)
            {
                if (index == 0)
                {
                    points[1] += delta;
                    points[points.Length - 2] += delta;
                    points[points.Length - 1] = point;
                }
                else if (index == points.Length - 1)
                {
                    points[0] = point;
                    points[1] += delta;
                    points[index - 1] += delta;
                }
                else
                {
                    points[index - 1] += delta;
                    points[index + 1] += delta;
                }
            }
            else
            {
                if (index > 0)
                {
                    points[index - 1] += delta;
                }
                if (index + 1 < points.Length)
                {
                    points[index + 1] += delta;
                }
            }
        }
        points[index] = point;
        EnforceMode(index);
    }

    public BezierControlPointMode GetControlPointMode(int index)
    {
        return modes[(index + 1) / 3];
    }

    public void SetControlPointMode(int index, BezierControlPointMode mode)
    {
        int modeIndex = (index + 1) / 3;
        modes[modeIndex] = mode;
        if (loop)
        {
            if (modeIndex == 0)
            {
                modes[modes.Length - 1] = mode;
            }
            else if (modeIndex == modes.Length - 1)
            {
                modes[0] = mode;
            }
        }
        EnforceMode(index);
    }

    private void EnforceMode(int index)
    {
        int modeIndex = (index + 1) / 3;
        BezierControlPointMode mode = modes[modeIndex];
        if (mode == BezierControlPointMode.Free || !loop && (modeIndex == 0 || modeIndex == modes.Length - 1))
        {
            return;
        }

        int middleIndex = modeIndex * 3;
        int fixedIndex, enforcedIndex;
        if (index <= middleIndex)
        {
            fixedIndex = middleIndex - 1;
            if (fixedIndex < 0)
            {
                fixedIndex = points.Length - 2;
            }
            enforcedIndex = middleIndex + 1;
            if (enforcedIndex >= points.Length)
            {
                enforcedIndex = 1;
            }
        }
        else
        {
            fixedIndex = middleIndex + 1;
            if (fixedIndex >= points.Length)
            {
                fixedIndex = 1;
            }
            enforcedIndex = middleIndex - 1;
            if (enforcedIndex < 0)
            {
                enforcedIndex = points.Length - 2;
            }
        }

        Vector3 middle = points[middleIndex];
        Vector3 enforcedTangent = middle - points[fixedIndex];
        if (mode == BezierControlPointMode.Aligned)
        {
            enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, points[enforcedIndex]);
        }
        points[enforcedIndex] = middle + enforcedTangent;
    }

    public int CurveCount
    {
        get
        {
            return (points.Length - 1) / 3;
        }
    }

    public Vector3 GetPoint(float t)
    {
        int i;
        if (t >= 1f)
        {
            t = 1f;
            i = points.Length - 4;
        }
        else
        {
            t = Mathf.Clamp01(t) * CurveCount;
            i = (int)t;
            t -= i;
            i *= 3;
        }
        return transform.TransformPoint(Bezier.GetPoint(points[i], points[i + 1], points[i + 2], points[i + 3], t));
    }

    public Vector3 GetVelocity(float t)
    {
        int i;
        if (t >= 1f)
        {
            t = 1f;
            i = points.Length - 4;
        }
        else
        {
            t = Mathf.Clamp01(t) * CurveCount;
            i = (int)t;
            t -= i;
            i *= 3;
        }
        return transform.TransformPoint(Bezier.GetFirstDerivative(points[i], points[i + 1], points[i + 2], points[i + 3], t)) - transform.position;
    }

    public Vector3 GetDirection(float t)
    {
        return GetVelocity(t).normalized;
    }

    public void AddCurve()
    {
        Vector3 point = points[points.Length - 1];
        Array.Resize(ref points, points.Length + 3);
        point.x += 1f;
        points[points.Length - 3] = point;
        point.x += 1f;
        points[points.Length - 2] = point;
        point.x += 1f;
        points[points.Length - 1] = point;

        Array.Resize(ref modes, modes.Length + 1);
        modes[modes.Length - 1] = modes[modes.Length - 2];
        EnforceMode(points.Length - 4);

        if (loop)
        {
            points[points.Length - 1] = points[0];
            modes[modes.Length - 1] = modes[0];
            EnforceMode(0);
        }
    }

    public void Reset()
    {
        points = new Vector3[] {
            new Vector3(1f, 0f, 0f),
            new Vector3(2f, 0f, 0f),
            new Vector3(3f, 0f, 0f),
            new Vector3(4f, 0f, 0f)
        };
        modes = new BezierControlPointMode[] {
            BezierControlPointMode.Free,
            BezierControlPointMode.Free
        };
    }
}