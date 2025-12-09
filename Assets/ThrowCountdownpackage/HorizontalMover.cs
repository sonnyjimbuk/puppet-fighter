using UnityEngine;
using System.Collections;

public class HorizontalMover : MonoBehaviour
{
    public enum MoveMode
    {
        None,
        Random,
        Keyboard
    }

    [Header("X Axis Settings")]
    public MoveMode xMoveMode = MoveMode.None;
    public float xSpeed = 3f;
    public float minX = -5f;
    public float maxX = 5f;

    [Header("Z Axis Random Movement")]
    public bool enableZRandom = true;
    public float zSpeed = 2f;
    public float minZ = -5f;
    public float maxZ = 5f;
    public float zRandomChangeInterval = 1.5f;

    [Header("X Random Movement Settings")]
    public float xRandomChangeInterval = 1.5f;

    [Header("Keyboard Controls (Camera-based)")]
    public KeyCode leftKey = KeyCode.M;   // visually LEFT
    public KeyCode rightKey = KeyCode.N;  // visually RIGHT
    public Camera referenceCamera;

    // -------- NEW: Throw Animation Settings --------
    [Header("Throw Animation (Z Rotation)")]
    public float throwAngle = 10f;      // rotation amount
    public float throwSpeed = 15f;      // going outward
    public float returnSpeed = 15f;     // coming back

    private Quaternion originalRot;
    private Coroutine throwRoutine;


    // Internal movement state
    private float xRandomTimer = 0f;
    private int xRandomDir = 0;

    private float zRandomTimer = 0f;
    private int zRandomDir = 0;


    private void Awake()
    {
        if (referenceCamera == null)
            referenceCamera = Camera.main;

        originalRot = transform.localRotation;
    }


    private void Update()
    {
        Vector3 delta = Vector3.zero;

        // ---------------- X movement ----------------
        switch (xMoveMode)
        {
            case MoveMode.None:
                break;

            case MoveMode.Random:
                HandleXRandom(ref delta);
                break;

            case MoveMode.Keyboard:
                HandleXKeyboard(ref delta);
                break;
        }

        // ---------------- Z random movement ----------------
        if (enableZRandom)
            HandleZRandom(ref delta);

        // Apply movement
        transform.position += delta;

        // Clamp
        ClampPosition();
    }


    // ---------------- X RANDOM ----------------
    private void HandleXRandom(ref Vector3 delta)
    {
        xRandomTimer += Time.deltaTime;
        if (xRandomTimer >= xRandomChangeInterval)
        {
            xRandomTimer = 0f;
            xRandomDir = Random.value > 0.5f ? 1 : -1;
        }

        delta += new Vector3(xRandomDir * xSpeed * Time.deltaTime, 0f, 0f);
    }


    // ---------------- X KEYBOARD ----------------
    private void HandleXKeyboard(ref Vector3 delta)
    {
        float dir = 0f;

        if (Input.GetKey(leftKey))  // visually left
            dir -= 1f;

        if (Input.GetKey(rightKey)) // visually right
            dir += 1f;

        if (Mathf.Abs(dir) < 0.01f)
            return;

        Vector3 right = referenceCamera.transform.right;
        right.y = 0;
        right.Normalize();

        delta += right * dir * xSpeed * Time.deltaTime;
    }


    // ---------------- Z RANDOM ----------------
    private void HandleZRandom(ref Vector3 delta)
    {
        zRandomTimer += Time.deltaTime;
        if (zRandomTimer >= zRandomChangeInterval)
        {
            zRandomTimer = 0f;
            zRandomDir = Random.value > 0.5f ? 1 : -1;
        }

        delta += new Vector3(0f, 0f, zRandomDir * zSpeed * Time.deltaTime);
    }


    // ---------------- Clamp ----------------
    private void ClampPosition()
    {
        Vector3 pos = transform.position;

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);

        transform.position = pos;
    }


    // ======================================================
    //   NEW: Throw Animation (rotate Z axis)
    // ======================================================

    public void PlayThrowAnimation()
    {
        if (throwRoutine != null)
            StopCoroutine(throwRoutine);

        throwRoutine = StartCoroutine(ThrowAnim());
    }

    private IEnumerator ThrowAnim()
    {
        Quaternion targetRot = originalRot * Quaternion.Euler(0, 0, throwAngle);

        // rotate to throwAngle
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * throwSpeed;
            transform.localRotation = Quaternion.Lerp(originalRot, targetRot, t);
            yield return null;
        }

        // rotate back
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * returnSpeed;
            transform.localRotation = Quaternion.Lerp(targetRot, originalRot, t);
            yield return null;
        }

        throwRoutine = null;
    }
}
