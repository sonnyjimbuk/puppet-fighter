using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ArmAnimationTrigger : MonoBehaviour
{
    [Header("Player Settings")]
    public bool isPlayer1 = true;   // Player1 = J, Player2 = 1

    private Animator animator;
    private List<Rigidbody> rightArmRigidbodies = new List<Rigidbody>();
    private bool modelReady = false;
    private int rightArmLayerIndex = -1;

    private readonly string[] rightArmKeywords = {
        "RightShoulder",
        "RightArm",
        "RightForeArm",
        "RightHand"
    };

    void Start()
    {
        StartCoroutine(WaitForAnimator());
    }

    IEnumerator WaitForAnimator()
    {
        while (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
            yield return null;
        }


        InitializeModel(animator.gameObject);
    }


    void InitializeModel(GameObject model)
    {

        FindRightArmRigidbodies(model.transform);

        FindRightArmLayerIndex();

        if (rightArmLayerIndex < 0)
        {
            Debug.LogError("ArmAnimationTrigger: 找不到名为 'RightArmLayer' 的 Layer！");
            return;
        }

        modelReady = true;
    }

    void FindRightArmRigidbodies(Transform root)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>())
        {
            foreach (string key in rightArmKeywords)
            {
                if (t.name.Contains(key))
                {
                    Rigidbody rb = t.GetComponent<Rigidbody>();
                    if (rb != null && !rightArmRigidbodies.Contains(rb))
                        rightArmRigidbodies.Add(rb);
                }
            }
        }
    }


    void FindRightArmLayerIndex()
    {
        for (int i = 0; i < animator.layerCount; i++)
        {
            if (animator.GetLayerName(i) == "RightArmLayer")
            {
                rightArmLayerIndex = i;
                break;
            }
        }
    }

    void Update()
    {
        if (!modelReady) return;

        if (isPlayer1)
        {
            if (Input.GetKeyDown(KeyCode.J))
                PlayRightArmAnimation();
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                PlayRightArmAnimation();
        }
    }

    public void PlayRightArmAnimation()
    {
        if (!modelReady || animator == null) return;
        StartCoroutine(AnimationRoutine());
    }

    IEnumerator AnimationRoutine()
    {
        foreach (var rb in rightArmRigidbodies)
            rb.isKinematic = true;

        animator.SetTrigger("AttackTrigger");

        yield return null;

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(rightArmLayerIndex);
        float duration = state.length;

        yield return new WaitForSeconds(duration);

        foreach (var rb in rightArmRigidbodies)
            rb.isKinematic = false;
    }
}
