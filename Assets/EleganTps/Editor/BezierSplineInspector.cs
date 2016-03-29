using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BezierSpline))]
public class BezierSplineInspector : Editor
{
    private const int stepsPerCurve = 3;
    private const float directionScale = 0.5f;
    private const float handleSize = 0.07f;
    private const float pickSize = 0.06f;
    private BezierSpline spline;
    private Transform handleTransform;
    private Quaternion handleRotation;
    private int selectedIndex = -1;

    public override void OnInspectorGUI()
    {

        spline = target as BezierSpline;


        EditorGUI.BeginChangeCheck();
        bool loop = EditorGUILayout.Toggle("Loop", spline.Loop);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(spline, "Toggle Loop");
            EditorUtility.SetDirty(spline);
            spline.Loop = loop;
        }
        if (selectedIndex >= 0 && selectedIndex < spline.ControlPointCount)
        {
            DrawSelectedPointInspector();
        }
        if (GUILayout.Button("Add Curve"))
        {
            Undo.RecordObject(spline, "Add Curve");
            spline.AddCurve();
            EditorUtility.SetDirty(spline);
        }
        if (GUILayout.Button("StickToGround"))
        {
            Undo.RecordObject(spline, "Stick");
            spline.StickToGround();
            EditorUtility.SetDirty(spline);
        }
    }
    void OnDisable()
    {
        spline = target as BezierSpline;
        spline.isScene = false;
    }

    private void DrawSelectedPointInspector()
    {
        GUILayout.Label("Selected Point");
        EditorGUI.BeginChangeCheck();
        Vector3 point = EditorGUILayout.Vector3Field("Position", spline.GetControlPoint(selectedIndex));
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(spline, "Move Point");
            EditorUtility.SetDirty(spline);
            spline.SetControlPoint(selectedIndex, point);
        }
        EditorGUI.BeginChangeCheck();
        BezierControlPointMode mode = (BezierControlPointMode)EditorGUILayout.EnumPopup("Mode", spline.GetControlPointMode(selectedIndex));
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(spline, "Change Point Mode");
            spline.SetControlPointMode(selectedIndex, mode);
            EditorUtility.SetDirty(spline);
        }
    }

    private void OnSceneGUI()
    {
        spline = target as BezierSpline;
        spline.isScene = true;
        handleTransform = spline.transform;
        handleRotation = Tools.pivotRotation == PivotRotation.Local ?
            handleTransform.rotation : Quaternion.identity;

        Vector3 p0 = ShowPoint(0);
        for (int i = 1; i < spline.ControlPointCount; i += 3)
        {
            Vector3 p1 = ShowPoint(i); //Debug.DrawRay(p1, Vector3.up*.1f,Color.black);

            colorG = colorCurv; sizeG = sizeCurv;

            Vector3 p2 = ShowPoint(i + 1);// Debug.DrawRay(p2, Vector3.up* .1f, Color.black);

            colorG = colorDot; sizeG = sizeDot;

            Vector3 p3 = ShowPoint(i + 2); //Debug.DrawRay(p3, Vector3.up * .1f,Color.red); 
            colorG = colorCurv; sizeG = sizeCurv;

            Handles.color = Color.gray;
            Handles.DrawLine(p0, p1);
            Handles.DrawLine(p2, p3);

            Handles.DrawBezier(p0, p3, p1, p2, Color.white, Resources.Load("metal") as Texture2D, 2f);
            p0 = p3;
        }
        ShowDirections();
    }

    private void ShowDirections()
    {
        //Handles.color = Color.white;
        //Vector3 point = spline.GetPoint(0f);
        //Handles.DrawLine(point, point + spline.GetDirection(0f) * directionScale);
        //int steps = stepsPerCurve * spline.CurveCount;
        //for (int i = 1; i <= steps; i++) {
        //	point = spline.GetPoint(i / (float)steps);
        //	Handles.DrawLine(point, point + spline.GetDirection(i / (float)steps) * directionScale);
        //}
    }
    float sizeCurv = 3f;
    float sizeDot = 3.6f;
    float sizeG;
    Color colorDot = Color.red;
    Color colorCurv = Color.black;
    Color colorG;
    private Vector3 ShowPoint(int index)
    {
        Vector3 point = handleTransform.TransformPoint(spline.GetControlPoint(index));

        float size = HandleUtility.GetHandleSize(point);
        if (index == 0)
        {
            size *= 4f;
            Handles.color = Color.green;
        }
        else
        {
            Handles.color = colorG;
            size *= sizeG;
        }



        //Handles.color = modeColors[(int)spline.GetControlPointMode(index)];
        int i = (int)spline.GetControlPointMode(index);
        i++;
        if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.CircleCap))
        {
            selectedIndex = index;
            Repaint();
        }
        if (selectedIndex == index)
        {
            EditorGUI.BeginChangeCheck();
            point = Handles.DoPositionHandle(point, handleRotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(spline, "Move Point");
                EditorUtility.SetDirty(spline);
                spline.SetControlPoint(index, handleTransform.InverseTransformPoint(point));
            }
        }
        return point;
    }
}