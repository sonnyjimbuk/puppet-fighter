using UnityEngine;
using System.Collections;

public class JoyconDualArmSwing : MonoBehaviour
{
    [Header("Joy-Con Settings")]
    public bool useLeftJoycon = true;
    public Joycon.Button leftTrigger = Joycon.Button.SHOULDER_2;  // ZL
    public Joycon.Button rightTrigger = Joycon.Button.SHOULDER_1; // ZR

    [Header("Swing Settings")]
    [Range(0.1f, 2f)] public float swingDuration = 0.8f;
    [Range(0.1f, 2f)] public float cooldown = 1.0f;
    public bool useTorque = true;
    [Range(0f, 200f)] public float swingForce = 70f;
    [Range(0f, 200f)] public float torquePower = 50f;

    [Header("Direction Fine-Tuning")]
    [Range(-1f, 1f)] public float sideOffset = 0.3f;    // outward
    [Range(-1f, 1f)] public float upOffset = 1.0f;      // how high it lifts
    [Range(-1f, 1f)] public float downOffset = -1.0f;   // how hard it swings down
    [Range(0f, 1f)] public float forwardFactor = 0.6f;  // forward blend

    private Joycon joycon;
    private bool canLeft = true, canRight = true;
    private Rigidbody lFore, lHand, rFore, rHand;

    void Start()
    {
        Invoke(nameof(FindJoyconAndBones), 0.5f);
    }

    void FindJoyconAndBones()
    {
        var jcs = JoyconManager.Instance?.j;
        if (jcs == null || jcs.Count == 0) return;

        joycon = jcs.Find(c => c.isLeft == useLeftJoycon);

        foreach (var rb in GetComponentsInChildren<Rigidbody>())
        {
            string n = rb.name;
            if (n.Contains("LeftForeArm")) lFore = rb;
            if (n.Contains("LeftHand")) lHand = rb;
            if (n.Contains("RightForeArm")) rFore = rb;
            if (n.Contains("RightHand")) rHand = rb;
        }
    }

    void Update()
    {
        if (joycon == null) return;

        if (canLeft && joycon.GetButtonDown(leftTrigger))
            StartCoroutine(SwingArm(true));

        if (canRight && joycon.GetButtonDown(rightTrigger))
            StartCoroutine(SwingArm(false));
    }

    IEnumerator SwingArm(bool isLeft)
    {
        if (isLeft) canLeft = false; else canRight = false;

        Rigidbody[] chain = isLeft ?
            new Rigidbody[] { lFore, lHand } :
            new Rigidbody[] { rFore, rHand };

        float t = 0f;
        while (t < swingDuration)
        {
            Vector3 dir = GetLiftAndDownDir(isLeft, t / swingDuration);
            foreach (var rb in chain)
            {
                if (rb == null) continue;
                Debug.DrawRay(rb.transform.position, dir * 0.3f, Color.magenta, 0.1f);

                if (useTorque)
                    rb.AddTorque(dir * torquePower * Time.deltaTime, ForceMode.Impulse);
                else
                    rb.AddForce(dir * swingForce * Time.deltaTime, ForceMode.Impulse);
            }
            t += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(cooldown);
        if (isLeft) canLeft = true; else canRight = true;
    }

    Vector3 GetLiftAndDownDir(bool isLeft, float progress)
    {
        // at start (0): move upward
        // at end (1): swing forward-downward
        Vector3 startDir = transform.up * upOffset;
        Vector3 endDir =
            (transform.forward * forwardFactor) +
            (transform.up * downOffset) +
            ((isLeft ? -transform.right : transform.right) * sideOffset);

        // smooth interpolate from up to forward-down
        Vector3 dir = Vector3.Slerp(startDir, endDir, progress);
        return dir.normalized;
    }
}



