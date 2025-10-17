using UnityEngine;
using UnityEngine.InputSystem;

public class MarionetteControl : MonoBehaviour
{
    float xPos;
    float yPos;

    float xRot;
    float yRot;

    float xForce;
    float yForce;
    
    public float xForceMultiplier;
    public float yForceMultiplier;

    public float yMaxRotationMultiplier;
    public float xMaxRotationMultiplier;

    public float rotationSpeed;

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
        yForce = movementInput.y * xForceMultiplier;
        xForce = -1 * movementInput.x * yForceMultiplier;

        yPos += yForce * Time.deltaTime;
        xPos += xForce * Time.deltaTime;

        float targetYRot = yForce * yMaxRotationMultiplier;
        float targetXRot = xForce * xMaxRotationMultiplier;

        Vector3 targetRotation = new Vector3(-15 - targetYRot, 0, -1 * targetXRot);

        centerJoint.transform.localPosition = new Vector3(xPos, yPos, 0);
        centerJoint.transform.localRotation = Quaternion.RotateTowards(centerJoint.transform.localRotation, Quaternion.Euler(targetRotation), rotationSpeed *  Time.deltaTime);


    }
}
