using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MTR_CatchTheBone))]
public class MTR_CatchTheBone_Inspector : Editor
{
    public Texture MecanimPic;
    public Texture MecanimDotR;
    public Texture MecanimDotS;
    private MTR_CatchTheBone MTR_CTB;

    public override void OnInspectorGUI()
    {
        if (!Application.isPlaying)
        {
            MTR_CTB = (MTR_CatchTheBone)target;

            EditorGUI.BeginChangeCheck();
            MTR_CTB.CT = (CatchType)EditorGUILayout.EnumPopup("Catch Type", MTR_CTB.CT);
            if (EditorGUI.EndChangeCheck())
                Undo.RegisterCompleteObjectUndo(MTR_CTB, "Catch Type Changed");

            int c = 0;
            while (c < 20)
            {
                EditorGUILayout.LabelField("");
                c++;
            }
            GUILayout.BeginArea(new Rect(Screen.width / 2F - MecanimPic.width / 2F, 40, 148, 366), MecanimPic);
            {
                Rect[][] R = new Rect[5][];
                R[0] = new Rect[6];
                R[0][0] = new Rect(64, 30, 19, 19);
                R[0][1] = new Rect(64, 49, 19, 19);
                R[0][2] = new Rect(64, 79, 19, 19);
                R[0][3] = new Rect(64, 107, 19, 19);
                R[0][4] = new Rect(64, 136, 19, 19);
                R[0][5] = new Rect(64, 158, 19, 19);
                R[1] = new Rect[4];
                R[1][0] = new Rect(124, 168, 19, 19);
                R[1][1] = new Rect(108, 117, 19, 19);
                R[1][2] = new Rect(91, 66, 19, 19);
                R[1][3] = new Rect(77, 60, 19, 19);
                R[2] = new Rect[4];
                R[2][0] = new Rect(5, 168, 19, 19);
                R[2][1] = new Rect(21, 117, 19, 19);
                R[2][2] = new Rect(37, 66, 19, 19);
                R[2][3] = new Rect(52, 60, 19, 19);
                R[3] = new Rect[4];
                R[3][0] = new Rect(87, 342, 19, 19);
                R[3][1] = new Rect(85, 325, 19, 19);
                R[3][2] = new Rect(83, 249, 19, 19);
                R[3][3] = new Rect(79, 172, 19, 19);
                R[4] = new Rect[4];
                R[4][0] = new Rect(42, 342, 19, 19);
                R[4][1] = new Rect(44, 325, 19, 19);
                R[4][2] = new Rect(46, 249, 19, 19);
                R[4][3] = new Rect(50, 172, 19, 19);
                if (MTR_CTB.ID == null)
                    MTR_CTB.ID = new List<int>();
                int l = 0;
                while (l < 5)
                {
                    int b = 0;
                    if (l == 0)
                        c = 6;
                    else
                        c = 4;
                    while (b < c)
                    {
                        int id = l * 10 + b;
                        if (MTR_CTB.ID.Contains(id))
                        {
                            if (GUI.Button(R[l][b], MecanimDotS, GUIStyle.none))
                            {
                                Undo.RegisterCompleteObjectUndo(MTR_CTB, "Catch Type Bone Changed");
                                MTR_CTB.ID.Remove(id);
                            }
                        }
                        else
                        {
                            if (GUI.Button(R[l][b], MecanimDotR, GUIStyle.none))
                            {
                                Undo.RegisterCompleteObjectUndo(MTR_CTB, "Catch Type Bone Changed");
                                MTR_CTB.ID.Add(id);
                            }
                        }
                        b++;
                    }
                    l++;
                }
            }
            GUILayout.EndArea();
            if (MTR_CTB.CT == CatchType.SpringJoint)
            {
                MTR_CTB.Spring = FloatField("Spring", MTR_CTB.Spring, MTR_CTB);
                MTR_CTB.Damper = FloatField("Damper", MTR_CTB.Damper, MTR_CTB);
                MTR_CTB.MinDistance = FloatField("Min Distance", MTR_CTB.MinDistance, MTR_CTB);
                MTR_CTB.MaxDistance = FloatField("Max Distance", MTR_CTB.MaxDistance, MTR_CTB);
            }
        }
    }

    private float FloatField(string name, float value, Object obj1)
    {
        EditorGUI.BeginChangeCheck();
        value = EditorGUILayout.FloatField(name, value);
        if (EditorGUI.EndChangeCheck())
            Undo.RegisterCompleteObjectUndo(obj1, name + " Changed");
        return value;
    }
}