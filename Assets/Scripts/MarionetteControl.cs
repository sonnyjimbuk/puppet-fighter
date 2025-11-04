using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class MarionetteControl : MonoBehaviour
{
    // name of the puppet, used to pull the correct model from resources
    public string puppetName;
    string tempPuppetName;
    GameObject puppetModel;
    Transform modelTransform;
    private Animator animator;

    float xPos;
    float yPos;

    float xRot;
    float yRot;

    float xForce;
    float yForce;
    
    public float xForceMultiplier;
    public float yForceMultiplier;

    public float maxRotation;

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

    // joycon input section/
    private List<Joycon> joycons;

    // joycon input values
    Joycon j;
    public float[] stick;
    public Vector3 gyro;
    public Vector3 accel;
    public int jc_ind;
    public Quaternion orientation;

    int frameeCounter = 0;

    void Awake()
    {

    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        modelTransform = transform.Find("Model");
        SwapModel();
        tempPuppetName = puppetName;

        crossbarCenterJoint = transform.Find("crossbar/Joints/center").gameObject;
        crossbarLeftArmJoint = transform.Find("crossbar/Joints/center/arm_L").gameObject;
        crossbarRightArmJoint = transform.Find("crossbar/Joints/center/arm_R").gameObject;
        crossbarLeftLegJoint = transform.Find("crossbar/Joints/center/leg_L").gameObject;
        crossbarRightLegJoint = transform.Find("crossbar/Joints/center/leg_R").gameObject;

        
        centerRigidbody = modelCenterJoint.GetComponent<Rigidbody>();
        leftArmRigidbody = modelLeftArmJoint.GetComponent<Rigidbody>();
        rightArmRigidbody = modelRightArmJoint.GetComponent<Rigidbody>();
        leftLegRigidbody = modelLeftLegJoint.GetComponent<Rigidbody>();
        rightLegRigidbody = modelRightLegJoint.GetComponent<Rigidbody>();
        

        xPos = startingPosition.x;
        yPos = startingPosition.y;

        joycons = JoyconManager.Instance.j;
        if (joycons.Count < jc_ind + 1)
        {
            Debug.Log("Not enough Joy-Cons connected for the specified index!");
        }
    }

    void OnMove(InputValue value)
    {
        movementInput = value.Get<Vector2>();
    }

    // Update is called once per frame
    void Update()
    {
        frameeCounter++;

        if (joycons.Count > 0)
        {
            j = joycons[jc_ind];
            stick = j.GetStick();
            gyro = j.GetGyro();
            accel = j.GetAccel();
            orientation = j.GetVector();

            if (frameeCounter % 2 == 0)
            {
                /*Debug.Log(string.Format("Joycon Stick x: {0:N} Stick y: {1:N}", stick[0], stick[1]));
                Debug.Log(string.Format("Joycon Gyro x: {0:N} Gyro y: {1:N} Gyro z: {2:N}", gyro.x, gyro.y, gyro.z));
                Debug.Log(string.Format("Joycon Accel x: {0:N} Accel y: {1:N} Accel z: {2:N}", accel.x, accel.y, accel.z));*/
                Debug.Log(string.Format("Joycon Orientation x: {0:N} Orientation y: {1:N} Orientation z: {2:N} Orientation w: {3:N}", orientation.x, orientation.y, orientation.z, orientation.w));
              
            }
        }

        if (j.GetButtonDown(Joycon.Button.STICK))
        {
            Debug.Log("Shoulder button 2 pressed");
            // GetStick returns a 2-element vector with x/y joystick components
            Debug.Log(string.Format("Stick x: {0:N} Stick y: {1:N}", j.GetStick()[0], j.GetStick()[1]));

            // Joycon has no magnetometer, so it cannot accurately determine its yaw value. Joycon.Recenter allows the user to reset the yaw value.
            j.Recenter();
        }

        if (j.GetButtonDown(Joycon.Button.SL))
        {
            Debug.Log("Shoulder button 1 pressed - Rumble activated");
            // Rumble for 200 milliseconds, with low frequency rumble at 160 Hz and high frequency rumble at 320 Hz. For more information check:
            //)

            j.SetRumble(160, 320, 0.6f, 200);

        } 

        if (puppetName != tempPuppetName)
        {
            SwapModel();
        }
        tempPuppetName = puppetName;


        yForce = 0;
        xForce = 0;

        /*
        yForce = movementInput.y * xForceMultiplier;
        xForce = -1 * movementInput.x * yForceMultiplier;
        */


        yPos += yForce * Time.deltaTime;
        xPos += xForce * Time.deltaTime;

        float w = orientation.w;
        float x = orientation.x;
        float y = orientation.y;
        float z = orientation.z;            

        float roll = Mathf.Atan2(2 * y * w - 2 * x * z, 1 - 2 * y * y - 2 * z * z) * Mathf.Rad2Deg;
        float pitch = Mathf.Atan2(2 * x * w - 2 * y * z, 1 - 2 * x * x - 2 * z * z) * Mathf.Rad2Deg;
        float yaw = Mathf.Asin(2 * x * y + 2 * z * w) * Mathf.Rad2Deg;


        crossbarCenterJoint.transform.localPosition = new Vector3(xPos, yPos, 0);


        //Debug.Log("roll: " + roll + " pitch: " + pitch + " yaw: " + yaw);

        //crossbarCenterJoint.transform.eulerAngles = new Vector3(20, 0, Mathf.Cos(roll * Mathf.Deg2Rad) * (pitch + 90));

        float adjustedRoll = Mathf.Clamp(Mathf.Cos(roll * Mathf.Deg2Rad) * (pitch + 90) + Mathf.Sin(roll * Mathf.Deg2Rad) * (yaw), -maxRotation, maxRotation);

        crossbarCenterJoint.transform.eulerAngles = new Vector3(20, 0, adjustedRoll);

        //crossbarCenterJoint.transform.eulerAngles = new Vector3(20, 0, Mathf.Cos(roll * Mathf.Deg2Rad)  * (pitch) + Mathf.Sin(roll * Mathf.Deg2Rad) * (yaw-90));
        //crossbarCenterJoint.transform.eulerAngles = new Vector3 (20, 0, Mathf.Sin(roll) * (yaw + 90) - Mathf.Cos(roll) * (pitch) );

        //pitch doesn't go smoothly around its maximum? also needs to be inverted

        //apply tension forces to each joint
        centerRigidbody.AddForce(CalculateTension(crossbarCenterJoint.transform.position, modelCenterJoint.transform.position, headStringLength, tensionMultiplier * 2));
        leftArmRigidbody.AddForce(CalculateTension(crossbarLeftArmJoint.transform.position, modelLeftArmJoint.transform.position, armsStringLength, tensionMultiplier));
        rightArmRigidbody.AddForce(CalculateTension(crossbarRightArmJoint.transform.position, modelRightArmJoint.transform.position, armsStringLength, tensionMultiplier));
        leftLegRigidbody.AddForce(CalculateTension(crossbarLeftLegJoint.transform.position, modelLeftLegJoint.transform.position, legsStringLength, tensionMultiplier));
        rightLegRigidbody.AddForce(CalculateTension(crossbarRightLegJoint.transform.position, modelRightLegJoint.transform.position, legsStringLength, tensionMultiplier));

    }

    void SwapModel()
    {
        GameObject newModel = Resources.Load<GameObject>("PuppetModels/" + puppetName);

        if (newModel == null) {
            Debug.Log("Puppet model " + puppetName + " not found in Resources/PuppetModels/");
            return;
        }

        GameObject newModelInstantiated = Instantiate(newModel, modelTransform);
        animator = newModelInstantiated.GetComponent<Animator>();

        GameObject puppetModelOld = puppetModel;
        puppetModel = newModelInstantiated;
        Destroy(puppetModelOld);

        //get all joints from the model where the "strings" will be attached
        //assuming a specific hierarchy here - will need to be adjusted based on model naming conventions
        if (animator == null)
        {
            animator = puppetModel.GetComponent<Animator>();
            if (animator == null)
            {
                Debug.Log("Animator component not found on the puppet model!");
            }
        }

        modelCenterJoint = animator.GetBoneTransform(HumanBodyBones.Head).gameObject;
        modelLeftArmJoint = animator.GetBoneTransform(HumanBodyBones.LeftHand).gameObject;
        modelRightArmJoint = animator.GetBoneTransform(HumanBodyBones.RightHand).gameObject;
        modelLeftLegJoint = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg).gameObject;
        modelRightLegJoint = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg).gameObject;

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

