using UnityEngine;
using System.Collections;

public class JoyconDualArmSwing : MonoBehaviour
{
    [Header("Joy-Con Settings")]
    public bool useLeftJoycon = true; // ✅ 左 Joy-Con = P1 (J/K)，右 Joy-Con = P2 (1/2)
    public Joycon.Button leftTrigger = Joycon.Button.SL;  // SL
    public Joycon.Button rightTrigger = Joycon.Button.SR; // SR

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
    private float leftAttackCooldown;
    private float rightAttackCooldown;
    private bool hasWeapon = false;
    private Rigidbody lFore, lHand, rFore, rHand;



    void Update()
    {
        hasWeapon = weaponDamage != null;
        if (!hasWeapon) return;

        /*// 🎮 Joy-Con Attack
        if (joycon != null)
        {
            if (canAttack1 && joycon.GetButtonDown(rightTrigger)) StartCoroutine(SwingArm(useLeftJoycon, true));
            if (canAttack2 && joycon.GetButtonDown(leftTrigger)) StartCoroutine(SwingArm(useLeftJoycon, false));
        }*/

        /*
        // ⌨ Keyboard Attack (J/K for P1, 1/2 for P2)
        if (canAttack1 && Input.GetKeyDown(keyAttack1)) StartCoroutine(SwingArm(useLeftJoycon, true));
        if (canAttack2 && Input.GetKeyDown(keyAttack2)) StartCoroutine(SwingArm(useLeftJoycon, false));
        */
    }

    public IEnumerator SwingArm(char leftOrRight)
    {
        Rigidbody[] chain;
        if (!hasWeapon) yield break;

        if (leftOrRight == 'L')
        {
            if (leftAttackCooldown > 0f) yield break;
            chain = new Rigidbody[] { lFore, lHand };

        }

        else if (leftOrRight == 'R')
        {
            if (rightAttackCooldown > 0f) yield break;
            chain = new Rigidbody[] { rFore, rHand };
        }

        else
        {
            Debug.LogWarning("⚠ JoyconDualArmSwing: Invalid arm specified for swing!");
            yield break;
        }

        // Start attack
        if (weaponDamage != null)
            weaponDamage.StartAttack();

        float t = 0f;
        while (t < swingDuration)
        {
            Vector3 dir = GetLiftAndDownDir(t / swingDuration);
            foreach (var rb in chain)
            {
                if (rb == null) continue;
                if (useTorque)
                    rb.AddTorque(dir * torquePower * Time.deltaTime, ForceMode.Impulse);
                else
                    rb.AddForce(dir * swingForce * Time.deltaTime, ForceMode.Impulse);
            }
            t += Time.deltaTime;
            Debug.Log($"⚔ Swinging {leftOrRight} arm with direction {dir}");
            yield return null;
        }

        if (weaponDamage != null)
            weaponDamage.EndAttack();

        yield break;

    }

    Vector3 GetLiftAndDownDir(float progress)
    {
        Vector3 startDir = transform.up * upOffset;
        Vector3 endDir = transform.up * downOffset;
            
            /*
            (transform.forward * forwardFactor) +
            (transform.up * downOffset) +
            ((isLeft ? -transform.right : transform.right) * sideOffset);*/

        Vector3 dir = Vector3.Slerp(startDir, endDir, progress);
        return dir.normalized;
    }
}
