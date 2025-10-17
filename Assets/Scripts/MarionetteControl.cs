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

    public Vector3 startingPosition;

    public float headStringLength;
    public float armsStringLength;
    public float legsStringLength;
    public float tensionMultiplier;

    GameObject crossbarCenterJoint;
    GameObject crossbarLeftArmJoint; 
    GameObject crossbarRightArmJoint;
    GameObject crossbarLeftLegJoint;
    GameObject crossbarRightLegJoint;

    GameObject modelCenterJoint;
    GameObject modelLeftArmJoint;
    GameObject modelRightArmJoint;
    GameObject modelLeftLegJoint;
    GameObject modelRightLegJoint;

    Rigidbody centerRigidbody;
    Rigidbody leftArmRigidbody;
    Rigidbody rightArmRigidbody;
    Rigidbody leftLegRigidbody;
    Rigidbody rightLegRigidbody;

    Vector2 movementInput;



    void Awake()
    {

    }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        crossbarCenterJoint = GameObject.Find("crossbar/Joints/center");
        crossbarLeftArmJoint = GameObject.Find("crossbar/Joints/center/arm_L");
        crossbarRightArmJoint = GameObject.Find("crossbar/Joints/center/arm_R");
        crossbarLeftLegJoint = GameObject.Find("crossbar/Joints/center/leg_L");
        crossbarRightLegJoint = GameObject.Find("crossbar/Joints/center/leg_R");

        xPos = startingPosition.x;
        yPos = startingPosition.y;


        //get all joints from the model where the "strings" will be attached
        //assuming a specific hierarchy here - will need to be adjusted based on model naming conventions
        modelCenterJoint = GameObject.Find("model/DEFORM_RIG/root/DEF_hipJA_1/DEF_spineJA_1/DEF_spineJB_1/DEF_spineJC_1/DEF_chestJA_1/DEF_neckJA_1");
        modelLeftArmJoint = GameObject.Find("model/DEFORM_RIG/root/DEF_hipJA_1/DEF_spineJA_1/DEF_spineJB_1/DEF_spineJC_1/DEF_chestJA_1/DEF_armJALt_1/DEF_shoulderJALt_1/DEF_elbowJALt_1/DEF_wristJALt_1");
        modelRightArmJoint = GameObject.Find("model/DEFORM_RIG/root/DEF_hipJA_1/DEF_spineJA_1/DEF_spineJB_1/DEF_spineJC_1/DEF_chestJA_1/DEF_armJARt_1/DEF_shoulderJARt_1/DEF_elbowJARt_1/DEF_wristJARt_1");
        modelLeftLegJoint = GameObject.Find("model/DEFORM_RIG/root/DEF_hipJA_1/DEF_legJARt_1/DEF_kneeJARt_1");
        modelRightLegJoint = GameObject.Find("model/DEFORM_RIG/root/DEF_hipJA_1/DEF_legJALt_1/DEF_kneeJALt_1");

        //get rigidbodies for each joint: forces will be applied to these
        centerRigidbody = modelCenterJoint.GetComponent<Rigidbody>();
        leftArmRigidbody = modelLeftArmJoint.GetComponent<Rigidbody>();
        rightArmRigidbody = modelRightArmJoint.GetComponent<Rigidbody>();
        leftLegRigidbody = modelLeftLegJoint.GetComponent<Rigidbody>();
        rightLegRigidbody = modelRightLegJoint.GetComponent<Rigidbody>();


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

        crossbarCenterJoint.transform.localPosition = new Vector3(xPos, yPos, 0);
        crossbarCenterJoint.transform.localRotation = Quaternion.RotateTowards(crossbarCenterJoint.transform.localRotation, Quaternion.Euler(targetRotation), rotationSpeed *  Time.deltaTime);

        //apply tension forces to each joint
        centerRigidbody.AddForce(CalculateTension(crossbarCenterJoint.transform.position, modelCenterJoint.transform.position, headStringLength, tensionMultiplier * 2));
        leftArmRigidbody.AddForce(CalculateTension(crossbarLeftArmJoint.transform.position, modelLeftArmJoint.transform.position, armsStringLength, tensionMultiplier));
        rightArmRigidbody.AddForce(CalculateTension(crossbarRightArmJoint.transform.position, modelRightArmJoint.transform.position, armsStringLength, tensionMultiplier));
        leftLegRigidbody.AddForce(CalculateTension(crossbarLeftLegJoint.transform.position, modelLeftLegJoint.transform.position, legsStringLength, tensionMultiplier));
        rightLegRigidbody.AddForce(CalculateTension(crossbarRightLegJoint.transform.position, modelRightLegJoint.transform.position, legsStringLength, tensionMultiplier));


    }


    Vector3 CalculateTension(Vector3 crossbarJointPosition, Vector3 modelJointPosition, float baseLength, float tensionMultiplier)
    {
        Vector3 modelToCrossbar = new Vector3();
        modelToCrossbar = crossbarJointPosition - modelJointPosition;

        float currentLength = modelToCrossbar.magnitude;
        float stretchLength = currentLength - baseLength;
        stretchLength = Mathf.Clamp(stretchLength, 0, 100);

        return modelToCrossbar.normalized * stretchLength * tensionMultiplier;

    }

}
