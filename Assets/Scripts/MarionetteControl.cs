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

    public float xPos;
    public float yPos;

    public bool jumping;
    public Vector2 currentJumpVector;

    public float xSpeed;
    public float ySpeed;

    float timeSinceJumpStart;

    public float jumpStrengthMultiplier;
    public float maxJumpStrength;
    public float jumpDecayRate;

    public float whileJumpingCrossbarGravityRate;
    public float crossbarGravityRate;

    public float maxRotation;

    public float rotationToXSpeedMultiplier;

    public Vector3 startingPosition;

    public float xBoundary;
    public float yBoundary;

    public float headStringLength;
    public float armsStringLength;
    public float legsStringLength;
    public float tensionMultiplier;

    Transform crossbarTransform;

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

    GameObject modelHipJoint;

    Rigidbody headRigidbody;
    Rigidbody leftArmRigidbody;
    Rigidbody rightArmRigidbody;
    Rigidbody leftLegRigidbody;
    Rigidbody rightLegRigidbody;

    Rigidbody hipsRigidbody;

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

    public Vector3 gravityDirectionTest;

    int frameCounter = 0;

    void Awake()
    {

    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        xPos = startingPosition.x;
        yPos = startingPosition.y;

        crossbarTransform = transform.Find("crossbar");
        modelTransform = transform.Find("Model");
        SwapModel();
        tempPuppetName = puppetName;

        crossbarCenterJoint = transform.Find("crossbar/Joints/center").gameObject;
        crossbarLeftArmJoint = transform.Find("crossbar/Joints/center/arm_L").gameObject;
        crossbarRightArmJoint = transform.Find("crossbar/Joints/center/arm_R").gameObject;
        crossbarLeftLegJoint = transform.Find("crossbar/Joints/center/leg_L").gameObject;
        crossbarRightLegJoint = transform.Find("crossbar/Joints/center/leg_R").gameObject;        

        jumping = false;

        joycons = JoyconManager.Instance.j;
        if (joycons.Count < jc_ind + 1)
        {
            Debug.Log("Not enough Joy-Cons connected for the specified index!");
        }
    }


    // Update is called once per frame
    void Update()
    {
        frameCounter++;

        if (joycons.Count > 0)
        {
            j = joycons[jc_ind];
            stick = j.GetStick();
            gyro = j.GetGyro();
            accel = j.GetAccel();
            orientation = j.GetVector();

            if (frameCounter % 2 == 0)
            {
                //Debug.Log(string.Format("Joycon Stick x: {0:N} Stick y: {1:N}", stick[0], stick[1]));
                //Debug.Log(string.Format("Joycon Gyro x: {0:N} Gyro y: {1:N} Gyro z: {2:N}", gyro.x, gyro.y, gyro.z));
                //Debug.Log(string.Format("Joycon Accel x: {0:N} Accel y: {1:N} Accel z: {2:N}", accel.x, accel.y, accel.z));
                //Debug.Log(string.Format("Joycon Orientation x: {0:N} Orientation y: {1:N} Orientation z: {2:N} Orientation w: {3:N}", orientation.x, orientation.y, orientation.z, orientation.w));
              
            }
        }

        if (j.GetButtonDown(Joycon.Button.SR))
        {
            Debug.Log("Shoulder button 2 pressed");
            // GetStick returns a 2-element vector with x/y joystick components
            //Debug.Log(string.Format("Stick x: {0:N} Stick y: {1:N}", j.GetStick()[0], j.GetStick()[1]));

            CalculateJumpAccel(accel, orientation, true, j);

            // Joycon has no magnetometer, so it cannot accurately determine its yaw value. Joycon.Recenter allows the user to reset the yaw value.
            //j.Recenter();
        }

        if (j.GetButtonDown(Joycon.Button.SL))
        {
            Debug.Log("Shoulder button 1 pressed - Rumble activated");
            // Rumble for 200 milliseconds, with low frequency rumble at 160 Hz and high frequency rumble at 320 Hz. For more information check:
            //)

            j.SetRumble(160, 320, 0.2f, 200);

        } 

        if (puppetName != tempPuppetName)
        {
            SwapModel();
        }
        tempPuppetName = puppetName;

        float w = orientation.w;
        float x = orientation.x;
        float y = orientation.y;
        float z = orientation.z;            

        float roll = Mathf.Atan2(2 * y * w - 2 * x * z, 1 - 2 * y * y - 2 * z * z) * Mathf.Rad2Deg;
        float pitch = Mathf.Atan2(2 * x * w - 2 * y * z, 1 - 2 * x * x - 2 * z * z) * Mathf.Rad2Deg;
        float yaw = Mathf.Asin(2 * x * y + 2 * z * w) * Mathf.Rad2Deg;

        float adjustedRoll = Mathf.Cos(roll * Mathf.Deg2Rad) * (pitch + 90) + Mathf.Sin(roll * Mathf.Deg2Rad) * (yaw);

        if (j.isLeft)
        {
            adjustedRoll = -adjustedRoll;
        }

        float crossbarRotation = Mathf.Clamp(adjustedRoll, -maxRotation, maxRotation);

        crossbarTransform.eulerAngles = new Vector3(20, 0, crossbarRotation);

       
        Vector2 jumpAccel = CalculateJumpAccel(accel, orientation, false, j);
        float totalJumpAccel = jumpAccel.magnitude;
        
        // check to see if accel is strong enough to initiate a jump
        if (jumping == false && Mathf.Abs(yPos - startingPosition.y) < 0.5)
        {
            if (totalJumpAccel > 1.2)
            { 
                timeSinceJumpStart = 0;
                jumping = true;
                currentJumpVector = jumpAccel;
                //Debug.Log("Jump initiated with accel magnitude: " + totalJumpAccel);
                xSpeed = currentJumpVector.x * jumpStrengthMultiplier;
                ySpeed = currentJumpVector.y * jumpStrengthMultiplier;
            }
        }

        if (jumping == true)
        {
            timeSinceJumpStart += Time.deltaTime;
            float jumpAccelDotProduct = Vector2.Dot(jumpAccel.normalized, currentJumpVector.normalized) * jumpAccel.magnitude;
            if (jumpAccelDotProduct > currentJumpVector.magnitude)
            {
                xSpeed += jumpAccel.x - currentJumpVector.x;
                ySpeed += jumpAccel.y - currentJumpVector.y;
                currentJumpVector = jumpAccel;
                //Debug.Log("Jump redirected with new accel magnitude: " + totalJumpAccel);
            }
        }
        
        // clamp overall speed
        Vector2 totalSpeed = new Vector2(xSpeed, ySpeed);
        float speedMagnitude = totalSpeed.magnitude;
        speedMagnitude = Mathf.Clamp(speedMagnitude, 0, maxJumpStrength);

        if (totalSpeed.magnitude > 0)
        {
            xSpeed = (speedMagnitude / totalSpeed.magnitude) * xSpeed;
            ySpeed = (speedMagnitude / totalSpeed.magnitude) * ySpeed;
        }

        // decay rate applied to x speed only
        xSpeed = Mathf.MoveTowards(xSpeed, 0, jumpDecayRate * Time.deltaTime);


        // bump into floor and ceiling
        if (yPos < 0)
        {
            yPos = 0;
            ySpeed = 0;
        }

        if (yPos > yBoundary)
        {
            yPos = yBoundary;
            ySpeed = 0;
        }

        // bump into left and right walls
        if (xPos < -xBoundary)
        {
            xPos = -xBoundary;
            xSpeed = -0.5f * xSpeed;
        }

        if (xPos > xBoundary)
        {
            xPos = xBoundary;
            xSpeed = -0.5f * xSpeed;
        }


        if (timeSinceJumpStart > 0.2)
        {
            jumping = false;
            //Debug.Log("Initial jump ended");
        }

        if (jumping)
        {
            ySpeed -= Mathf.Sign(yPos - startingPosition.y) * whileJumpingCrossbarGravityRate * Time.deltaTime;
        }
        else
        {
            ySpeed -= Mathf.Sign(yPos - startingPosition.y) * crossbarGravityRate * Time.deltaTime;
        }

        if (yPos < startingPosition.y)
        {
            yPos += ySpeed * Time.deltaTime;
            yPos = Mathf.Min(yPos, startingPosition.y);
        }

        else if (yPos > startingPosition.y)
        {
            yPos += ySpeed * Time.deltaTime;
            yPos = Mathf.Max(yPos, startingPosition.y);
        }

        if (yPos == startingPosition.y)
        {
            //if the jump is over and we are back at starting position, return yPos to its resting position and kill the ySpeed
            if (jumping == false)
            {
                yPos = startingPosition.y;
                ySpeed = 0;
            }
            //if the jump just started and we are at starting position, apply ySpeed normally
            else
            {
                yPos += ySpeed * Time.deltaTime;
            }
        }

        if (jumping == false && yPos == startingPosition.y)
        {
            ySpeed = 0;
        }

        xPos += xSpeed * Time.deltaTime;

        xPos -= rotationToXSpeedMultiplier * (crossbarRotation / maxRotation) * Time.deltaTime;
        crossbarTransform.localPosition = new Vector3(xPos, yPos, 0);

        //apply tension forces to each joint

        headRigidbody.AddForce(CalculateTension(crossbarCenterJoint.transform.position, modelCenterJoint.transform.position, headStringLength, tensionMultiplier * 2));
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

        modelTransform.localPosition = new Vector3(xPos, yPos - 2, 0);

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
        modelHipJoint = animator.GetBoneTransform(HumanBodyBones.Hips).gameObject;

        headRigidbody = modelCenterJoint.GetComponent<Rigidbody>();
        leftArmRigidbody = modelLeftArmJoint.GetComponent<Rigidbody>();
        rightArmRigidbody = modelRightArmJoint.GetComponent<Rigidbody>();
        leftLegRigidbody = modelLeftLegJoint.GetComponent<Rigidbody>();
        rightLegRigidbody = modelRightLegJoint.GetComponent<Rigidbody>();
        hipsRigidbody = modelHipJoint.GetComponent<Rigidbody>();

    }

    Vector3 CalculateTension(Vector3 crossbarJointPosition, Vector3 modelJointPosition, float baseLength, float tensionMultiplier)
    {
        Vector3 modelToCrossbar = new Vector3();
        modelToCrossbar = crossbarJointPosition - modelJointPosition;

        float currentLength = modelToCrossbar.magnitude;
        float stretchLength = currentLength - baseLength;

        // with the clamp disabled, strings can also push if they are compressed: they act like stiff springs

        //stretchLength = Mathf.Clamp(stretchLength, 0, 100);

        return modelToCrossbar.normalized * stretchLength * tensionMultiplier;

    }

    Vector2 CalculateJumpAccel(Vector3 accelData, Quaternion orientation, bool printResults, Joycon j)
    {
        float accelX = accelData.x;
        float accelY = accelData.y;
        float accelZ = accelData.z; // correct for base -1 z accel that occurs for some reason even when still

        Quaternion orientationConjugate = new Quaternion(-orientation.x, -orientation.y, -orientation.z, orientation.w);

        Quaternion gravityCorrectionQuaternion = new Quaternion(gravityDirectionTest.x, gravityDirectionTest.y, gravityDirectionTest.z, 0);

        gravityCorrectionQuaternion = orientationConjugate * gravityCorrectionQuaternion * orientation;

        Quaternion accelQuaternion = new Quaternion (accelX + gravityCorrectionQuaternion.z, accelY + gravityCorrectionQuaternion.x, accelZ - gravityCorrectionQuaternion.y, 0);

        Quaternion accelCorrectedQuaternion = orientationConjugate * accelQuaternion * orientation;

        Vector3 accelCorrected = new Vector3(accelCorrectedQuaternion.x, accelCorrectedQuaternion.y, accelCorrectedQuaternion.z);

        float accelXCorrected = Vector3.Dot(accelCorrected, new Vector3(1f, 0f, 0f));
        float accelYCorrected = Vector3.Dot(accelCorrected, new Vector3(0f, 1f, 0f));

        if (j.isLeft)
        {
            //invert x accel for left joycon to match right joycon orientation
            accelXCorrected = -accelXCorrected;
        }

        Vector2 returnVector = new Vector2(-accelXCorrected, accelYCorrected);

        if (printResults)
        {
            Debug.Log("Raw accel data: " + accelData.ToString("F4"));

            Debug.Log("Gravity correction quat: " + gravityCorrectionQuaternion.ToString("F4"));

            Debug.Log("Gravity corrected accel data: " + accelQuaternion.ToString("F4"));

            Debug.Log("Corrected Accel X: " + accelXCorrected.ToString("F4") + " Corrected Accel Y: " + accelYCorrected.ToString("F4"));
        }

        return returnVector;
    }

   

}

