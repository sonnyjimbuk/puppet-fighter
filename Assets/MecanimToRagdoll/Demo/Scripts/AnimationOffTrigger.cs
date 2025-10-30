using UnityEngine;

public class AnimationOffTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider collider)
    {
        if (collider.transform.parent)
        {
            GameObject GO = collider.transform.parent.gameObject;
            MTR_BoneControl[] BC = GO.GetComponentsInChildren<MTR_BoneControl>();
            if (BC != null)
                if (BC.Length > 0)
                {
                    MTR_Control C = BC[0].Root.GetComponent<MTR_Control>();
                    if (C)
                        C.Animate = false;
                }
        }
    }
}