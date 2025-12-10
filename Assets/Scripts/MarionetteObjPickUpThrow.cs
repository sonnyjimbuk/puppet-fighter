using UnityEngine;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

public class MarionetteObjPickUpThrow : MonoBehaviour
{
    [Header("References")]
    public MarionetteControl marionette;
    private Transform rightHand;
    private Transform leftHand;
    private Rigidbody heldRigidbody;
    private Collider[] heldColliders;
    private bool holdingRightHand = true;

    [Header("Pickup Settings")]
    public float pickupRange = 2.5f;
    public float throwForce = 50f;
    public float dropTime = 10f;
    public float pickupCooldown = 0.5f;

    [Header("Smooth Pickup Settings")]
    public float pickupSmoothSpeed = 6f;
    public AnimationCurve pickupEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Joycon Settings")]
    public bool useJoycon = true;
    public bool isPlayer1 = true;
    private Joycon leftJoycon;
    private Joycon rightJoycon;

    private bool pickedUp = false;
    private bool isFlyingToHand = false;
    private float pickupTimer = 0f;
    private float cooldownTimer = 0f;

    void Start()
    {
        if (marionette == null)
            marionette = GetComponent<MarionetteControl>();

        // Setup Joycon references
        if (useJoycon && JoyconManager.Instance.j.Count > 0)
        {
            var joycons = JoyconManager.Instance.j;
            leftJoycon = joycons.Find(c => c.isLeft);
            rightJoycon = joycons.Find(c => !c.isLeft);
        }
    }

    void Update()
    {
        cooldownTimer -= Time.deltaTime;

        // Ensure left/right hand references are found
        if ((rightHand == null || leftHand == null) && marionette != null)
        {
            FieldInfo rightField = typeof(MarionetteControl).GetField("modelRightArmJoint", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo leftField = typeof(MarionetteControl).GetField("modelLeftArmJoint", BindingFlags.NonPublic | BindingFlags.Instance);

            if (rightField != null)
                rightHand = (rightField.GetValue(marionette) as GameObject)?.transform;
            if (leftField != null)
                leftHand = (leftField.GetValue(marionette) as GameObject)?.transform;
        }

        if (marionette == null || (rightHand == null && leftHand == null))
            return;

        // 🎮 Input handling
        bool pickupLeft = false;
        bool pickupRight = false;
        bool dropPressed = false;

        if (useJoycon)
        {
            if (isPlayer1 && leftJoycon != null)
            {
                if (leftJoycon.GetButtonDown(Joycon.Button.SHOULDER_2)) pickupLeft = true;
                if (leftJoycon.GetButtonDown(Joycon.Button.DPAD_DOWN)) dropPressed = true;
            }
            else if (!isPlayer1 && rightJoycon != null)
            {
                if (rightJoycon.GetButtonDown(Joycon.Button.SHOULDER_2)) pickupRight = true;
                if (rightJoycon.GetButtonDown(Joycon.Button.DPAD_DOWN)) dropPressed = true;
            }
        }

        // Keyboard fallback
        if (isPlayer1 && Input.GetKeyDown(KeyCode.Z)) pickupLeft = true;
        if (!isPlayer1 && Input.GetKeyDown(KeyCode.X)) pickupRight = true;
        if (Input.GetKeyDown(KeyCode.C)) dropPressed = true;

        // Main pickup/drop logic
        if (!pickedUp && !isFlyingToHand && cooldownTimer <= 0f)
        {
            if (pickupLeft && leftHand != null)
            {
                holdingRightHand = false;
                TryPickUpNearbyWeapon(leftHand);
            }
            else if (pickupRight && rightHand != null)
            {
                holdingRightHand = true;
                TryPickUpNearbyWeapon(rightHand);
            }
        }
        else
        {
            pickupTimer += Time.deltaTime;
            if (pickupTimer >= dropTime || dropPressed)
                DropObject();
            else if (pickedUp)
                HoldObjectStable();
        }
    }

    void TryPickUpNearbyWeapon(Transform hand)
    {
        Collider[] hits = Physics.OverlapSphere(hand.position, pickupRange);

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Weapon")) continue;

            Rigidbody rb = hit.attachedRigidbody;
            if (rb == null) continue;

            PickableItem pickable = rb.GetComponent<PickableItem>();
            if (pickable == null) continue;

            // Skip if already held by another player
            if (pickable.currentHolder != null && pickable.currentHolder != gameObject)
            {
                Debug.Log($"⚠ {rb.name} is owned by {pickable.currentHolder.name}");
                continue;
            }

            pickable.currentHolder = gameObject;

            // ✅ Play pickup sound
            pickable.PlayPickUpSound();

            StartCoroutine(FlyToHand(rb, hand));
            Debug.Log($"🟢 {gameObject.name} picked up {rb.name}");
            return;
        }

        Debug.Log($"⚠ No Weapon found near {(hand == rightHand ? "right" : "left")} hand.");
    }

    IEnumerator FlyToHand(Rigidbody objRigidbody, Transform hand)
    {
        isFlyingToHand = true;
        heldRigidbody = objRigidbody;
        heldColliders = objRigidbody.GetComponentsInChildren<Collider>();
        foreach (var col in heldColliders) col.enabled = false;

        Vector3 startPos = objRigidbody.position;
        Quaternion startRot = objRigidbody.rotation;

        Transform grip = objRigidbody.transform.Find("GripPoint");
        Vector3 targetPos = grip ? grip.position : hand.position;
        Quaternion targetRot = grip ? grip.rotation : hand.rotation;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * pickupSmoothSpeed;
            float ease = pickupEase.Evaluate(t);

            objRigidbody.MovePosition(Vector3.Lerp(startPos, targetPos, ease));
            objRigidbody.MoveRotation(Quaternion.Slerp(startRot, targetRot, ease));
            yield return null;
        }

        PickUpObject(objRigidbody, hand, grip);
        isFlyingToHand = false;
    }

    void PickUpObject(Rigidbody objRigidbody, Transform hand, Transform grip)
    {
        heldRigidbody.useGravity = false;
        heldRigidbody.isKinematic = false;

        if (grip != null)
        {
            objRigidbody.transform.SetParent(hand, true);
            Vector3 offsetPos = hand.position - grip.position;
            Quaternion offsetRot = Quaternion.Inverse(grip.rotation) * hand.rotation;
            objRigidbody.transform.position += offsetPos;
            objRigidbody.transform.rotation *= offsetRot;
            objRigidbody.transform.SetParent(null);
        }
        else
        {
            objRigidbody.transform.position = hand.position;
            objRigidbody.transform.rotation = hand.rotation;
        }

        pickedUp = true;
        pickupTimer = 0f;
    }

    void HoldObjectStable()
    {
        if (heldRigidbody == null) return;
        Transform hand = holdingRightHand ? rightHand : leftHand;
        if (hand == null) return;

        heldRigidbody.MovePosition(Vector3.Lerp(heldRigidbody.position, hand.position, Time.deltaTime * 25f));
        heldRigidbody.MoveRotation(Quaternion.Slerp(heldRigidbody.rotation, hand.rotation, Time.deltaTime * 25f));
    }

    void DropObject()
    {
        if (heldRigidbody == null) return;

        PickableItem pickable = heldRigidbody.GetComponent<PickableItem>();
        if (pickable != null && pickable.currentHolder != gameObject)
        {
            Debug.Log($"⚠ {gameObject.name} tried to drop but isn't owner.");
            return;
        }

        foreach (var col in heldColliders)
            col.enabled = true;

        if (pickable != null)
        {
            pickable.currentHolder = null;
            pickable.PlayDropSound(); // ✅ Play drop sound
        }

        heldRigidbody.useGravity = true;
        heldRigidbody.linearVelocity = (holdingRightHand ? rightHand.forward : leftHand.forward) * throwForce;
        heldRigidbody.angularVelocity = Vector3.zero;

        heldRigidbody = null;
        heldColliders = null;
        pickedUp = false;
        cooldownTimer = pickupCooldown;

        Debug.Log($"🔴 {gameObject.name} dropped weapon");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        if (rightHand != null)
            Gizmos.DrawWireSphere(rightHand.position, pickupRange);
        if (leftHand != null)
            Gizmos.DrawWireSphere(leftHand.position, pickupRange);
    }
}
