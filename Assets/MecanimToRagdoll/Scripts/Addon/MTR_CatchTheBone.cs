using System.Collections.Generic;
using UnityEngine;

public enum CatchType { MakeKinematik, MakeKinematikAndAttach, SpringJoint };

public class MTR_CatchTheBone : MonoBehaviour
{
    public List<int> ID;
    public CatchType CT;

    private List<Transform> Catched;
    private List<Transform> CatchedT;

    private List<SpringJoint> CatchedSJ;

    public float Spring = 1000;
    public float Damper;

    public float MinDistance = 0;
    public float MaxDistance = 1;

    void Start()
    {
        if (ID.Count == 0)
            Destroy(this);
    }

    void Update()
    {
        if (CT == CatchType.MakeKinematikAndAttach && Catched != null && Catched.Count != 0)
        {
            int c = Catched.Count - 1;
            while (c >= 0)
            {
                MTR_Control MTR_C = Catched[c].root.GetComponent<MTR_Control>();
                if (MTR_C != null && MTR_C.Animate)
                {
                    Catched.RemoveAt(c);
                    Destroy(CatchedT[c].gameObject);
                    CatchedT.RemoveAt(c);
                }
                else
                {
                    Catched[c].position = CatchedT[c].position;
                    Catched[c].rotation = CatchedT[c].rotation;
                }
                c--;
            }
        }
        else if (CT == CatchType.SpringJoint && Catched != null && Catched.Count != 0)
        {
            int c = Catched.Count - 1;
            while (c >= 0)
            {
                MTR_Control MTR_C = Catched[c].root.GetComponent<MTR_Control>();
                if (MTR_C != null && MTR_C.Animate)
                {
                    Catched.RemoveAt(c);
                    Destroy(CatchedSJ[c]);
                    CatchedSJ.RemoveAt(c);
                }
                c--;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        MTR_BoneCollidersControl BCC = collision.gameObject.GetComponent<MTR_BoneCollidersControl>();
        Catch(BCC);
    }

    private void OnTriggerEnter(Collider other)
    {
        MTR_BoneCollidersControl BCC = other.GetComponent<MTR_BoneCollidersControl>();
        Catch(BCC);
    }

    private void Catch(MTR_BoneCollidersControl BCC)
    {
        if (BCC && ID.Contains(BCC.ID))
        {
            Rigidbody RB = BCC.transform.parent.GetComponent<Rigidbody>();
            switch (CT)
            {
                case CatchType.MakeKinematik:
                    if (RB)
                        RB.isKinematic = true;
                    break;

                case CatchType.MakeKinematikAndAttach:
                    if (RB)
                        RB.isKinematic = true;
                    Catched ??= new List<Transform>();
                    if (!Catched.Contains(BCC.transform.parent))
                        Catched.Add(BCC.transform.parent);
                    CatchedT ??= new List<Transform>();
                    Transform T = new GameObject("Character " + BCC.transform.root.name + " Bone " + BCC.ID).transform;
                    T.position = BCC.transform.parent.position;
                    T.rotation = BCC.transform.parent.rotation;
                    T.parent = transform;
                    CatchedT.Add(T);
                    break;

                case CatchType.SpringJoint:
                    if (Catched == null || !Catched.Contains(BCC.transform.parent))
                    {
                        if (!gameObject.TryGetComponent<Rigidbody>(out _))
                        {
                            Rigidbody RBt = gameObject.AddComponent<Rigidbody>();
                            RBt.isKinematic = true;
                        }
                        Catched ??= new List<Transform>();
                        Catched.Add(BCC.transform.parent);

                        CatchedSJ ??= new List<SpringJoint>();
                        SpringJoint SJ = gameObject.AddComponent<SpringJoint>();
                        SJ.anchor = transform.InverseTransformPoint(RB.position);
                        SJ.autoConfigureConnectedAnchor = false;
                        SJ.connectedAnchor = RB.centerOfMass;
                        SJ.connectedBody = RB;
                        SJ.spring = Spring;
                        SJ.damper = Damper;
                        SJ.minDistance = MinDistance;
                        SJ.maxDistance = MaxDistance;
                        CatchedSJ.Add(SJ);
                    }
                    break;
            }
        }
    }
}