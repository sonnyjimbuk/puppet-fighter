using UnityEngine;
using UnityEngine.InputSystem;

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
        modelTransform = transform.Find("Model");
        SwapModel();
        tempPuppetName = puppetName;

        crossbarCenterJoint = transform.Find("crossbar/Joints/center").gameObject;
        crossbarLeftArmJoint = transform.Find("crossbar/Joints/center/arm_L").gameObject;
        crossbarRightArmJoint = transform.Find("crossbar/Joints/center/arm_R").gameObject;
        crossbarLeftLegJoint = transform.Find("crossbar/Joints/center/leg_L").gameObject;
        crossbarRightLegJoint = transform.Find("crossbar/Joints/center/leg_R").gameObject;

        xPos = startingPosition.x;
        yPos = startingPosition.y;


    }

    void OnMove(InputValue value)
    {
        movementInput = value.Get<Vector2>();
    }

    // Update is called once per frame
    void Update()
    {
        if (centerRigidbody == null)
        {
            centerRigidbody = modelCenterJoint.GetComponent<Rigidbody>();
            leftArmRigidbody = modelLeftArmJoint.GetComponent<Rigidbody>();
            rightArmRigidbody = modelRightArmJoint.GetComponent<Rigidbody>();
            leftLegRigidbody = modelLeftLegJoint.GetComponent<Rigidbody>();
            rightLegRigidbody = modelRightLegJoint.GetComponent<Rigidbody>();
        }

        if (puppetName != tempPuppetName)
        {
            SwapModel();
        }
        tempPuppetName = puppetName;

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

