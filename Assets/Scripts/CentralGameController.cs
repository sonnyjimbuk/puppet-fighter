using UnityEngine;
using System.Collections.Generic;

public class CentralGameController : MonoBehaviour
{
    //public int timerMinutes = 5;
    private float timerSeconds;

    public GameObject[] playerObjects;
    public TimerDisplay timerDisplay;

    private bool lockedPlayers = false;

    public float startTimeSeconds = 300f;

    void Start()
    {
      timerSeconds = startTimeSeconds;
    }


    void Update()
    {
        if (lockedPlayers)
            return;

        // Count down
        if (timerSeconds > 0f)
        {
            timerSeconds -= Time.deltaTime;

            if (timerSeconds < 0f)
                timerSeconds = 0f;

            if (timerDisplay != null)
                timerDisplay.UpdateTimer(timerSeconds);
        }
        else
        {
            LockPlayers();
        }
    }

  private void LockPlayers()
{
    if (lockedPlayers)
        return;

    Debug.Log("🛑 Timer reached 0 — freezing all players.");

    foreach (GameObject player in playerObjects)
    {
        // 1) Add a FreezeTransforms component to lock transforms in LateUpdate.
        //    Add it BEFORE disabling Behaviours so it stays active.
        FreezeTransforms freeze = player.GetComponent<FreezeTransforms>();
        if (freeze == null)
            freeze = player.AddComponent<FreezeTransforms>();

        // 2) Disable all Behaviour components (Animator, MonoBehaviours, Visual Scripting, etc.)
        //    We skip the FreezeTransforms component we just added.
        Behaviour[] behaviours = player.GetComponentsInChildren<Behaviour>(true);
        foreach (Behaviour b in behaviours)
        {
            if (b == null) continue;

            // keep the FreezeTransforms component enabled
            if (b is FreezeTransforms) 
                continue;

            // Optionally skip things you definitely want to keep (like renderers),
            // but Renderer isn't a Behaviour so we don't need to worry here.
            try
            {
                b.enabled = false;
                Debug.Log($"🔒 Disabled Behaviour: {b.GetType().Name} on {player.name}");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Could not disable {b.GetType().Name} on {player.name}: {ex.Message}");
            }
        }

        // 3) Also try to disable any 'ScriptMachine' / Visual Scripting runtime objects that might not be Behaviour.
        //    Many visual scripting components are MonoBehaviour/Behaviour; this is a safety log if they aren't.
        //    (If your team has custom non-Behaviour drivers this won't catch them — the transform freeze will.)

        // 4) Freeze physics (if any) as a fallback
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb == null)
            rb = player.AddComponent<Rigidbody>();

        rb.isKinematic = true; // make it safe for manual transform control
        rb.constraints = RigidbodyConstraints.FreezeAll;

        Debug.Log($"🧊 Applied Rigidbody freeze and transform lock on {player.name}");
    }

   lockedPlayers = true;
  }




    private void DisableMovementScripts(GameObject player)
    {
        // Gets ALL scripts on the object and its children
        MonoBehaviour[] scripts = player.GetComponentsInChildren<MonoBehaviour>(true);

        foreach (MonoBehaviour script in scripts)
        {
            // Skip non-movement scripts  
            string name = script.GetType().Name;

            if (name.Contains("Move") ||
                name.Contains("Controller") ||
                name.Contains("Joy") ||
                name.Contains("OSC") ||
                name.Contains("Input"))
            {
                script.enabled = false;
                Debug.Log($"🔒 Disabled: {name} on {player.name}");
            }
        }
    }

    private void FreezePlayerPhysics(GameObject player)
    {
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb == null)
            rb = player.AddComponent<Rigidbody>();

        rb.constraints = RigidbodyConstraints.FreezeAll;
    }
}
