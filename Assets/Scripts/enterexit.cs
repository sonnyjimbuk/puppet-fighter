using UnityEngine;

public class ObjectMover : MonoBehaviour
{
    public Vector3 enterPosition; // Position when the object is fully in view
    public Vector3 exitPosition;  // Position when the object is fully out of view
    public float moveSpeed = 2f;  // Speed of movement

    private bool movingIn = true; // True if moving into frame, false if moving out

    void Update()
    {
        if (movingIn)
        {
            // Move towards the enterPosition
            transform.position = Vector3.MoveTowards(transform.position, enterPosition, moveSpeed * Time.deltaTime);

            // If the object reaches the enterPosition, start moving out
            if (transform.position == enterPosition)
            {
                movingIn = false;
            }
        }
        else
        {
            // Move towards the exitPosition
            transform.position = Vector3.MoveTowards(transform.position, exitPosition, moveSpeed * Time.deltaTime);

            // If the object reaches the exitPosition, start moving in again
            if (transform.position == exitPosition)
            {
                movingIn = true;
            }
        }
    }
}
