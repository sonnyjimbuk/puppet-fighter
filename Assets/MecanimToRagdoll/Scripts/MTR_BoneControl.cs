using UnityEngine;

public class MTR_BoneControl : MonoBehaviour
{
    public bool isHips;

    //Physics
    public Transform Root;
    public Rigidbody RB;
    public Vector3 ChJLocalRotation;

    private Rigidbody RBGOTC;

    public Vector3 axis;
    public Vector3 swingAxis;
    public Vector3 LimitMin;
    public Vector3 LimitMax;
    public GameObject ChJbodyToConnect;

    public float ChJSpring;
    public float ChJDamper;
    public bool ChJPreprocessing;

    //Animation
    public Transform Parent;
    public Vector3 LocalPosition;

    //Resurrection
    private Vector3 ResAnimPos;
    private Vector3 ResRagdPos;
    private Quaternion ResAnimRot;
    private Quaternion ResRagdRot;

    public void SetSettings()
    {
        if (RB == null && !gameObject.TryGetComponent(out RB))
            RB = gameObject.AddComponent<Rigidbody>();

        if (ChJbodyToConnect != null)
        {
            if (RBGOTC == null && !ChJbodyToConnect.TryGetComponent(out RBGOTC))
                RBGOTC = ChJbodyToConnect.AddComponent<Rigidbody>();
            if (!gameObject.TryGetComponent(out CharacterJoint _))
            {
                transform.parent = Parent;
                transform.localPosition = LocalPosition;
                Vector3 LR = transform.localEulerAngles;
                transform.localEulerAngles = ChJLocalRotation;

                CharacterJoint ChJ = gameObject.AddComponent<CharacterJoint>();
                ChJ.axis = axis;
                ChJ.swingAxis = -swingAxis;

                SoftJointLimit SJL = new() { bounciness = 0.1F, limit = LimitMin.x };

                ChJ.lowTwistLimit = SJL;

                SJL.limit = LimitMax.x;
                ChJ.highTwistLimit = SJL;

                SJL.limit = (LimitMax.y - LimitMin.y) / 2F;
                ChJ.swing1Limit = SJL;
                SJL.limit = (LimitMax.z - LimitMin.z) / 2F;
                ChJ.swing2Limit = SJL;

                SoftJointLimitSpring SJLS = new() { spring = 0 };

                if (ChJ.lowTwistLimit.limit != 0 && ChJ.lowTwistLimit.limit != 0)
                {
                    SJLS.spring = ChJSpring;
                    SJLS.damper = ChJDamper;
                    ChJ.twistLimitSpring = SJLS;
                }
                SJLS.spring = 0;
                if (ChJ.swing1Limit.limit != 0 || ChJ.swing2Limit.limit != 0)
                {
                    SJLS.spring = ChJSpring;
                    SJLS.damper = ChJDamper;
                    ChJ.swingLimitSpring = SJLS;
                }
                ChJ.connectedBody = RBGOTC;
                ChJ.enablePreprocessing = ChJPreprocessing;
                transform.localEulerAngles = LR;
            }
        }
    }

    public void PhysicsBone(Vector3 velocity)
    {
        if (ChJbodyToConnect != null && RBGOTC.isKinematic)
            RBGOTC.isKinematic = false;
        transform.parent = Root;
        if (RB.isKinematic)
            RB.isKinematic = false;
        RB.linearVelocity += velocity;
    }

    public void СonnectBone()
    {
        if (RB != null)
            RB.isKinematic = true;

        transform.parent = Parent;
        if (isHips)
            ResRagdPos = transform.localPosition;
        transform.localPosition = LocalPosition;
        if (!isHips)
            ResRagdRot = transform.localRotation;
    }

    public void CorrectHipsPosition(Vector3 correctionPos)
    {
        ResRagdPos -= Parent.InverseTransformDirection(correctionPos);
    }

    public void CorrectHipsRotation()
    {
        ResRagdRot = transform.localRotation;
    }

    public void AnimationBoneSnapshot()
    {
        ResAnimRot = transform.localRotation;
        if (isHips)
            ResAnimPos = transform.localPosition;
    }

    public void ResurrectBone(float normal)
    {
        transform.localRotation = Quaternion.Slerp(ResRagdRot, ResAnimRot, normal);
        if (isHips)
            transform.localPosition = Vector3.Lerp(ResRagdPos, ResAnimPos, normal);
    }
}