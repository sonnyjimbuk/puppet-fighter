using UnityEngine;

public float xPos;
public float yPos;

public class MarionetteControl : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        GameObject centerJoint = GameObject.Find("crossbar/Joints/center");
        GameObject leftArmJoint = GameObject.Find("crossbar/Joints/arm_L");
        GameObject rightArmJoint = GameObject.Find("crossbar/Joints/arm_R");
        GameObject leftLegJoint = GameObject.Find("crossbar/Joints/leg_L");
        GameObject rightLegJoint = GameObject.Find("crossbar/Joints/leg_R");
        if (centerJoint != null)
        {
            centerJoint.transform.localRotation = Quaternion.Euler(60, 0, 0);
        }
        else
        {
            Debug.LogError("Center joint not found!");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
