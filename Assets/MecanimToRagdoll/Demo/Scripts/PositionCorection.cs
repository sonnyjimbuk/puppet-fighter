using UnityEngine;

public class PositionCorection : MonoBehaviour
{
    public float Z;
    void Update()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, Z);
    }
}