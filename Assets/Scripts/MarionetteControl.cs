using UnityEngine;
using UnityEngine.InputSystem;

public class MarionetteControl : MonoBehaviour
{
    public float xPos;
    public float yPos;

    public float xRot;
    public float yRot;

    public float xForce;
    public float yForce;

    GameObject centerJoint;
    GameObject leftArmJoint; 
    GameObject rightArmJoint;
    GameObject leftLegJoint;
    GameObject rightLegJoint;

    Vector2 movementInput;

    void Awake()
    {

    }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        centerJoint = GameObject.Find("crossbar/Joints/center");
        leftArmJoint = GameObject.Find("crossbar/Joints/arm_L");
        rightArmJoint = GameObject.Find("crossbar/Joints/arm_R");
        leftLegJoint = GameObject.Find("crossbar/Joints/leg_L");
        rightLegJoint = GameObject.Find("crossbar/Joints/leg_R");
        if (centerJoint != null)
        {
            centerJoint.transform.localRotation = Quaternion.Euler(-60, 0, 0);
        }
        else
        {
            Debug.LogError("Center joint not found!");
        }
    }

    void OnMove(InputValue value)
    {
        movementInput = value.Get<Vector2>();
    }

    // Update is called once per frame
    void Update()
    {
        yForce = movementInput.y * 20f;
        xForce = movementInput.x * 20f;

        yPos += yForce * Time.deltaTime;
        xPos += xForce * Time.deltaTime;   

        if (yForce > yRot)
        {
            yRot += 5f;
        }
        else
        {
            yRot -= 5f;
        }

        centerJoint.transform.localPosition = new Vector3(xPos, yPos, 0);
        centerJoint.transform.localRotation = Quaternion.Euler(-60 + yForce, xForce, 0);

    }
}
