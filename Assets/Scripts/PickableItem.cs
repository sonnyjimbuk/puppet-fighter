using UnityEngine;

public class PickableItem : MonoBehaviour
{
    [HideInInspector] public GameObject currentHolder = null; // 谁在持有这个物体
}
