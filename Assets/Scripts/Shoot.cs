using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot : MonoBehaviour
{
    [SerializeField]
    private float _maximumForce;

    [SerializeField]
    private float _maximumForceTime;
    
    private float _timeMouseButtonDown;
    
    private Camera _camera;
    
    bool isRagdoll = true;

    void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            print("Mouse Button Down");
            _timeMouseButtonDown = Time.time;
        }

        if (Input.GetMouseButtonUp(0))
        {
            print("Mouse Button Up");
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
             
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                Zombie zombie = hitInfo.collider.GetComponentInParent<Zombie>();
                print("Hit something: " + hitInfo.collider.name); 
                if (zombie != null)
                {
                    print("Hit a zombie!");
                    float mouseButtonDownDuration = Time.time - _timeMouseButtonDown;
                    float forcePercentage = mouseButtonDownDuration / _maximumForceTime;
                    float forceMagnitude = Mathf.Lerp(1, _maximumForce, forcePercentage);

                    Vector3 forceDirection = zombie.transform.position - _camera.transform.position;
                    forceDirection.y = 1;
                    forceDirection.Normalize();

                    Vector3 force = forceMagnitude * forceDirection;

                    if (isRagdoll)
                    {
                        zombie.TriggerRagdoll(force, hitInfo.point);
                        isRagdoll = false;
                    }else{
                        zombie.TriggerAnimation();
                        isRagdoll = true;
                    }
                }
            }
        }
    }
}
