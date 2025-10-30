using UnityEngine;
using System.Collections;

public class DragRigidbody : MonoBehaviour
{
    public float spring = 50.0f;
    public float damper = 5.0f;
    public float drag = 10.0f;
    public float angularDrag = 5.0f;
    public float distance = 0.2f;
    public bool attachToCenterOfMass = false;
    public RaycastHit hit;

    private SpringJoint springJoint;
    private float distanceRoll = 1;
    void Update()
    {
        if (!Input.GetMouseButtonDown(0))
            return;
        else
            distanceRoll = 1;

        if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
            return;

        if (!hit.rigidbody || hit.rigidbody.isKinematic)
            return;

        if (!springJoint)
        {
            GameObject go = new GameObject("Rigidbody dragger");
            Rigidbody body = go.AddComponent<Rigidbody>();
            springJoint = go.AddComponent<SpringJoint>();
            body.isKinematic = true;
        }

        springJoint.transform.position = hit.point;
        if (attachToCenterOfMass)
        {
            Vector3 anchor = transform.TransformDirection(hit.rigidbody.centerOfMass) + hit.rigidbody.transform.position;
            anchor = springJoint.transform.InverseTransformPoint(anchor);
            springJoint.anchor = anchor;
        }
        else
        {
            springJoint.anchor = Vector3.zero;
        }

        springJoint.spring = spring;
        springJoint.damper = damper;
        springJoint.maxDistance = distance;
        springJoint.connectedBody = hit.rigidbody;

        StartCoroutine(DragObject(hit.distance));
    }

    IEnumerator DragObject(float distance)
    {
        float oldDrag = springJoint.connectedBody.linearDamping;
        float oldAngularDrag = springJoint.connectedBody.angularDamping;
        springJoint.connectedBody.linearDamping = drag;
        springJoint.connectedBody.angularDamping = angularDrag;
        while (Input.GetMouseButton(0))
        {
            distanceRoll += Input.GetAxis("Mouse ScrollWheel");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            springJoint.transform.position = ray.GetPoint(distance * distanceRoll);
            yield return null;
        }
        if (springJoint.connectedBody)
        {
            springJoint.connectedBody.linearDamping = oldDrag;
            springJoint.connectedBody.angularDamping = oldAngularDrag;
            springJoint.connectedBody = null;
        }
    }
}