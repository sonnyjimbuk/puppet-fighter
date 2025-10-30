using UnityEngine;

[RequireComponent(typeof(Animator))]
/// <summary>
/// From the settings of Mecanim makes ragdoll.
/// </summary>
public class MecanimToRagdoll : MonoBehaviour
{
    /// <summary>
    /// If "true" - just a ragdoll without the ability to animate.
    /// </summary>
    public bool JustRagdoll;

    /// <summary>
    /// If "true" - an animation with the ability to switch to a ragdoll. If "false" - a ragdoll with the ability to switch to animation.
    /// </summary>
    public bool Animate = true;

    /// <summary>
    /// If "true" - пenerates default colliders.
    /// </summary>
    public bool GenerateCollidersOnStart = true;

    /// <summary>
    /// Body mass. It is taken from the "Rigidbody" of root if it is.
    /// </summary>
    public float Mass = 60;

    /// <summary>
    /// Body mass. It is taken from the "Rigidbody" of root if it is.
    /// </summary>
    public float ResurrectingTime = 1.5F;

    /// <summary>
    /// Add character velocity to character bones when it becomes a ragdoll.
    /// </summary>
    public bool AddVelocity = true;

    /// <summary>
    /// Rigidbodys Drag.
    /// </summary>
    public float Drag = 0;

    /// <summary>
    /// Rigidbodys AngularDrag.
    /// </summary>
    public float AngularDrag = 0.05F;

    /// <summary>
    /// Rigidbodys Interpolation.
    /// </summary>
    public RigidbodyInterpolation RBI = RigidbodyInterpolation.None;

    /// <summary>
    /// Rigidbodys collision detection mode.
    /// </summary>
    public CollisionDetectionMode RBCDM = CollisionDetectionMode.Discrete;

    /// <summary>
    /// Character Joint Spring.
    /// </summary>
    public float ChJSpring = 10000;

    /// <summary>
    /// Character Joint Damper.
    /// </summary>
    public float ChJDamper = 100;

    /// <summary>
    /// Character Preprocessing.
    /// </summary>
    public bool ChJPreprocessing = true;

    /// <summary>
    /// T-pose prefab fron Mecanim.
    /// </summary>
    public GameObject TPose;
    private GameObject TPoseInScene;

    /// <summary>
    /// Muscles pose prefab fron Mecanim.
    /// </summary>
    public GameObject MusclesPose;
    private GameObject MusclesPoseInScene;

    /// <summary>
    /// List of scripts responsible for setting up colliders on bones.
    /// </summary>
    public MTR_BoneCollidersControl[][] BoneCollidersControl;

    /// <summary>
    /// Physic material of colliders on bones.
    /// </summary>
    public PhysicsMaterial CollidersMaterial;

    /// <summary>
    /// The name of the trigger parameter in the animator controller that indicates that the ragdoll is "resurrecting".
    /// </summary>
    public string AnimatorResurrectTriggerName = "Resurrect";

    /// <summary>
    /// The name of the bool parameter in the animator controller that indicates that the ragdoll is lying on its back.
    /// </summary>
    public string AnimatorOnBackBoolName = "OnBack";

    private Animator animator;
    private GameObject[][] Bones;
    private GameObject[][] BonesTPose;
    private GameObject[][] BonesMPose;
    private Vector3[][] Axis;
    private Vector3[][] SwingAxis;
    private Vector3[][] LimitMin;
    private Vector3[][] LimitMax;

    private MTR_BoneControl HipsMTRBC = null;
    private MTR_BoneControl LArmMTRBC = null;
    private MTR_BoneControl RArmMTRBC = null;

    private void DestroyComponents(GameObject GO)
    {
        foreach (Component c in GO.GetComponents<Component>())
        {
            if (c.GetType() != typeof(Transform) && c.GetType() != typeof(Rigidbody) && c.GetType() != typeof(MecanimToRagdoll) && c.GetType() != typeof(MTR_Control))
                Destroy(c);
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Collider generation in the editor.
    /// </summary>
    public void GenerateColliders()
    {
        animator = gameObject.GetComponent<Animator>();

        if (TPose && MusclesPose && animator)
        {
            GetBones();
            GetAxisAndLimits();
            AddDefaultColliders();
            DeletePrefabsInScene();
        }
        else
            Warnings();
    }

    /// <summary>
    /// Create a ragdoll blank in the editor.
    /// </summary>
    public void CreateRagdoll()
    {
        animator = gameObject.GetComponent<Animator>();

        if (TPose && MusclesPose && animator)
        {
            GetBones();
            GetAxisAndLimits();
            if (GenerateCollidersOnStart)
            {
                if (BoneCollidersControl == null)
                    AddDefaultColliders();
                else
                {
                    if (BoneCollidersControl.Length == 0)
                        AddDefaultColliders();
                }
            }
            SetIDsAndMaterial();
            AddConnections();
            FreeRigidbodies();
            DeletePrefabsInScene();
        }
        else
            Warnings();
    }
#endif

    /// <summary>
    /// Getting default colliders for editing in the editor.
    /// </summary>
    public void GetDefaultColliders()
    {
        animator = gameObject.GetComponent<Animator>();

        if (TPose && MusclesPose && animator)
        {
            GetBones();

            BoneCollidersControl = new MTR_BoneCollidersControl[5][];
            BoneCollidersControl[0] = new MTR_BoneCollidersControl[6];
            BoneCollidersControl[1] = new MTR_BoneCollidersControl[4];
            BoneCollidersControl[2] = new MTR_BoneCollidersControl[4];
            BoneCollidersControl[3] = new MTR_BoneCollidersControl[4];
            BoneCollidersControl[4] = new MTR_BoneCollidersControl[4];
            int l = 0;
            while (l < 5)
            {
                int b = 0;
                while (b < Bones[l].Length)
                {
                    if (Bones[l][b])
                    {
                        MTR_BoneCollidersControl BCC = null;
                        int i = 0;
                        while (i < Bones[l][b].transform.childCount)
                        {
                            BCC = Bones[l][b].transform.GetChild(i).GetComponent<MTR_BoneCollidersControl>();
                            if (BCC)
                                break;

                            i++;
                        }
                        if (BCC)
                            BoneCollidersControl[l][b] = BCC;
                    }
                    b++;
                }
                l++;
            }
            DeletePrefabsInScene();
        }
        else
            Warnings();
    }

    //Crutch for new versions of Unity
    private void DeletePrefabsInScene()
    {
        if (Application.isEditor)
        {
            if (TPoseInScene != null)
                DestroyImmediate(TPoseInScene);
            if (MusclesPoseInScene != null)
                DestroyImmediate(MusclesPoseInScene);
        }
        else if (Application.isPlaying)
        {
            if (TPoseInScene != null)
                Destroy(TPoseInScene);
            if (MusclesPoseInScene != null)
                Destroy(MusclesPoseInScene);
        }
    }
    private void SetIDsAndMaterial()
    {
        int l = 0;
        while (l < 5)
        {
            int b = 0;
            while (b < Bones[l].Length)
            {
                if (Bones[l][b])
                {
                    int i = 0;
                    while (i < Bones[l][b].transform.childCount)
                    {
                        MTR_BoneCollidersControl BCC = Bones[l][b].transform.GetChild(i).GetComponent<MTR_BoneCollidersControl>();
                        if (BCC)
                        {
                            BCC.ID = l * 10 + b;
                            foreach (Collider c in BCC.colliders)
                                c.material = CollidersMaterial;
                            break;
                        }
                        i++;
                    }
                }
                b++;
            }
            l++;
        }
    }

    private void Start()
    {
        animator = gameObject.GetComponent<Animator>();

        if (TPose && MusclesPose && animator)
        {
            GetBones();
            GetAxisAndLimits();
            if (GenerateCollidersOnStart)
            {
                if (BoneCollidersControl == null)
                    AddDefaultColliders();
                else
                {
                    if (BoneCollidersControl.Length == 0)
                        AddDefaultColliders();
                }
            }
            SetIDsAndMaterial();
            AddConnections();
            FreeRigidbodies();
            DeletePrefabsInScene();
        }
        else
            Warnings();
    }

    private void GetBones()
    {
        Bones = new GameObject[5][];
        BonesTPose = new GameObject[5][];
        BonesMPose = new GameObject[5][];

        Bones[0] = new GameObject[6];
        BonesTPose[0] = new GameObject[6];
        BonesMPose[0] = new GameObject[6];
        GetBone(HumanBodyBones.Head, 0, 0);
        GetBone(HumanBodyBones.Neck, 0, 1);
        GetBone(HumanBodyBones.UpperChest, 0, 2);
        GetBone(HumanBodyBones.Chest, 0, 3);
        GetBone(HumanBodyBones.Spine, 0, 4);
        GetBone(HumanBodyBones.Hips, 0, 5);

        Bones[1] = new GameObject[4];
        BonesTPose[1] = new GameObject[4];
        BonesMPose[1] = new GameObject[4];
        GetBone(HumanBodyBones.LeftHand, 1, 0);
        GetBone(HumanBodyBones.LeftLowerArm, 1, 1);
        GetBone(HumanBodyBones.LeftUpperArm, 1, 2);
        GetBone(HumanBodyBones.LeftShoulder, 1, 3);

        Bones[2] = new GameObject[4];
        BonesTPose[2] = new GameObject[4];
        BonesMPose[2] = new GameObject[4];
        GetBone(HumanBodyBones.RightHand, 2, 0);
        GetBone(HumanBodyBones.RightLowerArm, 2, 1);
        GetBone(HumanBodyBones.RightUpperArm, 2, 2);
        GetBone(HumanBodyBones.RightShoulder, 2, 3);

        Bones[3] = new GameObject[4];
        BonesTPose[3] = new GameObject[4];
        BonesMPose[3] = new GameObject[4];
        GetBone(HumanBodyBones.LeftToes, 3, 0);
        GetBone(HumanBodyBones.LeftFoot, 3, 1);
        GetBone(HumanBodyBones.LeftLowerLeg, 3, 2);
        GetBone(HumanBodyBones.LeftUpperLeg, 3, 3);

        Bones[4] = new GameObject[4];
        BonesTPose[4] = new GameObject[4];
        BonesMPose[4] = new GameObject[4];
        GetBone(HumanBodyBones.RightToes, 4, 0);
        GetBone(HumanBodyBones.RightFoot, 4, 1);
        GetBone(HumanBodyBones.RightLowerLeg, 4, 2);
        GetBone(HumanBodyBones.RightUpperLeg, 4, 3);
    }

    private void GetBone(HumanBodyBones HBBone, int layer, int bone)
    {
        Transform T = animator.GetBoneTransform(HBBone);
        if (T)
        {
            Bones[layer][bone] = T.gameObject;

            if (TPoseInScene == null && TPose.GetComponent<Animator>().GetBoneTransform(HBBone) == null)
                TPoseInScene = GameObject.Instantiate(TPose);
            if (MusclesPoseInScene == null && MusclesPose.GetComponent<Animator>().GetBoneTransform(HBBone) == null)
                MusclesPoseInScene = GameObject.Instantiate(MusclesPose);

            if (TPoseInScene != null)
                BonesTPose[layer][bone] = TPoseInScene.GetComponent<Animator>().GetBoneTransform(HBBone).gameObject;
            else
                BonesTPose[layer][bone] = TPose.GetComponent<Animator>().GetBoneTransform(HBBone).gameObject;
            if (MusclesPoseInScene != null)
                BonesMPose[layer][bone] = MusclesPoseInScene.GetComponent<Animator>().GetBoneTransform(HBBone).gameObject;
            else
                BonesMPose[layer][bone] = MusclesPose.GetComponent<Animator>().GetBoneTransform(HBBone).gameObject;
        }
    }

    private void GetAxisAndLimits()
    {
        Axis = new Vector3[5][];
        SwingAxis = new Vector3[5][];
        LimitMin = new Vector3[5][];
        LimitMax = new Vector3[5][];

        Avatar A = TPose.GetComponent<Animator>().avatar;

        int al = 0;
        while (al < 5)
        {
            if (Bones[al] != null)
            {
                LimitMin[al] = new Vector3[Bones[al].Length];
                LimitMax[al] = new Vector3[Bones[al].Length];
                Axis[al] = new Vector3[Bones[al].Length];
                SwingAxis[al] = new Vector3[Bones[al].Length];

                int b = 0;
                while (b < Bones[al].Length)
                {
                    if (Bones[al][b] != null)
                    {
                        int l = 0;
                        while (l < A.humanDescription.human.Length)
                        {
                            if (Bones[al][b].name == A.humanDescription.human[l].boneName)
                            {
                                Transform TP = BonesTPose[al][b].transform;

                                Vector3 axis = Vector3.up;
                                Vector3 swingAxis;
                                if (al == 0)
                                {
                                    if (b != 0)
                                    {
                                        int g = b - 1;
                                        while (g >= 0)
                                        {
                                            if (BonesTPose[al][g] != null)
                                                break;
                                            g--;
                                        }
                                        axis = (BonesTPose[al][g].transform.position - TP.position).normalized;
                                    }
                                    swingAxis = Vector3.Cross(Vector3.right, axis);
                                }
                                else if (al == 1 || al == 2)
                                {
                                    if (b == 0)
                                        axis = (TP.position - BonesTPose[al][b + 1].transform.position).normalized;
                                    else
                                        axis = (BonesTPose[al][b - 1].transform.position - TP.position).normalized;

                                    swingAxis = -Vector3.Cross(Vector3.forward, axis);
                                    if (b == 1)
                                        swingAxis = -Vector3.Cross(Vector3.Cross(Vector3.forward, axis).normalized, axis);
                                }
                                else
                                {
                                    if (b == 0)
                                    {
                                        axis = Vector3.forward;
                                        swingAxis = Vector3.up;
                                    }
                                    else if (b == 1)
                                    {
                                        axis = -Vector3.up;
                                        swingAxis = Vector3.forward;
                                    }
                                    else
                                    {
                                        int g = b - 1;
                                        while (g > 0)
                                        {
                                            if (BonesTPose[al][g] != null)
                                                break;
                                            g--;
                                        }
                                        axis = (BonesTPose[al][g].transform.position - TP.position).normalized;
                                        if (b == 2)
                                            swingAxis = Vector3.Cross(Vector3.right, axis);
                                        else
                                            swingAxis = -Vector3.Cross(Vector3.right, axis);
                                    }
                                }

                                axis = TP.InverseTransformDirection(axis.normalized);
                                swingAxis = TP.InverseTransformDirection(swingAxis.normalized);

                                if (animator.avatar.humanDescription.human[l].humanName != "Hips")
                                {
                                    Vector3 limitMin = animator.avatar.humanDescription.human[l].limit.min;
                                    Vector3 limitMax = animator.avatar.humanDescription.human[l].limit.max;

                                    if (animator.avatar.humanDescription.human[l].limit.useDefaultValues)
                                    {
                                        int boneID = 0;
                                        string Bname = animator.avatar.humanDescription.human[l].humanName;
                                        if (HumanBodyBones.Head.ToString() == Bname)
                                            boneID = 10;
                                        else if (HumanBodyBones.Neck.ToString() == Bname)
                                            boneID = 9;
                                        else if (HumanBodyBones.UpperChest.ToString() == Bname)
                                            boneID = 54;
                                        else if (HumanBodyBones.Chest.ToString() == Bname)
                                            boneID = 8;
                                        else if (HumanBodyBones.Spine.ToString() == Bname)
                                            boneID = 7;
                                        else if (HumanBodyBones.LeftHand.ToString() == Bname)
                                            boneID = 17;
                                        else if (HumanBodyBones.LeftLowerArm.ToString() == Bname)
                                            boneID = 15;
                                        else if (HumanBodyBones.LeftUpperArm.ToString() == Bname)
                                            boneID = 13;
                                        else if (HumanBodyBones.LeftShoulder.ToString() == Bname)
                                            boneID = 11;
                                        else if (HumanBodyBones.RightHand.ToString() == Bname)
                                            boneID = 18;
                                        else if (HumanBodyBones.RightLowerArm.ToString() == Bname)
                                            boneID = 16;
                                        else if (HumanBodyBones.RightUpperArm.ToString() == Bname)
                                            boneID = 14;
                                        else if (HumanBodyBones.RightShoulder.ToString() == Bname)
                                            boneID = 12;
                                        else if (HumanBodyBones.LeftToes.ToString() == Bname)
                                            boneID = 19;
                                        else if (HumanBodyBones.LeftFoot.ToString() == Bname)
                                            boneID = 5;
                                        else if (HumanBodyBones.LeftLowerLeg.ToString() == Bname)
                                            boneID = 3;
                                        else if (HumanBodyBones.LeftUpperLeg.ToString() == Bname)
                                            boneID = 1;
                                        else if (HumanBodyBones.RightToes.ToString() == Bname)
                                            boneID = 20;
                                        else if (HumanBodyBones.RightFoot.ToString() == Bname)
                                            boneID = 6;
                                        else if (HumanBodyBones.RightLowerLeg.ToString() == Bname)
                                            boneID = 4;
                                        else if (HumanBodyBones.RightUpperLeg.ToString() == Bname)
                                            boneID = 2;


                                        if (boneID == 1 || boneID == 2 || boneID == 7 || boneID == 8 || boneID == 9 || boneID == 10 || boneID == 13 || boneID == 14 || boneID == 54)
                                        {
                                            limitMin = new Vector3(HumanTrait.GetMuscleDefaultMin(HumanTrait.MuscleFromBone(boneID, 0)), HumanTrait.GetMuscleDefaultMin(HumanTrait.MuscleFromBone(boneID, 1)), HumanTrait.GetMuscleDefaultMin(HumanTrait.MuscleFromBone(boneID, 2)));
                                            limitMax = new Vector3(HumanTrait.GetMuscleDefaultMax(HumanTrait.MuscleFromBone(boneID, 0)), HumanTrait.GetMuscleDefaultMax(HumanTrait.MuscleFromBone(boneID, 1)), HumanTrait.GetMuscleDefaultMax(HumanTrait.MuscleFromBone(boneID, 2)));
                                        }
                                        else if (boneID == 11 || boneID == 12 || boneID == 5 || boneID == 6 || boneID == 17 || boneID == 18)
                                        {
                                            limitMin = new Vector3(0, HumanTrait.GetMuscleDefaultMin(HumanTrait.MuscleFromBone(boneID, 1)), HumanTrait.GetMuscleDefaultMin(HumanTrait.MuscleFromBone(boneID, 2)));
                                            limitMax = new Vector3(0, HumanTrait.GetMuscleDefaultMax(HumanTrait.MuscleFromBone(boneID, 1)), HumanTrait.GetMuscleDefaultMax(HumanTrait.MuscleFromBone(boneID, 2)));
                                        }
                                        else if (boneID == 3 || boneID == 4 || boneID == 15 || boneID == 16)
                                        {
                                            limitMin = new Vector3(HumanTrait.GetMuscleDefaultMin(HumanTrait.MuscleFromBone(boneID, 0)), 0, HumanTrait.GetMuscleDefaultMin(HumanTrait.MuscleFromBone(boneID, 2)));
                                            limitMax = new Vector3(HumanTrait.GetMuscleDefaultMax(HumanTrait.MuscleFromBone(boneID, 0)), 0, HumanTrait.GetMuscleDefaultMax(HumanTrait.MuscleFromBone(boneID, 2)));
                                        }
                                        else
                                        {
                                            limitMin = new Vector3(0, 0, HumanTrait.GetMuscleDefaultMin(HumanTrait.MuscleFromBone(boneID, 2)));
                                            limitMax = new Vector3(0, 0, HumanTrait.GetMuscleDefaultMax(HumanTrait.MuscleFromBone(boneID, 2)));
                                        }
                                    }
                                    LimitMin[al][b] = limitMin;
                                    LimitMax[al][b] = limitMax;

                                    if (al == 0 || al == 3)
                                    {
                                        LimitMin[al][b].x = -limitMax.x;
                                        LimitMax[al][b].x = -limitMin.x;
                                    }
                                    else if (al == 1)
                                    {
                                        LimitMin[al][b].x = -limitMax.x;
                                        LimitMax[al][b].x = -limitMin.x;

                                        if (b != 1)
                                        {
                                            LimitMin[al][b].z = -limitMax.z;
                                            LimitMax[al][b].z = -limitMin.z;
                                        }
                                    }
                                    else if (al == 4)
                                    {
                                        LimitMin[al][b].y = -limitMax.y;
                                        LimitMax[al][b].y = -limitMin.y;
                                    }
                                }
                                Axis[al][b] = axis;
                                SwingAxis[al][b] = swingAxis;
                                break;
                            }
                            l++;
                        }
                    }
                    b++;
                }
            }
            al++;
        }

        //Twist lower arm to hand
        LimitMin[1][0].x = LimitMin[1][1].x;
        LimitMax[1][0].x = LimitMax[1][1].x;
        LimitMin[2][0].x = LimitMin[2][1].x;
        LimitMax[2][0].x = LimitMax[2][1].x;

        LimitMin[1][1].x = 0;
        LimitMax[1][1].x = 0;
        LimitMin[2][1].x = 0;
        LimitMax[2][1].x = 0;

        //Twist lower leg to foot
        LimitMin[3][1].x = LimitMin[3][2].x;
        LimitMax[3][1].x = LimitMax[3][2].x;
        LimitMin[3][2].x = 0;
        LimitMax[3][2].x = 0;

        LimitMin[4][1].x = LimitMin[4][2].x;
        LimitMax[4][1].x = LimitMax[4][2].x;
        LimitMin[4][2].x = 0;
        LimitMax[4][2].x = 0;
    }

    private void AddDefaultColliders()
    {
        BoneCollidersControl = new MTR_BoneCollidersControl[5][];
        BoneCollidersControl[0] = new MTR_BoneCollidersControl[6];
        BoneCollidersControl[1] = new MTR_BoneCollidersControl[4];
        BoneCollidersControl[2] = new MTR_BoneCollidersControl[4];
        BoneCollidersControl[3] = new MTR_BoneCollidersControl[4];
        BoneCollidersControl[4] = new MTR_BoneCollidersControl[4];

        int l = 0;
        while (l < 5)
        {
            int b = 0;
            while (b < Bones[l].Length)
            {
                if (Bones[l][b] != null)
                {
                    Transform ParentT = Bones[l][b].transform;

                    GameObject ColliderGo = new() { layer = Bones[l][b].layer };
                    Transform ColliderT = ColliderGo.transform;
                    ColliderT.name = ParentT.name + " Collider";
                    ColliderT.parent = ParentT;
                    ColliderT.SetLocalPositionAndRotation(Vector3.zero, Quaternion.LookRotation(Axis[l][b], SwingAxis[l][b]));
                    ColliderT.localScale = Vector3.one;
                    MTR_BoneCollidersControl BCC = ColliderGo.AddComponent<MTR_BoneCollidersControl>();
                    BCC.ID = l * 10 + b;
                    BCC.DirectionX = Axis[l][b];
                    BCC.DirectionY = SwingAxis[l][b];
                    BCC.DirectionZ = Vector3.Cross(Axis[l][b], SwingAxis[l][b]);

                    if (b == 0)
                        BCC.Length = 0;
                    else
                    {
                        int g = b - 1;
                        while (g > 0)
                        {
                            if (Bones[l][g] != null)
                                break;
                            g--;
                        }
                        if (Bones[l][g] != null)
                        {
                            BCC.Length = Vector3.Distance(ParentT.position, Bones[l][g].transform.position);
                            BCC.NextBone = Bones[l][g].transform;
                        }
                        else
                            BCC.Length = 0;
                    }

                    CapsuleCollider C = ColliderGo.AddComponent<CapsuleCollider>();
                    C.center = new Vector3(0, 0, BCC.Length / ColliderT.lossyScale.x / 2);

                    if (l == 0)
                    {
                        if (b == 0)
                        {
                            C.radius = 0.08F;
                            C.height = 0.2F;
                            C.direction = 1;
                            C.center = new Vector3(0, 0, C.radius / 2F);
                        }
                        else if (b == Bones[l].Length - 1)
                        {
                            C.radius = 0.1F;
                            C.height = BCC.Length / ColliderT.lossyScale.x;
                            C.direction = 2;
                        }
                        else
                        {
                            if (b == 1 && animator.GetBoneTransform(HumanBodyBones.Neck))
                            {
                                C.radius = 0.05F;
                                C.height = BCC.Length / ColliderT.lossyScale.x + C.radius * 2;
                                C.direction = 2;
                            }
                            else
                            {
                                C.radius = 0.1F;
                                C.height = BCC.Length / ColliderT.lossyScale.x;
                                C.direction = 2;
                            }
                        }

                    }
                    else if (l == 1 || l == 2)
                    {
                        if (b == 0)
                        {
                            C.radius = 0.05F;
                            C.height = BCC.Length / ColliderT.lossyScale.x + C.radius * 2;
                            C.direction = 2;
                            C.center = new Vector3(0, 0, C.radius);
                        }
                        else
                        {
                            C.radius = 0.05F;
                            C.height = BCC.Length / ColliderT.lossyScale.x + C.radius * 2;
                            C.direction = 2;
                        }
                    }
                    else
                    {
                        if (Bones[l][b].transform == animator.GetBoneTransform(HumanBodyBones.LeftFoot) || Bones[l][b].transform == animator.GetBoneTransform(HumanBodyBones.RightFoot))
                        {
                            C.radius = 0.05F;
                            C.height = BCC.Length / ColliderT.lossyScale.x / 2 + C.radius * 2;
                            C.direction = 1;
                            C.center = new Vector3(0, BCC.Length / ColliderT.lossyScale.x / 2, C.radius);
                        }
                        else
                        {
                            C.radius = 0.05F;
                            C.height = BCC.Length / ColliderT.lossyScale.x + C.radius * 2;
                            C.direction = 2;
                        }
                    }

                    BCC.colliders = new Collider[1];
                    BCC.colliders[0] = C;

                    BoneCollidersControl[l][b] = BCC;
                }
                b++;
            }
            l++;
        }
        GenerateCollidersOnStart = false;
    }

    private void AddConnections()
    {
        int al = 0;
        while (al < 5)
        {
            int b = 0;

            while (b < Bones[al].Length)
            {
                if (Bones[al][b] != null)
                {
                    GameObject GO = Bones[al][b];
                    GameObject GO3 = new();
                    GO3.transform.eulerAngles = BonesMPose[al][b].transform.localEulerAngles;
                    GO3.transform.Rotate(GO3.transform.TransformDirection(SwingAxis[al][b]), -(LimitMax[al][b].y + LimitMin[al][b].y) / 2F, Space.World);
                    GO3.transform.Rotate(GO3.transform.TransformDirection(Vector3.Cross(Axis[al][b], SwingAxis[al][b]).normalized), -(LimitMax[al][b].z + LimitMin[al][b].z) / 2F, Space.World);

                    MTR_BoneControl MTRBC = GO.AddComponent<MTR_BoneControl>();

                    if (al == 0 && b == Bones[al].Length - 1)
                    {
                        MTRBC.isHips = true;
                        HipsMTRBC = MTRBC;
                    }
                    else if (al == 1 && b == 2)
                        LArmMTRBC = MTRBC;
                    else if (al == 2 && b == 2)
                        RArmMTRBC = MTRBC;

                    MTRBC.Root = transform;
                    MTRBC.Parent = GO.transform.parent;
                    MTRBC.LocalPosition = GO.transform.localPosition;
                    MTRBC.ChJLocalRotation = GO3.transform.localEulerAngles;
                    if (!Application.isPlaying)
                        DestroyImmediate(GO3);
                    else
                        Destroy(GO3);

                    MTRBC.axis = Axis[al][b];
                    MTRBC.swingAxis = SwingAxis[al][b];

                    MTRBC.LimitMin = LimitMin[al][b];
                    MTRBC.LimitMax = LimitMax[al][b];

                    MTRBC.ChJSpring = ChJSpring;
                    MTRBC.ChJDamper = ChJDamper;
                    MTRBC.ChJPreprocessing = ChJPreprocessing;

                    if (al != 0 || b != Bones[al].Length - 1)
                    {
                        int g = b + 1;
                        if (g < Bones[al].Length)
                        {
                            while (g < Bones[al].Length - 1)
                            {
                                if (Bones[al][g] != null)
                                    break;
                                g++;
                            }
                            if (Bones[al][g] != null)
                                MTRBC.ChJbodyToConnect = Bones[al][g];
                            else
                            {
                                g = 2;
                                while (g < Bones[0].Length)
                                {
                                    if (Bones[0][g] != null)
                                        break;
                                    g++;
                                }
                                MTRBC.ChJbodyToConnect = Bones[0][g];
                            }
                        }
                        else
                        {
                            if (al == 1 || al == 2)
                            {
                                g = 2;
                                while (g < Bones[0].Length)
                                {
                                    if (Bones[0][g] != null)
                                        break;
                                    g++;
                                }
                                MTRBC.ChJbodyToConnect = Bones[0][g];
                            }
                            else
                                MTRBC.ChJbodyToConnect = Bones[0][5];
                        }
                    }
                }
                b++;
            }
            al++;
        }
    }

    private void FreeRigidbodies()
    {
        MTR_Control MTR_C = gameObject.AddComponent<MTR_Control>();
        MTR_C.Animate = Animate;
        MTR_C.Mass = Mass;
        MTR_C.ResurrectionTime = ResurrectingTime;
        MTR_C.AddVelocity = AddVelocity;
        MTR_C.Drag = Drag;
        MTR_C.AngularDrag = AngularDrag;
        MTR_C.RBI = RBI;
        MTR_C.RBCDM = RBCDM;


        MTR_C.HipsBC = HipsMTRBC;
        MTR_C.HipsTR = HipsMTRBC.transform;
        MTR_C.LArmTR = LArmMTRBC.transform;
        MTR_C.RArmTR = RArmMTRBC.transform;

        MTR_C.AnimatorResurrectTriggerName = AnimatorResurrectTriggerName;
        MTR_C.AnimatorOnBackBoolName = AnimatorOnBackBoolName;

        Rigidbody RB = gameObject.GetComponent<Rigidbody>();

        if (JustRagdoll)
        {
            if (Application.isPlaying)
            {
                DestroyComponents(gameObject);
                if (RB)
                    Destroy(RB);
            }
            transform.name += " Ragdoll";
        }
        else
        {
            Collider[] C = gameObject.GetComponents<Collider>();
            Collider[] B = gameObject.GetComponentsInChildren<Collider>();
            if (C != null && C.Length != 0)
            {
                foreach (Collider c in C)
                {
                    if (c.material == null)
                        c.material = CollidersMaterial;

                    foreach (Collider b in B)
                        if (b.gameObject != gameObject)
                            Physics.IgnoreCollision(b, c, true);

                    c.enabled = Animate;
                }
            }
            if (RB)
            {
                if (Animate)
                    RB.isKinematic = false;
                else
                    RB.isKinematic = true;
            }
            if (!animator.runtimeAnimatorController)
                Debug.LogWarning(transform.name + " in \"Animator\" does not have \"Controller\". \"MecanimToRagdoll\" may not work properly.");
        }
        if (Application.isPlaying)
        {
            DestroyImmediate(gameObject.GetComponent<MecanimToRagdoll>());
            if (JustRagdoll)
                DestroyImmediate(animator);
        }
    }

    private void Warnings()
    {
        if (!TPose)
            Debug.LogWarning(transform.name + " in \"MecanimToRagdoll\" does not have \"T Pose\" prefab.");
        if (!MusclesPose)
            Debug.LogWarning(transform.name + " in \"MecanimToRagdoll\" does not have \"Muscles Pose\" prefab.");
        if (!animator)
            Debug.LogWarning(transform.name + " in \"MecanimToRagdoll\" does not have \"Animator\".");
        Debug.LogAssertion("In " + transform.name + " \"MecanimToRagdoll\" can not work properly. Self-destruct of \"MecanimToRagdoll\" initiated. Kaboom! :)");
        Destroy(gameObject.GetComponent<MecanimToRagdoll>());
    }
}