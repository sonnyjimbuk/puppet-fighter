using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MecanimToRagdoll))]
public class MTR_Inspector : Editor
{
    private enum RagdollType { AnimationStart, RagdollStart, JustRagdoll };
    private enum RagdollInterpolate { None, Interpolate, Extrapolate };
    private enum RagdollCollisionDetection { Discrete, Continuous, ContinuousDynamic, ContinuousSpeculative };

    public Texture MecanimPic;
    private float MecanimPicY = 0;
    public Texture MecanimDotR;
    public Texture MecanimDotB;
    public Texture MecanimDotS;
    private MecanimToRagdoll MTR_I;

    public bool ShowColliders;
    public int SelectedColliderL;
    public int SelectedColliderB;
    public string SelectedColliderN = "";
    public bool Mirroring = false;

    private Animator animator;

    private Object HandleObject;

    public Texture storeIcon;
    public Texture forumIcon;
    public Texture coffeeIcon;

    private readonly GUIContent storeButtonContent = new("", "Please leave a review about this asset in the Asset Store.\nThis will help me a lot.\nThank you.");
    private readonly GUIContent forumButtonContent = new("", "If you have any problems, questions, or suggestions, welcome to the forum.");
    private readonly GUIContent coffeeButtonContent = new("", "If you think this asset is underpriced or want to thank me, you can buy me a coffee. ;)");

    private readonly GUIContent TPoseContent = new("T Pose", "Here should be a \"T-pose\" prefab of this character.");
    private readonly GUIContent MusclesPoseContent = new("Muscles Pose", "Here should be a \"Muscles\" prefab of this character.");

    private readonly GUIContent RagdollTypeContent = new("Ragdoll Type", "Choosing the type of ragdoll depends on your needs.");
    private readonly GUIContent MassContent = new("Mass", "The total mass of the ragdoll.\nIf the cell is not active, then this mass is entered in the Rigidbody of this character.");
    private readonly GUIContent ResurrectingTimeContent = new("Resurrecting Time", "Time for which the character will be resurrected.");

    private readonly GUIContent AnimatorResurrectTriggerNameContent = new("Trigger Parameter \"Resurrect\"", "The name of the trigger parameter in the animator controller that indicates that the ragdoll is \"resurrecting\". \nIf the text field is empty, then the resurrection will use the last animation.");
    private readonly GUIContent AnimatorOnBackBoolNameContent = new("Bool Parameter \"On Back\"", "The name of the bool parameter in the animator controller that indicates that the ragdoll is lying on its back. \nIf the text field is empty, then you use one resurrection animation.");


    private readonly GUIContent AddVelocityContent = new("Add Velocity", "Add character velocity to character bones when it becomes a ragdoll.");
    private readonly GUIContent DragContent = new("Drag", "The linear drag coefficient for all bones of ragdoll.\n0 means no damping. [ 0, infinity ]");
    private readonly GUIContent AngularDragContent = new("Angular Drag", "The angular drag coefficient for all bones of ragdoll.\n0 means no damping. [ 0, infinity ]");
    private readonly GUIContent InterpolateContent = new("Interpolate", "Interpolate for all bones of ragdoll.");
    private readonly GUIContent CollisionDetectionContent = new("Collision Detection", "Collision Detection for all bones of ragdoll.");
    private readonly GUIContent PhysicMaterialContent = new("Physic Material", "Reference to the Physics Material that determines how ragdoll's Colliders interact with others.");

    private readonly GUIContent GCOSContent = new("Generate Colliders On Start", "Generate default colliders for bones at the start of the game.");

    private readonly GUIContent CJSpringContent = new("CJ Spring", "The spring force used to keep the two objects together.\nIt will be specified for all joints");
    private readonly GUIContent CJDamperContent = new("CJ Damper", "The damper force used to dampen the spring force.\nIt will be specified for all joints");
    private readonly GUIContent CJPreprocessingContent = new ("CJ Preprocessing", "Disabling preprocessing helps to stabilize impossible-to-fulfil configurations.\nIt will be specified for all joints");

    void OnEnable()
    {
        storeButtonContent.image = storeIcon;
        forumButtonContent.image = forumIcon;
        coffeeButtonContent.image = coffeeIcon;
    }

    public override void OnInspectorGUI()
    {
        MTR_I = (MecanimToRagdoll)target;
        if (MTR_I)
            animator = MTR_I.GetComponent<Animator>();
        if (animator)
        {
            if (animator.isHuman)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Prefabs", EditorStyles.boldLabel);
                //Please support me.
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(storeButtonContent, GUIStyle.none, GUILayout.Width(19), GUILayout.Height(19)))
                    Application.OpenURL("https://assetstore.unity.com/packages/tools/physics/mecanim-to-ragdoll-158348#reviews");
                GUILayout.Space(5);
                if (GUILayout.Button(forumButtonContent, GUIStyle.none, GUILayout.Width(19), GUILayout.Height(19)))
                    Application.OpenURL("https://forum.unity.com/threads/mecanim-to-ragdoll.804075/");
                GUILayout.Space(5);
                if (GUILayout.Button(coffeeButtonContent, GUIStyle.none, GUILayout.Width(19), GUILayout.Height(19)))
                    Application.OpenURL("https://www.buymeacoffee.com/virtualsun");
                EditorGUILayout.EndHorizontal();

                GameObject Pose = MTR_I.TPose;
                GameObject PoseN = (GameObject)EditorGUILayout.ObjectField(TPoseContent, MTR_I.TPose, typeof(GameObject), true);
                if (Pose != PoseN)
                {
                    Undo.RecordObject(MTR_I, "T Pose Changed");
                    MTR_I.TPose = PoseN;
                }

                Pose = MTR_I.MusclesPose;
                PoseN = (GameObject)EditorGUILayout.ObjectField(MusclesPoseContent, MTR_I.MusclesPose, typeof(GameObject), true);
                if (Pose != PoseN)
                {
                    Undo.RecordObject(MTR_I, "Muscles Pose Changed");
                    MTR_I.MusclesPose = PoseN;
                }

                if (MTR_I.TPose && MTR_I.MusclesPose)
                {
                    //Ragdoll Type
                    RagdollType ragdollType;
                    if (MTR_I.JustRagdoll)
                        ragdollType = RagdollType.JustRagdoll;
                    else
                    {
                        if (MTR_I.Animate)
                            ragdollType = RagdollType.AnimationStart;
                        else
                            ragdollType = RagdollType.RagdollStart;
                    }

                    EditorGUILayout.Space(10);
                    EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

                    ragdollType = (RagdollType)EditorGUILayout.EnumPopup(RagdollTypeContent, ragdollType);
                    if (ragdollType == RagdollType.JustRagdoll)
                    {
                        if (!MTR_I.JustRagdoll)
                        {
                            Undo.RecordObject(MTR_I, "Ragdoll Type Changed to Just Ragdoll");
                            MTR_I.JustRagdoll = true;
                            MTR_I.Animate = false;
                        }
                    }
                    else
                    {
                        if (ragdollType == RagdollType.AnimationStart)
                        {
                            if (!MTR_I.Animate || MTR_I.JustRagdoll)
                            {
                                Undo.RecordObject(MTR_I, "Ragdoll Type Changed to Animation Start");
                                MTR_I.Animate = true;
                                MTR_I.JustRagdoll = false;
                            }
                        }
                        else if (ragdollType == RagdollType.RagdollStart)
                        {
                            if (MTR_I.Animate || MTR_I.JustRagdoll)
                            {
                                Undo.RecordObject(MTR_I, "Ragdoll Type Changed Ragdoll Start");
                                MTR_I.Animate = false;
                                MTR_I.JustRagdoll = false;
                            }
                        }
                    }

                    GUI.enabled = !MTR_I.TryGetComponent(out Rigidbody RB);
                    MTR_I.Mass = FloatField(MassContent, MTR_I.Mass, MTR_I, null);
                    GUI.enabled = true;

                    MTR_I.ResurrectingTime = FloatField(ResurrectingTimeContent, MTR_I.ResurrectingTime, MTR_I, null);

                    EditorGUILayout.Space(10);
                    EditorGUILayout.LabelField("Animator Settings", EditorStyles.boldLabel);

                    MTR_I.AnimatorResurrectTriggerName = TextField(AnimatorResurrectTriggerNameContent, MTR_I.AnimatorResurrectTriggerName, MTR_I, null);
                    MTR_I.AnimatorOnBackBoolName = TextField(AnimatorOnBackBoolNameContent, MTR_I.AnimatorOnBackBoolName, MTR_I, null);

                    EditorGUILayout.Space(10);
                    EditorGUILayout.LabelField("Rigidbodies Settings", EditorStyles.boldLabel);

                    MTR_I.AddVelocity = Toggle(AddVelocityContent, MTR_I.AddVelocity, MTR_I, null);

                    MTR_I.Drag = FloatField(DragContent, MTR_I.Drag, MTR_I, null);
                    MTR_I.AngularDrag = FloatField(AngularDragContent, MTR_I.AngularDrag, MTR_I, null);

                    //Ragdoll Interpolate
                    RagdollInterpolate ragdollInterpolate;
                    if (MTR_I.RBI == RigidbodyInterpolation.None)
                        ragdollInterpolate = RagdollInterpolate.None;
                    else if (MTR_I.RBI == RigidbodyInterpolation.Interpolate)
                        ragdollInterpolate = RagdollInterpolate.Interpolate;
                    else
                        ragdollInterpolate = RagdollInterpolate.Extrapolate;

                    ragdollInterpolate = (RagdollInterpolate)EditorGUILayout.EnumPopup(InterpolateContent, ragdollInterpolate);
                    if (ragdollInterpolate == RagdollInterpolate.None && MTR_I.RBI != RigidbodyInterpolation.None)
                    {
                        Undo.RecordObject(MTR_I, "Changed Interpolation to None");
                        MTR_I.RBI = RigidbodyInterpolation.None;
                    }
                    else if (ragdollInterpolate == RagdollInterpolate.Interpolate && MTR_I.RBI != RigidbodyInterpolation.Interpolate)
                    {
                        Undo.RecordObject(MTR_I, "Changed Interpolation to Interpolate");
                        MTR_I.RBI = RigidbodyInterpolation.Interpolate;
                    }
                    else if (ragdollInterpolate == RagdollInterpolate.Extrapolate && MTR_I.RBI != RigidbodyInterpolation.Extrapolate)
                    {
                        Undo.RecordObject(MTR_I, "Changed Interpolation to Extrapolate");
                        MTR_I.RBI = RigidbodyInterpolation.Extrapolate;
                    }

                    //Ragdoll Collision Detection
                    RagdollCollisionDetection ragdollCollisionDetection;
                    if (MTR_I.RBCDM == CollisionDetectionMode.Discrete)
                        ragdollCollisionDetection = RagdollCollisionDetection.Discrete;
                    else if (MTR_I.RBCDM == CollisionDetectionMode.Continuous)
                        ragdollCollisionDetection = RagdollCollisionDetection.Continuous;
                    else if (MTR_I.RBCDM == CollisionDetectionMode.ContinuousDynamic)
                        ragdollCollisionDetection = RagdollCollisionDetection.ContinuousDynamic;
                    else
                        ragdollCollisionDetection = RagdollCollisionDetection.ContinuousSpeculative;

                    ragdollCollisionDetection = (RagdollCollisionDetection)EditorGUILayout.EnumPopup(CollisionDetectionContent, ragdollCollisionDetection);
                    if (ragdollCollisionDetection == RagdollCollisionDetection.Discrete && MTR_I.RBCDM != CollisionDetectionMode.Discrete)
                    {
                        Undo.RecordObject(MTR_I, "Collision Detection Mode Changed to Discrete");
                        MTR_I.RBCDM = CollisionDetectionMode.Discrete;
                    }
                    else if (ragdollCollisionDetection == RagdollCollisionDetection.Continuous && MTR_I.RBCDM != CollisionDetectionMode.Continuous)
                    {
                        Undo.RecordObject(MTR_I, "Collision Detection Mode Changed to Continuous");
                        MTR_I.RBCDM = CollisionDetectionMode.Continuous;
                    }
                    else if (ragdollCollisionDetection == RagdollCollisionDetection.ContinuousDynamic && MTR_I.RBCDM != CollisionDetectionMode.ContinuousDynamic)
                    {
                        Undo.RecordObject(MTR_I, "Collision Detection Mode Changed to Continuous Dynamic");
                        MTR_I.RBCDM = CollisionDetectionMode.ContinuousDynamic;
                    }
                    else if (ragdollCollisionDetection == RagdollCollisionDetection.ContinuousSpeculative && MTR_I.RBCDM != CollisionDetectionMode.ContinuousSpeculative)
                    {
                        Undo.RecordObject(MTR_I, "Collision Detection Mode Changed to Continuous Speculative");
                        MTR_I.RBCDM = CollisionDetectionMode.ContinuousSpeculative;
                    }

                    //Physic Material
                    EditorGUI.BeginChangeCheck();
                    PhysicsMaterial PM = (PhysicsMaterial)EditorGUILayout.ObjectField(PhysicMaterialContent, MTR_I.CollidersMaterial, typeof(PhysicsMaterial), true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RegisterCompleteObjectUndo(MTR_I, "Physic Material Changed");
                        MTR_I.CollidersMaterial = PM;
                    }

                    EditorGUILayout.Space(10);
                    EditorGUILayout.LabelField("Character Joints Settings", EditorStyles.boldLabel);

                    //CharacterJoint Spring & Damper
                    MTR_I.ChJSpring = FloatField(CJSpringContent, MTR_I.ChJSpring, MTR_I, null);
                    MTR_I.ChJDamper = FloatField(CJDamperContent, MTR_I.ChJDamper, MTR_I, null);
                    MTR_I.ChJPreprocessing = Toggle(CJPreprocessingContent, MTR_I.ChJPreprocessing, MTR_I, null);

                    EditorGUILayout.Space(10);

                    if (MTR_I.GetComponentsInChildren<MTR_BoneCollidersControl>().Length == 0)
                    {
                        EditorGUILayout.BeginHorizontal();
                        MTR_I.GenerateCollidersOnStart = Toggle(GCOSContent, MTR_I.GenerateCollidersOnStart, MTR_I, null);
                        if (MTR_I.GenerateCollidersOnStart)
                        {
                            EditorGUILayout.LabelField("");
                        }
                        else
                        {
                            if (GUILayout.Button(new GUIContent("Generate Colliders", "Generate colliders for bones now.")))
                            {
                                if (MTR_I.GetComponent<Animator>() && MTR_I.TPose != null && MTR_I.MusclesPose != null)
                                {
                                    Undo.RegisterCompleteObjectUndo(MTR_I, "Generate Colliders");
                                    MTR_I.GenerateColliders();
                                    foreach (MTR_BoneCollidersControl BCC in MTR_I.GetComponentsInChildren<MTR_BoneCollidersControl>())
                                        Undo.RegisterCreatedObjectUndo(BCC.gameObject, "Generate Colliders");
                                }
                                else
                                    Debug.LogWarning("Not all values are filled!");
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        string TempString = "Show Colliders";
                        string tempString = "Show colliders editor for bones.";
                        if (ShowColliders)
                        {
                            TempString = "Hide Colliders";
                            tempString = "Hide colliders editor for bones.";
                        }
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button(new GUIContent(TempString, tempString)))
                        {
                            Undo.RecordObject(this, TempString);
                            if (ShowColliders)
                                ShowColliders = false;
                            else
                                ShowColliders = true;
                        }
                        if (GUILayout.Button(new GUIContent("Remove Colliders", "Remove colliders from bones now. \n\nWARNING: It might not work and throw an error if you don't edit the prefab itself.")))
                        {
                            Undo.RegisterCompleteObjectUndo(MTR_I, "Colliders Removed");
                            foreach (MTR_BoneCollidersControl BCC in MTR_I.gameObject.GetComponentsInChildren<MTR_BoneCollidersControl>())
                                Undo.DestroyObjectImmediate(BCC.gameObject);
                            MTR_I.BoneCollidersControl = null;
                            MTR_I.GenerateCollidersOnStart = true;
                            ShowColliders = false;
                        }
                        EditorGUILayout.EndHorizontal();

                        int c = 0;

                        if (ShowColliders)
                        {
                            EditorGUI.BeginChangeCheck();
                            bool m = EditorGUILayout.ToggleLeft("Mirror", Mirroring);
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(this, name + "Mirror Changed to " + m);
                                Mirroring = m;
                            }
                            if (Event.current.type == EventType.Repaint)
                                MecanimPicY = Mathf.Max(MecanimPicY, GUILayoutUtility.GetLastRect().y);
                            

                            GUILayout.Space(366);
                            GUILayout.BeginArea(new Rect(Screen.width / 2F - MecanimPic.width / 2F, MecanimPicY + 16, 148, 366), MecanimPic);
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
                                if (MTR_I.BoneCollidersControl == null)
                                    MTR_I.GetDefaultColliders();
                                else if (MTR_I.BoneCollidersControl.Length == 0)
                                    MTR_I.GetDefaultColliders();
                                else if (MTR_I.BoneCollidersControl[0][5] == null)
                                    MTR_I.GetDefaultColliders();
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
                                        if (MTR_I.BoneCollidersControl[l][b])
                                        {
                                            if (SelectedColliderL == l + 1 && SelectedColliderB == b + 1)
                                            {
                                                if (GUI.Button(R[l][b], new GUIContent(MecanimDotS, "Deselect the bone of the \"" + SelectedColliderN + "\"."), GUIStyle.none))
                                                {
                                                    GUI.FocusControl(null);
                                                    HandleObject = null;
                                                    Undo.RecordObject(this, "Bone Deselected");
                                                    SelectedColliderL = 0;
                                                    SelectedColliderB = 0;
                                                }
                                            }
                                            else
                                            {
                                                if (GUI.Button(R[l][b], new GUIContent(MecanimDotB, "Select the bone."), GUIStyle.none))
                                                {
                                                    GUI.FocusControl(null);
                                                    HandleObject = null;
                                                    Undo.RecordObject(this, "Bone Selected");
                                                    SelectedColliderL = l + 1;
                                                    SelectedColliderB = b + 1;

                                                    if (l == 0)
                                                    {
                                                        if (b == 0)
                                                            SelectedColliderN = "Head";
                                                        else if (b == 1)
                                                            SelectedColliderN = "Neck";
                                                        else if (b == 2)
                                                            SelectedColliderN = "Upper Chest";
                                                        else if (b == 3)
                                                            SelectedColliderN = "Chest";
                                                        else if (b == 4)
                                                            SelectedColliderN = "Spine";
                                                        else
                                                            SelectedColliderN = "Hips";
                                                    }
                                                    else if (l == 1)
                                                    {
                                                        if (b == 0)
                                                            SelectedColliderN = "Left Hand";
                                                        else if (b == 1)
                                                            SelectedColliderN = "Left Lower Arm";
                                                        else if (b == 2)
                                                            SelectedColliderN = "Left Upper  Arm";
                                                        else if (b == 3)
                                                            SelectedColliderN = "Left Shoulder";
                                                    }
                                                    else if (l == 2)
                                                    {
                                                        if (b == 0)
                                                            SelectedColliderN = "Right Hand";
                                                        else if (b == 1)
                                                            SelectedColliderN = "Right Lower Arm";
                                                        else if (b == 2)
                                                            SelectedColliderN = "Right Upper Arm";
                                                        else if (b == 3)
                                                            SelectedColliderN = "Right Shoulder";
                                                    }
                                                    else if (l == 3)
                                                    {
                                                        if (b == 0)
                                                            SelectedColliderN = "Left Toes";
                                                        else if (b == 1)
                                                            SelectedColliderN = "Left Foot";
                                                        else if (b == 2)
                                                            SelectedColliderN = "Left Lower Leg";
                                                        else if (b == 3)
                                                            SelectedColliderN = "Left Upper Leg";
                                                    }
                                                    else
                                                    {
                                                        if (b == 0)
                                                            SelectedColliderN = "Right Toes";
                                                        else if (b == 1)
                                                            SelectedColliderN = "Right Foot";
                                                        else if (b == 2)
                                                            SelectedColliderN = "Right Lower Leg";
                                                        else if (b == 3)
                                                            SelectedColliderN = "Right Upper Leg";
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (GUI.Button(R[l][b], new GUIContent(MecanimDotR, "This bone is missing from the character. \nIt is normal if it is not supposed to be there."), GUIStyle.none))
                                            {
                                                SelectedColliderL = 0;
                                                SelectedColliderB = 0;
                                            }
                                        }
                                        b++;
                                    }
                                    l++;
                                }
                            }
                            GUILayout.EndArea();

                            if (SelectedColliderL != 0 && SelectedColliderB != 0)
                            {
                                MTR_BoneCollidersControl BCC = MTR_I.BoneCollidersControl[SelectedColliderL - 1][SelectedColliderB - 1];
                                MTR_BoneCollidersControl BCCm = null;
                                if (Mirroring && SelectedColliderL - 1 != 0)
                                {
                                    if (SelectedColliderL == 2)
                                        BCCm = MTR_I.BoneCollidersControl[2][SelectedColliderB - 1];
                                    else if (SelectedColliderL == 3)
                                        BCCm = MTR_I.BoneCollidersControl[1][SelectedColliderB - 1];
                                    else if (SelectedColliderL == 4)
                                        BCCm = MTR_I.BoneCollidersControl[4][SelectedColliderB - 1];
                                    else
                                        BCCm = MTR_I.BoneCollidersControl[3][SelectedColliderB - 1];
                                }
                                if (BCC)
                                {
                                    if (BCCm)
                                    {
                                        if (BCCm.LookAtNextBone != BCC.LookAtNextBone)
                                        {
                                            Object[] obj = { BCCm, BCCm.transform };
                                            Undo.RecordObjects(obj, "Mirroring Collider Holder LookAt");
                                            BCCm.LookAtNextBone = BCC.LookAtNextBone;
                                            BCCm.SetPosition();
                                        }
                                        if ((SelectedColliderL == 2 || SelectedColliderL == 3) && SelectedColliderB != 2)
                                        {
                                            Vector3 V3p = BCC.Position;
                                            if (BCCm.Position != new Vector3(V3p.x, -V3p.y, V3p.z))
                                            {
                                                Object[] obj = { BCCm, BCCm.transform };
                                                Undo.RecordObjects(obj, "Mirroring Collider Holder Position");
                                                BCCm.Position = new Vector3(V3p.x, -V3p.y, V3p.z);
                                                BCCm.SetPosition();
                                            }
                                        }
                                        else
                                        {
                                            Vector3 V3p = BCC.Position;
                                            if (BCCm.Position != new Vector3(V3p.x, V3p.y, -V3p.z))
                                            {
                                                Object[] obj = { BCCm, BCCm.transform };
                                                Undo.RecordObjects(obj, "Mirroring Collider Holder Position");
                                                BCCm.Position = new Vector3(V3p.x, V3p.y, -V3p.z);
                                                BCCm.SetPosition();
                                            }
                                        }
                                    }
                                    EditorGUILayout.LabelField(SelectedColliderN + " collider holder:");
                                    EditorGUI.indentLevel++;
                                    EditorGUILayout.BeginHorizontal();
                                    if (BCC.NextBone != null)
                                    {
                                        EditorGUILayout.LabelField("Position & Look At");
                                        TempString = "Default";
                                        if (BCC.LookAtNextBone)
                                            TempString = "At next bone";
                                        if (GUILayout.Button(TempString))
                                        {
                                            Object[] obj = { BCC, BCC.transform };
                                            if (BCCm)
                                            {
                                                obj = new Object[4];
                                                obj[0] = BCC;
                                                obj[1] = BCC.transform;
                                                obj[2] = BCCm;
                                                obj[3] = BCCm.transform;
                                            }
                                            Undo.RecordObjects(obj, "Colliders Holder LookAt Editing");
                                            if (BCC.LookAtNextBone)
                                                BCC.LookAtNextBone = false;
                                            else
                                                BCC.LookAtNextBone = true;
                                            BCC.SetPosition();

                                            if (BCCm)
                                            {
                                                BCCm.LookAtNextBone = BCC.LookAtNextBone;
                                                BCCm.SetPosition();
                                            }
                                        }
                                    }
                                    else
                                        EditorGUILayout.LabelField("Position");
                                    string S = "Edit";
                                    string St = "Edit the all colliders of bone in the \"Scene\" window. \nMove in space with the \"Move tool\" only.";
                                    if (HandleObject == BCC)
                                    {
                                        S = "Editing";
                                        St = "Disable collider editing in the \"Scene\" window. \n\nDescription: You can now edit the collider in the Scene window. After clicking on this button, you will not be able to.";
                                    }
                                    if (GUILayout.Button(new GUIContent(S, St)))
                                    {
                                        Tools.current = Tool.Move;
                                        if (HandleObject != BCC)
                                            HandleObject = BCC;
                                        else
                                            HandleObject = null;
                                        SceneView.RepaintAll();
                                    }
                                    EditorGUILayout.EndHorizontal();
                                    EditorGUI.indentLevel++;

                                    EditorGUI.BeginChangeCheck();
                                    Vector3 V3 = EditorGUILayout.Vector3Field("", BCC.Position);
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        if (BCCm)
                                        {
                                            Object[] obj = { BCC, BCC.transform, BCCm, BCCm.transform };
                                            Undo.RecordObjects(obj, "Colliders Holders Position Changed");
                                            BCC.Position = V3;
                                            BCC.SetPosition();
                                            if ((SelectedColliderL == 2 || SelectedColliderL == 3) && SelectedColliderB != 2)
                                                BCCm.Position = new Vector3(V3.x, -V3.y, V3.z);
                                            else
                                                BCCm.Position = new Vector3(V3.x, V3.y, -V3.z);
                                            BCCm.SetPosition();
                                        }
                                        else
                                        {
                                            Object[] obj = { BCC, BCC.transform };
                                            Undo.RecordObjects(obj, "Colliders Holder Position Changed");
                                            BCC.Position = V3;
                                            BCC.SetPosition();
                                        }
                                    }

                                    //Mirroring Colliders options
                                    if (SelectedColliderL - 1 != 0 && BCCm)
                                    {
                                        if (BCCm.colliders == null)
                                        {
                                            Undo.RecordObject(BCCm, "Mirroring Collider Holder");
                                            BCCm.colliders = new Collider[BCC.colliders.Length];
                                        }
                                        else if (BCCm.colliders.Length != BCC.colliders.Length)
                                        {
                                            c = 0;
                                            while (c < BCCm.colliders.Length)
                                            {
                                                if (BCCm.colliders[c])
                                                    Undo.DestroyObjectImmediate(BCCm.colliders[c]);
                                                c++;
                                            }
                                            Undo.RecordObject(BCCm, "Mirroring Collider Holder");
                                            BCCm.colliders = new Collider[BCC.colliders.Length];
                                        }
                                        c = 0;
                                        while (c < BCC.colliders.Length)
                                        {
                                            if (BCC.colliders[c].GetType() == typeof(BoxCollider))
                                            {
                                                if (BCCm.colliders[c] == null)
                                                {
                                                    BCCm.colliders[c] = BCCm.gameObject.AddComponent<BoxCollider>();
                                                    (BCCm.colliders[c] as BoxCollider).center = (BCC.colliders[c] as BoxCollider).center;
                                                    (BCCm.colliders[c] as BoxCollider).size = (BCC.colliders[c] as BoxCollider).size;
                                                    Undo.RegisterCreatedObjectUndo(BCCm.colliders[c], "Mirroring Add Box Collider");
                                                }
                                                if (BCCm.colliders[c].GetType() != typeof(BoxCollider))
                                                {
                                                    Undo.DestroyObjectImmediate(BCCm.colliders[c]);
                                                    BCCm.colliders[c] = BCCm.gameObject.AddComponent<BoxCollider>();
                                                    (BCCm.colliders[c] as BoxCollider).center = (BCC.colliders[c] as BoxCollider).center;
                                                    (BCCm.colliders[c] as BoxCollider).size = (BCC.colliders[c] as BoxCollider).size;
                                                    Undo.RegisterCreatedObjectUndo(BCCm.colliders[c], "Mirroring Add Box Collider");
                                                }

                                                bool pressed = false;
                                                BCC.colliders = EditCollider("Box", BCC.colliders, BCC, c, ref pressed);
                                                if (pressed)
                                                {
                                                    BCCm.colliders = RemoveColliders("Boxes", BCCm.colliders, BCCm, c);
                                                    break;
                                                }

                                                EditorGUI.indentLevel++;
                                                BoxCollider BC1 = BCC.colliders[c] as BoxCollider;
                                                BoxCollider BC2 = BCCm.colliders[c] as BoxCollider;

                                                if ((SelectedColliderL == 2 || SelectedColliderL == 3) && SelectedColliderB != 2)
                                                {
                                                    if (BC2.center != new Vector3(BC1.center.x, -BC1.center.y, BC1.center.z))
                                                    {
                                                        Undo.RecordObject(BC2, "Mirroring Box Collider");
                                                        BC2.center = new Vector3(BC1.center.x, -BC1.center.y, BC1.center.z);
                                                    }
                                                }
                                                else
                                                {
                                                    if (BC2.center != new Vector3(-BC1.center.x, BC1.center.y, BC1.center.z))
                                                    {
                                                        Undo.RecordObject(BC2, "Mirroring Box Collider");
                                                        BC2.center = new Vector3(-BC1.center.x, BC1.center.y, BC1.center.z);
                                                    }
                                                }
                                                if (BC2.size != BC1.size)
                                                {
                                                    Undo.RecordObject(BC2, "Mirroring Box Collider");
                                                    BC2.size = BC1.size;
                                                }

                                                BC1.center = Vector3Field("Center", BC1.center, BC1, BC2);
                                                if ((SelectedColliderL == 2 || SelectedColliderL == 3) && SelectedColliderB != 2)
                                                    BC2.center = new Vector3(BC1.center.x, -BC1.center.y, BC1.center.z);
                                                else
                                                    BC2.center = new Vector3(-BC1.center.x, BC1.center.y, BC1.center.z);
                                                BC1.size = Vector3Field("Size", BC1.size, BC1, BC2);
                                                BC2.size = BC1.size;

                                                EditorGUI.indentLevel--;
                                            }
                                            else if (BCC.colliders[c].GetType() == typeof(SphereCollider))
                                            {
                                                if (BCCm.colliders[c] == null)
                                                {
                                                    BCCm.colliders[c] = BCCm.gameObject.AddComponent<SphereCollider>();
                                                    (BCCm.colliders[c] as SphereCollider).center = (BCC.colliders[c] as SphereCollider).center;
                                                    (BCCm.colliders[c] as SphereCollider).radius = (BCC.colliders[c] as SphereCollider).radius;
                                                    Undo.RegisterCreatedObjectUndo(BCCm.colliders[c], "Mirroring Add Sphere Collider");
                                                }
                                                if (BCCm.colliders[c].GetType() != typeof(SphereCollider))
                                                {
                                                    Undo.DestroyObjectImmediate(BCCm.colliders[c]);
                                                    BCCm.colliders[c] = BCCm.gameObject.AddComponent<SphereCollider>();
                                                    (BCCm.colliders[c] as SphereCollider).center = (BCC.colliders[c] as SphereCollider).center;
                                                    (BCCm.colliders[c] as SphereCollider).radius = (BCC.colliders[c] as SphereCollider).radius;
                                                    Undo.RegisterCreatedObjectUndo(BCCm.colliders[c], "Mirroring Add Sphere Collider");
                                                }

                                                bool pressed = false;
                                                BCC.colliders = EditCollider("Sphere", BCC.colliders, BCC, c, ref pressed);
                                                if (pressed)
                                                {
                                                    BCCm.colliders = RemoveColliders("Spheres", BCCm.colliders, BCCm, c);
                                                    break;
                                                }

                                                EditorGUI.indentLevel++;
                                                SphereCollider SC1 = BCC.colliders[c] as SphereCollider;
                                                SphereCollider SC2 = BCCm.colliders[c] as SphereCollider;

                                                if ((SelectedColliderL == 2 || SelectedColliderL == 3) && SelectedColliderB != 2)
                                                {
                                                    if (SC2.center != new Vector3(SC1.center.x, -SC1.center.y, SC1.center.z))
                                                    {
                                                        Undo.RecordObject(SC2, "Mirroring Sphere Collider");
                                                        SC2.center = new Vector3(SC1.center.x, -SC1.center.y, SC1.center.z);
                                                    }
                                                }
                                                else
                                                {
                                                    if (SC2.center != new Vector3(-SC1.center.x, SC1.center.y, SC1.center.z))
                                                    {
                                                        Undo.RecordObject(SC2, "Mirroring Sphere Collider");
                                                        SC2.center = new Vector3(-SC1.center.x, SC1.center.y, SC1.center.z);
                                                    }
                                                }
                                                if (SC2.radius != SC1.radius)
                                                {
                                                    Undo.RecordObject(SC2, "Mirroring Sphere Collider");
                                                    SC2.radius = SC1.radius;
                                                }

                                                SC1.center = Vector3Field("Center", SC1.center, SC1, SC2);
                                                if ((SelectedColliderL == 2 || SelectedColliderL == 3) && SelectedColliderB != 2)
                                                    SC2.center = new Vector3(SC1.center.x, -SC1.center.y, SC1.center.z);
                                                else
                                                    SC2.center = new Vector3(-SC1.center.x, SC1.center.y, SC1.center.z);
                                                SC1.radius = FloatField("Radius", SC1.radius, SC1, SC2);
                                                SC2.radius = SC1.radius;

                                                EditorGUI.indentLevel--;
                                            }
                                            else if (BCC.colliders[c].GetType() == typeof(CapsuleCollider))
                                            {
                                                if (BCCm.colliders[c] == null)
                                                {
                                                    BCCm.colliders[c] = BCCm.gameObject.AddComponent<CapsuleCollider>();
                                                    (BCCm.colliders[c] as CapsuleCollider).center = (BCC.colliders[c] as CapsuleCollider).center;
                                                    (BCCm.colliders[c] as CapsuleCollider).radius = (BCC.colliders[c] as CapsuleCollider).radius;
                                                    (BCCm.colliders[c] as CapsuleCollider).height = (BCC.colliders[c] as CapsuleCollider).height;
                                                    (BCCm.colliders[c] as CapsuleCollider).direction = (BCC.colliders[c] as CapsuleCollider).direction;
                                                    Undo.RegisterCreatedObjectUndo(BCCm.colliders[c], "Mirroring Add Capsule Collider");
                                                }
                                                if (BCCm.colliders[c].GetType() != typeof(CapsuleCollider))
                                                {
                                                    Undo.DestroyObjectImmediate(BCCm.colliders[c]);
                                                    BCCm.colliders[c] = BCCm.gameObject.AddComponent<CapsuleCollider>();
                                                    (BCCm.colliders[c] as CapsuleCollider).center = (BCC.colliders[c] as CapsuleCollider).center;
                                                    (BCCm.colliders[c] as CapsuleCollider).radius = (BCC.colliders[c] as CapsuleCollider).radius;
                                                    (BCCm.colliders[c] as CapsuleCollider).height = (BCC.colliders[c] as CapsuleCollider).height;
                                                    (BCCm.colliders[c] as CapsuleCollider).direction = (BCC.colliders[c] as CapsuleCollider).direction;
                                                    Undo.RegisterCreatedObjectUndo(BCCm.colliders[c], "Mirroring Add Capsule Collider");
                                                }

                                                bool pressed = false;
                                                BCC.colliders = EditCollider("Capsule", BCC.colliders, BCC, c, ref pressed);
                                                if (pressed)
                                                {
                                                    BCCm.colliders = RemoveColliders("Capsules", BCCm.colliders, BCCm, c);
                                                    break;
                                                }

                                                EditorGUI.indentLevel++;
                                                CapsuleCollider CC1 = BCC.colliders[c] as CapsuleCollider;
                                                CapsuleCollider CC2 = BCCm.colliders[c] as CapsuleCollider;

                                                if ((SelectedColliderL == 2 || SelectedColliderL == 3) && SelectedColliderB != 2)
                                                {
                                                    if (CC2.center != new Vector3(CC1.center.x, -CC1.center.y, CC1.center.z))
                                                    {
                                                        Undo.RecordObject(CC2, "Mirroring Capsules Collider");
                                                        CC2.center = new Vector3(CC1.center.x, -CC1.center.y, CC1.center.z);
                                                    }
                                                }
                                                else
                                                {
                                                    if (CC2.center != new Vector3(-CC1.center.x, CC1.center.y, CC1.center.z))
                                                    {
                                                        Undo.RecordObject(CC2, "Mirroring Capsules Collider");
                                                        CC2.center = new Vector3(-CC1.center.x, CC1.center.y, CC1.center.z);
                                                    }
                                                }
                                                if (CC2.radius != CC1.radius || CC2.height != CC1.height || CC2.direction != CC1.direction)
                                                {
                                                    Undo.RecordObject(CC2, "Mirroring Capsules Collider");
                                                    CC2.radius = CC1.radius;
                                                    CC2.height = CC1.height;
                                                    CC2.direction = CC1.direction;
                                                }

                                                CC1.center = Vector3Field("Center", CC1.center, CC1, CC2);
                                                if ((SelectedColliderL == 2 || SelectedColliderL == 3) && SelectedColliderB != 2)
                                                    CC2.center = new Vector3(CC1.center.x, -CC1.center.y, CC1.center.z);
                                                else
                                                    CC2.center = new Vector3(-CC1.center.x, CC1.center.y, CC1.center.z);
                                                CC1.radius = FloatField("Radius", CC1.radius, CC1, CC2);
                                                CC2.radius = CC1.radius;
                                                CC1.height = FloatField("Height", CC1.height, CC1, CC2);
                                                CC2.height = CC1.height;

                                                EditorGUILayout.BeginHorizontal();
                                                EditorGUILayout.PrefixLabel("Direction");
                                                string[] s = { "X", "Y", "Z" };
                                                int[] v = { 0, 1, 2 };
                                                EditorGUI.BeginChangeCheck();
                                                int value = EditorGUILayout.IntPopup(CC1.direction, s, v);
                                                if (EditorGUI.EndChangeCheck())
                                                {
                                                    Object[] obj = { CC1, CC2 };
                                                    Undo.RecordObjects(obj, "Mirroring Direction Changed");
                                                    CC1.direction = value;
                                                    CC2.direction = value;
                                                }

                                                EditorGUILayout.EndHorizontal();
                                                EditorGUI.indentLevel--;
                                            }
                                            c++;
                                        }
                                    }

                                    //Colliders options
                                    else
                                    {
                                        c = 0;
                                        while (c < BCC.colliders.Length)
                                        {
                                            if (BCC.colliders[c].GetType() == typeof(BoxCollider))
                                            {
                                                bool pressed = false;
                                                BCC.colliders = EditCollider("Box", BCC.colliders, BCC, c, ref pressed);
                                                if (pressed)
                                                    break;

                                                EditorGUI.indentLevel++;
                                                BoxCollider BC = BCC.colliders[c] as BoxCollider;

                                                BC.center = Vector3Field("Center", BC.center, BC, null);
                                                BC.size = Vector3Field("Size", BC.size, BC, null);

                                                EditorGUI.indentLevel--;
                                            }
                                            else if (BCC.colliders[c].GetType() == typeof(SphereCollider))
                                            {
                                                bool pressed = false;
                                                BCC.colliders = EditCollider("Sphere", BCC.colliders, BCC, c, ref pressed);
                                                if (pressed)
                                                    break;

                                                EditorGUI.indentLevel++;
                                                SphereCollider SC = BCC.colliders[c] as SphereCollider;

                                                SC.center = Vector3Field("Center", SC.center, SC, null);
                                                SC.radius = FloatField("Radius", SC.radius, SC, null);

                                                EditorGUI.indentLevel--;
                                            }
                                            else if (BCC.colliders[c].GetType() == typeof(CapsuleCollider))
                                            {
                                                bool pressed = false;
                                                BCC.colliders = EditCollider("Capsule", BCC.colliders, BCC, c, ref pressed);
                                                if (pressed)
                                                    break;

                                                EditorGUI.indentLevel++;
                                                CapsuleCollider CC = BCC.colliders[c] as CapsuleCollider;

                                                CC.center = Vector3Field("Center", CC.center, CC, null);
                                                CC.radius = FloatField("Radius", CC.radius, CC, null);
                                                CC.height = FloatField("Height", CC.height, CC, null);

                                                EditorGUILayout.BeginHorizontal();
                                                EditorGUILayout.PrefixLabel("Direction");
                                                string[] s = { "X", "Y", "Z" };
                                                int[] v = { 0, 1, 2 };
                                                EditorGUI.BeginChangeCheck();
                                                int value = EditorGUILayout.IntPopup(CC.direction, s, v);
                                                if (EditorGUI.EndChangeCheck())
                                                {
                                                    Undo.RecordObject(CC, "Direction Changed");
                                                    CC.direction = value;
                                                }

                                                EditorGUILayout.EndHorizontal();
                                                EditorGUI.indentLevel--;
                                            }
                                            c++;
                                        }
                                    }
                                    EditorGUILayout.LabelField("Add Collider:");
                                    EditorGUILayout.BeginHorizontal();
                                    if (GUILayout.Button(new GUIContent("Box", "Add Box collider to the bone.")))
                                    {
                                        BCC.colliders = AddCollider("Box", BCC.colliders, BCC, BCC.gameObject);
                                        if (BCCm)
                                            BCCm.colliders = AddCollider("Boxes", BCCm.colliders, BCCm, BCCm.gameObject);
                                    }
                                    if (GUILayout.Button(new GUIContent("Sphere", "Add Sphere collider to the bone.")))
                                    {
                                        BCC.colliders = AddCollider("Sphere", BCC.colliders, BCC, BCC.gameObject);
                                        if (BCCm)
                                            BCCm.colliders = AddCollider("Spheres", BCCm.colliders, BCCm, BCCm.gameObject);
                                    }
                                    if (GUILayout.Button(new GUIContent("Capsule", "Add Capsule collider to the bone.")))
                                    {
                                        BCC.colliders = AddCollider("Capsule", BCC.colliders, BCC, BCC.gameObject);
                                        if (BCCm)
                                            BCCm.colliders = AddCollider("Capsules", BCCm.colliders, BCCm, BCCm.gameObject);
                                    }
                                    EditorGUILayout.EndHorizontal();
                                    EditorGUILayout.LabelField("");
                                    EditorGUI.indentLevel--;
                                    EditorGUI.indentLevel--;
                                }
                            }
                        }
                    }

                    if (GUILayout.Button(new GUIContent("Create Ragdoll", "Create Ragdoll now.")))
                    {
                        Object[] obj = { MTR_I.gameObject, MTR_I };
                        Undo.RegisterCompleteObjectUndo(obj, "Create Ragdoll");
                        bool CreateColliders = true;
                        if (MTR_I.GetComponentsInChildren<MTR_BoneCollidersControl>().Length > 0)
                            CreateColliders = false;
                        MTR_I.CreateRagdoll();
                        if (CreateColliders)
                        {
                            foreach (MTR_BoneCollidersControl BCC in MTR_I.GetComponentsInChildren<MTR_BoneCollidersControl>())
                            {
                                Undo.RegisterCreatedObjectUndo(BCC.gameObject, "Create Ragdoll");
                                foreach (Collider Col in BCC.colliders)
                                    Undo.RegisterCreatedObjectUndo(Col, "Create Ragdoll");
                            }
                        }
                        foreach (MTR_BoneControl BC in MTR_I.GetComponentsInChildren<MTR_BoneControl>())
                            Undo.RegisterCreatedObjectUndo(BC, "Create Ragdoll");

                        Undo.RegisterCreatedObjectUndo(MTR_I.GetComponent<MTR_Control>(), "Create Ragdoll");
                        if (MTR_I.JustRagdoll)
                        {
                            Undo.DestroyObjectImmediate(MTR_I.GetComponent<Animator>());
                            if (RB)
                                Undo.DestroyObjectImmediate(RB);
                            Collider[] C = MTR_I.GetComponents<Collider>();
                            if (C != null)
                                foreach (Collider nC in C)
                                    Undo.DestroyObjectImmediate(nC);
                        }
                        Undo.DestroyObjectImmediate(MTR_I);
                    }
                }
            }
            else
                EditorGUILayout.HelpBox("The animation type is not Humanoid.", MessageType.Warning);
        }
    }

    private void UndoRegister(string name, Object obj1, Object obj2)
    {
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(MTR_I);
            if (obj2)
            {
                Object[] obj = { obj1, obj2 };
                Undo.RegisterCompleteObjectUndo(obj, "Mirroring " + name + " Changed");
            }
            else
                Undo.RegisterCompleteObjectUndo(obj1, name + " Changed");
        }
    }

    private bool Toggle(GUIContent content, bool value, Object obj1, Object obj2)
    {
        EditorGUI.BeginChangeCheck();
        value = EditorGUILayout.Toggle(content, value);
        UndoRegister(content.text, obj1, obj2);
        return value;
    }

    private float FloatField(string name, float value, Object obj1, Object obj2)
    {
        EditorGUI.BeginChangeCheck();
        value = EditorGUILayout.FloatField(name, value);
        UndoRegister(name, obj1, obj2);
        return value;
    }

    private float FloatField(GUIContent content, float value, Object obj1, Object obj2)
    {
        EditorGUI.BeginChangeCheck();
        value = EditorGUILayout.FloatField(content, value);
        UndoRegister(content.text, obj1, obj2);
        return value;
    }

    private Vector3 Vector3Field(string name, Vector3 value, Object obj1, Object obj2)
    {
        EditorGUI.BeginChangeCheck();
        value = EditorGUILayout.Vector3Field(name, value);
        UndoRegister(name, obj1, obj2);
        return value;
    }

    private string TextField(GUIContent content, string value, Object obj1, Object obj2)
    {
        EditorGUI.BeginChangeCheck();
        value = EditorGUILayout.TextField(content, value);
        UndoRegister(content.text, obj1, obj2);
        return value;
    }

    private Collider[] EditCollider(string name, Collider[] colliders, Object obj, int id, ref bool pressed)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(id + 1 + ". " + name + " Collider:");

        string S = "Edit";
        string St = "Edit the collider in the \"Scene\" window. \nMove in space with the \"Move tool\" and change the size parameters with the \"Scale tool\".";
        if (HandleObject == colliders[id])
        {
            S = "Editing";
            St = "Disable collider editing in the \"Scene\" window. \n\nDescription: You can now edit the collider in the Scene window. After clicking on this button, you will not be able to.";
        }
        if (GUILayout.Button(new GUIContent(S, St)))
        {
            if (Tools.current == Tool.None || Tools.current == Tool.View || Tools.current == Tool.Rotate || Tools.current == Tool.Move)
                Tools.current = Tool.Move;
            else
                Tools.current = Tool.Scale;
            if (HandleObject != colliders[id])
                HandleObject = colliders[id];
            else
                HandleObject = null;
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Remove"))
        {
            colliders = RemoveColliders(name, colliders, obj, id);
            pressed = true;
        }
        EditorGUILayout.EndHorizontal();
        return colliders;
    }

    private Collider[] RemoveColliders(string name, Collider[] colliders, Object obj, int id)
    {

        Collider[] Temp = new Collider[colliders.Length - 1];
        int i = 0;
        while (i != id)
        {
            Temp[i] = colliders[i];
            i++;
        }
        Undo.DestroyObjectImmediate(colliders[i]);

        while (i < Temp.Length)
        {
            Temp[i] = colliders[i + 1];
            i++;
        }
        Undo.RecordObject(obj, "Remove " + name + " Collider");
        colliders = Temp;

        return colliders;
    }

    private Collider[] AddCollider(string name, Collider[] colliders, Object obj, GameObject GO)
    {
        Collider[] Temp = new Collider[colliders.Length + 1];
        int i = 0;
        while (i < colliders.Length)
        {
            Temp[i] = colliders[i];
            i++;
        }
        if (name == "Box" || name == "Boxes")
            Temp[i] = GO.AddComponent<BoxCollider>();
        else if (name == "Sphere" || name == "Spheres")
            Temp[i] = GO.AddComponent<SphereCollider>();
        else if (name == "Capsule" || name == "Capsules")
            Temp[i] = GO.AddComponent<CapsuleCollider>();

        Undo.RegisterCreatedObjectUndo(Temp[i], "Add " + name + " Collider");
        Undo.RecordObject(obj, "Add " + name + " Collider");
        colliders = Temp;

        return colliders;
    }

    private void OnSceneGUI()
    {
        if (HandleObject != null)
        {
            if (Tools.current == Tool.Move)
            {
                if (HandleObject.GetType() == typeof(MTR_BoneCollidersControl))
                {
                    MTR_BoneCollidersControl BCC = HandleObject as MTR_BoneCollidersControl;
                    Quaternion Q = Quaternion.LookRotation(BCC.transform.parent.TransformDirection(BCC.DirectionZ), BCC.transform.parent.TransformDirection(BCC.DirectionY));
                    Vector3 V3 = Quaternion.Inverse(Q) * (Handles.PositionHandle(BCC.transform.position, Q) - BCC.transform.parent.position);
                    if (V3 != BCC.Position)
                    {
                        Object[] obj = { BCC, BCC.transform };
                        Undo.RecordObjects(obj, "Colliders Holder Position Changed");
                        BCC.Position = V3;
                        BCC.SetPosition();
                        Repaint();
                    }
                }
                else if (HandleObject.GetType() == typeof(BoxCollider))
                {
                    BoxCollider BC = HandleObject as BoxCollider;
                    Vector3 V3 = BC.transform.InverseTransformPoint(Handles.PositionHandle(BC.transform.TransformPoint(BC.center), BC.transform.rotation));
                    if (V3 != BC.center)
                    {
                        EditorUtility.SetDirty(BC);
                        Undo.RegisterCompleteObjectUndo(BC, "Center Changed");
                        BC.center = V3;
                        Repaint();
                    }
                }
                else if (HandleObject.GetType() == typeof(SphereCollider))
                {
                    SphereCollider SC = HandleObject as SphereCollider;
                    Vector3 V3 = SC.transform.InverseTransformPoint(Handles.PositionHandle(SC.transform.TransformPoint(SC.center), SC.transform.rotation));
                    if (V3 != SC.center)
                    {
                        EditorUtility.SetDirty(SC);
                        Undo.RegisterCompleteObjectUndo(SC, "Center Changed");
                        SC.center = V3;
                        Repaint();
                    }
                }
                else if (HandleObject.GetType() == typeof(CapsuleCollider))
                {
                    CapsuleCollider CC = HandleObject as CapsuleCollider;
                    Vector3 V3 = CC.transform.InverseTransformPoint(Handles.PositionHandle(CC.transform.TransformPoint(CC.center), CC.transform.rotation));
                    if (V3 != CC.center)
                    {
                        EditorUtility.SetDirty(CC);
                        Undo.RegisterCompleteObjectUndo(CC, "Center Changed");
                        CC.center = V3;
                        Repaint();
                    }
                }
            }
            else if (Tools.current == Tool.Scale)
            {
                if (HandleObject.GetType() == typeof(BoxCollider))
                {
                    BoxCollider BC = HandleObject as BoxCollider;
                    Vector3 V3 = Handles.ScaleHandle(BC.size, BC.transform.TransformPoint(BC.center), BC.transform.rotation, Mathf.Max(Mathf.Max(BC.size.x, BC.size.y), BC.size.z));
                    if (V3 != BC.size)
                    {
                        EditorUtility.SetDirty(BC);
                        Undo.RegisterCompleteObjectUndo(BC, "Size Changed");
                        BC.size = V3;
                        Repaint();
                    }
                }
                else if (HandleObject.GetType() == typeof(SphereCollider))
                {
                    SphereCollider SC = HandleObject as SphereCollider;
                    Vector3 V3t = Vector3.one * SC.radius;
                    Vector3 V3 = Handles.ScaleHandle(V3t, SC.transform.TransformPoint(SC.center), SC.transform.rotation, SC.radius * 2F);
                    if (V3 != V3t)
                    {
                        EditorUtility.SetDirty(SC);
                        Undo.RegisterCompleteObjectUndo(SC, "Size Changed");
                        if (V3.x == V3.y && V3.y == V3.z)
                            SC.radius = V3.x;
                        else
                        {
                            if (V3.x == V3.y)
                                SC.radius = V3.z;
                            else if (V3.y == V3.z)
                                SC.radius = V3.x;
                            else
                                SC.radius = V3.y;
                        }
                        Repaint();
                    }
                }
                else if (HandleObject.GetType() == typeof(CapsuleCollider))
                {
                    CapsuleCollider CC = HandleObject as CapsuleCollider;
                    Vector3 V3t;
                    Vector3 V3;
                    float S = Mathf.Max(CC.radius, CC.height);
                    if (CC.direction == 0)
                    {
                        V3t = new Vector3(CC.height, CC.radius, CC.radius);
                        V3 = Handles.ScaleHandle(V3t, CC.transform.TransformPoint(CC.center), CC.transform.rotation, S);
                        if (V3t != V3)
                        {
                            EditorUtility.SetDirty(CC);
                            Undo.RegisterCompleteObjectUndo(CC, "Size Changed");
                            if (V3t.x != V3.x)
                                CC.height = V3.x;
                            if (V3t.y != V3.y)
                                CC.radius = V3.y;
                            else if (V3t.z != V3.z)
                                CC.radius = V3.z;
                            Repaint();
                        }
                    }
                    else if (CC.direction == 1)
                    {
                        V3t = new Vector3(CC.radius, CC.height, CC.radius);
                        V3 = Handles.ScaleHandle(V3t, CC.transform.TransformPoint(CC.center), CC.transform.rotation, S);
                        if (V3t != V3)
                        {
                            EditorUtility.SetDirty(CC);
                            Undo.RegisterCompleteObjectUndo(CC, "Size Changed");
                            if (V3t.y != V3.y)
                                CC.height = V3.y;
                            if (V3t.x != V3.x)
                                CC.radius = V3.x;
                            else if (V3t.z != V3.z)
                                CC.radius = V3.z;
                            Repaint();
                        }
                    }
                    else
                    {
                        V3t = new Vector3(CC.radius, CC.radius, CC.height);
                        V3 = Handles.ScaleHandle(V3t, CC.transform.TransformPoint(CC.center), CC.transform.rotation, S);
                        if (V3t != V3)
                        {
                            EditorUtility.SetDirty(CC);
                            Undo.RegisterCompleteObjectUndo(CC, "Size Changed");
                            if (V3t.z != V3.z)
                                CC.height = V3.z;
                            if (V3t.x != V3.x)
                                CC.radius = V3.x;
                            else if (V3t.y != V3.y)
                                CC.radius = V3.y;
                            Repaint();
                        }
                    }
                }
            }
        }
    }
}