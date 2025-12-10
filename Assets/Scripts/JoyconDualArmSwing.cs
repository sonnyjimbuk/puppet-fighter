using UnityEngine;
using System.Collections;

public class JoyconDualArmSwing : MonoBehaviour
{
    [Header("Joy-Con Settings")]
    public bool useLeftJoycon = true; // ✅ 左 Joy-Con = P1 (J/K)，右 Joy-Con = P2 (1/2)
    public Joycon.Button leftTrigger = Joycon.Button.DPAD_UP;  // ZL
    public Joycon.Button rightTrigger = Joycon.Button.DPAD_DOWN; // ZR

    [Header("Swing Settings")]
    [Range(0.1f, 2f)] public float swingDuration = 0.8f;
    [Range(0.1f, 2f)] public float cooldown = 1.0f;
    public bool useTorque = true;
    [Range(0f, 200f)] public float swingForce = 70f;
    [Range(0f, 200f)] public float torquePower = 50f;

    [Header("Direction Fine-Tuning")]
    [Range(-1f, 1f)] public float sideOffset = 0.3f;
    [Range(-1f, 1f)] public float upOffset = 1.0f;
    [Range(-1f, 1f)] public float downOffset = -1.0f;
    [Range(0f, 1f)] public float forwardFactor = 0.6f;

    [Header("Attack System")]
    public WeaponDamage weaponDamage;

    private Joycon joycon;
    private bool canAttack1 = true;
    private bool canAttack2 = true;
    private bool hasWeapon = false;
    private Rigidbody lFore, lHand, rFore, rHand;

    private KeyCode keyAttack1; // ✅ J / 1
    private KeyCode keyAttack2; // ✅ K / 2

    void Start()
    {
        // 🎯 自动分配键位
        if (useLeftJoycon)
        {
            keyAttack1 = KeyCode.J;
            keyAttack2 = KeyCode.K;
        }
        else
        {
            keyAttack1 = KeyCode.Alpha1;
            keyAttack2 = KeyCode.Alpha2;
        }

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
        hasWeapon = weaponDamage != null;
        if (!hasWeapon) return;

        // 🎮 Joy-Con Attack
        if (joycon != null)
        {
            if (canAttack1 && joycon.GetButtonDown(Joycon.Button.DPAD_RIGHT)) StartCoroutine(SwingArm(useLeftJoycon, true));
            if (canAttack2 && joycon.GetButtonDown(Joycon.Button.DPAD_LEFT)) StartCoroutine(SwingArm(useLeftJoycon, false));
        }

        // ⌨ Keyboard Attack (J/K for P1, 1/2 for P2)
        if (canAttack1 && Input.GetKeyDown(keyAttack1)) StartCoroutine(SwingArm(useLeftJoycon, true));
        if (canAttack2 && Input.GetKeyDown(keyAttack2)) StartCoroutine(SwingArm(useLeftJoycon, false));
    }

    IEnumerator SwingArm(bool isLeftJoycon, bool isPrimary)
    {
        if (isPrimary) canAttack1 = false;
        else canAttack2 = false;

        Rigidbody[] chain = isLeftJoycon ?
            new Rigidbody[] { lFore, lHand } :
            new Rigidbody[] { rFore, rHand };

        // Start attack
        if (weaponDamage != null)
            weaponDamage.StartAttack();

        float t = 0f;
        while (t < swingDuration)
        {
            Vector3 dir = GetLiftAndDownDir(isLeftJoycon, t / swingDuration);
            foreach (var rb in chain)
            {
                if (rb == null) continue;
                if (useTorque)
                    rb.AddTorque(dir * torquePower * Time.deltaTime, ForceMode.Impulse);
                else
                    rb.AddForce(dir * swingForce * Time.deltaTime, ForceMode.Impulse);
            }
            t += Time.deltaTime;
            yield return null;
        }

        if (weaponDamage != null)
            weaponDamage.EndAttack();

        yield return new WaitForSeconds(cooldown);
        if (isPrimary) canAttack1 = true;
        else canAttack2 = true;
    }

    Vector3 GetLiftAndDownDir(bool isLeft, float progress)
    {
        Vector3 startDir = transform.up * upOffset;
        Vector3 endDir =
            (transform.forward * forwardFactor) +
            (transform.up * downOffset) +
            ((isLeft ? -transform.right : transform.right) * sideOffset);

        Vector3 dir = Vector3.Slerp(startDir, endDir, progress);
        return dir.normalized;
    }
}
