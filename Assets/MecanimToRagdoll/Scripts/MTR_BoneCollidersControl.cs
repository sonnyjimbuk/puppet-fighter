using UnityEngine;

public class MTR_BoneCollidersControl : MonoBehaviour
{
    /// <summary>
    /// ID of bone.
    /// </summary>
    public int ID;

    /// <summary>
    /// The direction of the x-axis of the bone from Mecanim.
    /// </summary>
    public Vector3 DirectionX;

    /// <summary>
    /// The direction of the y-axis of the bone from Mecanim.
    /// </summary>
    public Vector3 DirectionY;

    /// <summary>
    /// The direction of the z-axis of the bone from Mecanim.
    /// </summary>
    public Vector3 DirectionZ;

    /// <summary>
    /// Position relative to the bone. 
    /// </summary>
    public Vector3 Position;

    /// <summary>
    /// The distance between this bone and the next one.
    /// </summary>
    public float Length;

    /// <summary>
    /// Next bone. 
    /// </summary>
    public Transform NextBone;

    /// <summary>
    /// Holder looks to next bone. 
    /// </summary>
    public bool LookAtNextBone;

    /// <summary>
    /// List of colliders for this bone.
    /// </summary>
    public Collider[] colliders;

    private void Start()
    {
        if (colliders == null)
        {
            Debug.LogWarning("\"" + transform.name + "\"" + " of " + "\"" + transform.root.name + "\"" + " has no colliders. Object destroyed as unnecessary.");
            Destroy(gameObject);
        }
        else if (colliders.Length == 0)
        {
            Debug.LogWarning("\"" + transform.name + "\"" + " of " + transform.root.name + "\"" + " has no colliders. Object destroyed as unnecessary.");
            Destroy(gameObject);
        }

        foreach (Collider Coll in colliders)
        {
            if (Coll.GetType() == typeof(BoxCollider))
            {
                BoxCollider BoxColl = Coll as BoxCollider;
                Collider[] InRange = Physics.OverlapBox(transform.position + BoxColl.center, BoxColl.size / 2F, transform.rotation);
                foreach (Collider Col1 in InRange)
                    foreach (Collider Col2 in transform.root.gameObject.GetComponentsInChildren<Collider>())
                        if (Col1 == Col2)
                            Physics.IgnoreCollision(BoxColl, Col1, true);
            }
            else if (Coll.GetType() == typeof(CapsuleCollider))
            {
                CapsuleCollider CapsColl = Coll as CapsuleCollider;
                Vector3 H;
                if (CapsColl.direction == 0)
                    H = Vector3.right * CapsColl.height / 2F;
                else if (CapsColl.direction == 1)
                    H = Vector3.up * CapsColl.height / 2F;
                else
                    H = Vector3.forward * CapsColl.height / 2F;
                Vector3 P1 = CapsColl.center + H;
                Vector3 P2 = CapsColl.center - H;
                Collider[] InRange = Physics.OverlapCapsule(transform.TransformPoint(P1), transform.TransformPoint(P2), CapsColl.radius);
                foreach (Collider Col1 in InRange)
                    foreach (Collider Col2 in transform.root.gameObject.GetComponentsInChildren<Collider>())
                        if (Col1 == Col2)
                            Physics.IgnoreCollision(CapsColl, Col1, true);
            }
            else if (Coll.GetType() == typeof(SphereCollider))
            {
                SphereCollider SphColl = Coll as SphereCollider;
                Collider[] InRange = Physics.OverlapSphere(transform.TransformPoint(SphColl.center), SphColl.radius);
                foreach (Collider Col1 in InRange)
                    foreach (Collider Col2 in transform.root.gameObject.GetComponentsInChildren<Collider>())
                        if (Col1 == Col2)
                            Physics.IgnoreCollision(SphColl, Col1, true);
            }
        }
    }

    /// <summary>
    /// The position of the starting position of the object that carries the colliders of the bones in relation to the new axes.
    /// </summary>
    public void SetPosition()
    {
        transform.localPosition = DirectionX * Position.x + DirectionY * Position.y + DirectionZ * Position.z;
        if (LookAtNextBone)
            transform.rotation = Quaternion.LookRotation(NextBone.position - transform.position, transform.parent.TransformDirection(DirectionY));
        else
            transform.localRotation = Quaternion.LookRotation(DirectionX, DirectionY);
    }
}