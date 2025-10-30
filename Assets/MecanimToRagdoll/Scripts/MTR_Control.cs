using UnityEngine;

/// <summary>
/// Control of MTR.
/// </summary>
public class MTR_Control : MonoBehaviour
{
    [Header("Settings"), Tooltip("If \"true\" Animate model. If \"false\" - ragdoll.")]
    /// <summary>
    /// If "true" Animate model. If "false" - ragdoll.
    /// </summary>
    public bool Animate;
    private bool AnimateTest;

    [Tooltip("The sum of the mass of all bones.")]
    /// <summary>
    /// The sum of the mass of all bones.
    /// </summary>
    public float Mass { get; set; }

    [HideInInspector, Tooltip("This tells if the character is \"resurrected\" when animated after the Ragdoll state.")]
    /// <summary>
    /// This tells if the character is "resurrected" when animated after the Ragdoll state.
    /// </summary>
    public bool Resurrected;

    public float ResurrectionTime = 10;
    private float ResurrectionTimer;
    private Vector3 ResurrectionPosition;

    [HideInInspector, Header("Rigidbodies Settings"), Tooltip("Add character velocity to character bones when it becomes a ragdoll.")]
    /// <summary>
    /// Add character velocity to character bones when it becomes a ragdoll.
    /// </summary>
    public bool AddVelocity = true;

    [HideInInspector, Tooltip("The drag of Rigidbody for all bones.")]
    /// <summary>
    /// The drag of Rigidbody for all bones.
    /// </summary>
    public float Drag;

    [HideInInspector, Tooltip("The angular drag of Rigidbody for all bones.")]
    /// <summary>
    /// The angular drag of Rigidbody for all bones.
    /// </summary>
    public float AngularDrag;

    [HideInInspector, Tooltip("Rigidbodies interpolation for all bones.")]
    /// <summary>
    /// Rigidbodies interpolation.
    /// </summary>
    public RigidbodyInterpolation RBI;

    [HideInInspector, Tooltip("Rigidbodies collision detection mode for all bones.")]
    /// <summary>
    /// Rigidbodies collision detection mode.
    /// </summary>
    public CollisionDetectionMode RBCDM;

    [HideInInspector, Tooltip("The name of the trigger parameter in the animator controller that indicates that the ragdoll is \"resurrecting\".")]
    /// <summary>
    /// The name of the trigger parameter in the animator controller that indicates that the ragdoll is "resurrecting".
    /// </summary>
    public string AnimatorResurrectTriggerName = "Resurrect";

    [HideInInspector, Tooltip("The name of the bool parameter in the animator controller that indicates that the ragdoll is lying on its back.")]
    /// <summary>
    /// The name of the bool parameter in the animator controller that indicates that the ragdoll is lying on its back.
    /// </summary>
    public string AnimatorOnBackBoolName = "OnBack";

    private Animator A;
    private Rigidbody RB;
    private Collider[] GOC;
    [HideInInspector]
    public MTR_BoneControl HipsBC;
    private MTR_BoneControl[] BonesControl;

    [HideInInspector]
    public Transform HipsTR;
    [HideInInspector]
    public Transform LArmTR;
    [HideInInspector]
    public Transform RArmTR;

    private void Start()
    {
        Resurrected = Animate;
        BonesControl = gameObject.GetComponentsInChildren<MTR_BoneControl>();

        foreach (MTR_BoneControl MTRBC in BonesControl)
            MTRBC.SetSettings();

        GOC = gameObject.GetComponents<Collider>();
        Collider[] Bc = gameObject.GetComponentsInChildren<Collider>();
        foreach (Collider C in GOC)
            foreach (Collider c in Bc)
                Physics.IgnoreCollision(C, c, true);

        Rigidbody[] Rb = gameObject.GetComponentsInChildren<Rigidbody>();
        float MassSum = 0;
        foreach (Rigidbody rb in Rb)
        {
            if (rb.gameObject != gameObject)
            {
                rb.mass = 1;
                foreach (Collider c in rb.GetComponentsInChildren<Collider>())
                    rb.mass += SetMass(c.gameObject);
                rb.mass -= 1;
                MassSum += rb.mass;
            }
        }

        MassSum = Mass / MassSum;
        Mass = 0;
        foreach (Rigidbody rb in Rb)
        {
            if (rb.gameObject != gameObject)
            {
                rb.mass *= MassSum;
                Mass += rb.mass;
            }
            if (rb.transform != transform)
            {
                rb.linearDamping = Drag;
                rb.angularDamping = AngularDrag;
                rb.interpolation = RBI;
                rb.collisionDetectionMode = RBCDM;
            }
        }

        if (gameObject.TryGetComponent(out A))
        {
            A.enabled = Animate;
            AnimateTest = Animate;
            bool ARTNExist = false;
            bool AOBBNExist = false;

            if (AnimatorResurrectTriggerName != "" || AnimatorOnBackBoolName != "")
            {
                for (int i = 0; i < A.parameters.Length; i++)
                {

                    if (A.parameters[i].type == AnimatorControllerParameterType.Trigger && AnimatorResurrectTriggerName != "" && A.parameters[i].name == AnimatorResurrectTriggerName)
                        ARTNExist = true;
                    if (A.parameters[i].type == AnimatorControllerParameterType.Bool && AnimatorOnBackBoolName != "" && A.parameters[i].name == AnimatorOnBackBoolName)
                        AOBBNExist = true;
                    if (ARTNExist && AOBBNExist)
                        break;
                }

                if (AnimatorResurrectTriggerName != "" && !ARTNExist)
                {
                    Debug.LogError("There is no trigger type parameter named \"" + AnimatorResurrectTriggerName + "\" in the animation controller. Check the animation controller settings or \"MecanimToRagdoll\" (Trigger Parameter \"Resurrect\").");
                    AnimatorResurrectTriggerName = "";
                }
                if (AnimatorOnBackBoolName != "" && !AOBBNExist)
                {
                    Debug.LogError("There is no bool type parameter named \"" + AnimatorOnBackBoolName + "\" in the animation controller. Check the animation controller settings or \"MecanimToRagdoll\" (Bool Parameter \"On Back\").");
                    AnimatorOnBackBoolName = "";
                }
            }

            if (Animate)
                СonnectBones(true);
        }
        else
            Destroy(gameObject.GetComponent<MTR_Control>());
    }

    private float SetMass(GameObject GO)
    {
        float Mass = 0;
        foreach (CapsuleCollider CC in GO.GetComponents<CapsuleCollider>())
        {
            CC.height = Mathf.Abs(CC.height);
            CC.radius = Mathf.Abs(CC.radius);
            Mass += 4 / 3F * Mathf.PI * Mathf.Pow(CC.radius * GO.transform.lossyScale.x, 3);
            if (CC.height - 2 * CC.radius > 0)
                Mass += Mathf.PI * Mathf.Pow(CC.radius * GO.transform.lossyScale.x, 2) * ((CC.height - 2 * CC.radius) * GO.transform.lossyScale.x);
        }
        foreach (SphereCollider SC in GO.GetComponents<SphereCollider>())
        {
            SC.radius = Mathf.Abs(SC.radius);
            Mass += 4 / 3F * Mathf.PI * Mathf.Pow(SC.radius * GO.transform.lossyScale.x, 3);
        }
        foreach (BoxCollider BC in GO.GetComponents<BoxCollider>())
        {
            BC.size = new Vector3(Mathf.Abs(BC.size.x), Mathf.Abs(BC.size.y), Mathf.Abs(BC.size.z));
            Mass += BC.size.x * BC.size.y * BC.size.z * Mathf.Pow(GO.transform.lossyScale.x, 3);
        }
        return Mass;
    }

    //private void Update()
    //{
    //    if (Animate != A.enabled)
    //        Do();
    //}
    private void LateUpdate()
    {
        if (Animate != AnimateTest)//A.enabled)
            Do();
        if (Animate && !Resurrected)
            ResurectIt();
    }

    private void Do()
    {
        if (RB == null)
            RB = gameObject.GetComponent<Rigidbody>();

        if (Animate)
        {
            СonnectBones(false);
            Debug.DrawLine(ResurrectionPosition, transform.position, Color.red, 10);

            HipsBC.CorrectHipsPosition(ResurrectionPosition - transform.position);

            Vector3 l = LArmTR.position - HipsTR.position;
            Vector3 r = RArmTR.position - HipsTR.position;
            Vector3 dir = (l + r).normalized;
            dir = Vector3.ProjectOnPlane(dir, Physics.gravity);
            Quaternion hipsR = HipsTR.rotation;
            bool onBack = Vector3.Angle(Vector3.Cross(l, r), Physics.gravity) < 90;
            transform.rotation = Quaternion.LookRotation(onBack ? -dir : dir, -Physics.gravity);
            HipsTR.rotation = hipsR;

            HipsBC.CorrectHipsRotation();

            A.enabled = true;

            if (ResurrectionTime > 0)
                AnimationSnapshot(onBack);

            transform.position = ResurrectionPosition;
        }
        else
            RagdollIt();
        AnimateTest = Animate;
    }

    private void RagdollIt()
    {
        A.enabled = false;
        Resurrected = false;
        Vector3 velocity = Vector3.zero;
        if (AddVelocity && RB != null)
            velocity = RB.linearVelocity;
        foreach (MTR_BoneControl MTRBC in BonesControl)
            MTRBC.PhysicsBone(velocity);
        if (RB != null)
            RB.isKinematic = true;
        if (GOC != null)
            foreach (Collider c in GOC)
                c.enabled = false;
    }

    private void СonnectBones(bool firstTime)
    {
        foreach (MTR_BoneControl MTRBC in BonesControl)
        {
            if (MTRBC.isHips && !firstTime)
            {
                bool hitedNotBody = false;
                Vector3 hitFrom = MTRBC.transform.position;
                float d = 1;
                int i = 1;
                while (!hitedNotBody && d > 0)
                {
                    if (Physics.Raycast(hitFrom, Vector3.down, out RaycastHit hit, d, ~0, QueryTriggerInteraction.Ignore))
                    {
                        Debug.DrawLine(hitFrom, hit.point, Color.white / i, 10);
                        MTR_Control[] test = hit.transform.gameObject.GetComponentsInParent<MTR_Control>();
                        if (test != null && test.Length > 0)
                        {
                            d -= (hit.point - hitFrom).magnitude;
                            hitFrom = hit.point;
                        }
                        else
                        {
                            hitedNotBody = true;
                            ResurrectionPosition = hit.point;
                        }
                        i++;
                    }
                    else
                        break;
                }
                if (!hitedNotBody)
                    ResurrectionPosition = MTRBC.transform.position;
            }
            MTRBC.СonnectBone();
        }
        if (GOC != null && GOC.Length > 0)
            foreach (Collider c in GOC)
                c.enabled = true;
        if (RB != null)
            RB.isKinematic = false;
    }

    private void AnimationSnapshot(bool onBack)
    {
        if (AnimatorResurrectTriggerName != "")
            A.SetTrigger(AnimatorResurrectTriggerName);

        if (AnimatorOnBackBoolName != "")
            A.SetBool(AnimatorOnBackBoolName, onBack);

        A.Update(0);

        A.speed = 0;
        Resurrected = false;
        ResurrectionTimer = 0;

        foreach (MTR_BoneControl MTRBC in BonesControl)
            MTRBC.AnimationBoneSnapshot();
    }

    private void ResurectIt()
    {
        if (ResurrectionTimer < ResurrectionTime)
        {
            ResurrectionTimer += Time.deltaTime;
            float normal = Mathf.Clamp01(ResurrectionTimer / ResurrectionTime);
            foreach (MTR_BoneControl MTRBC in BonesControl)
                MTRBC.ResurrectBone(normal);
        }
        else
        {
            Resurrected = true;
            A.speed = 1;
        }
    }
}