using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class CentralGameController : MonoBehaviour
{
    public int timerMinutes = 5; // Default timer time
    private float timerSeconds; // To store timer time in seconds
    //public TextMeshProUGUI timerTimeText; // Text UI element for displaying the timer time
    public GameObject[] playerObjects;
    public TimerDisplay timerDisplay;

    private bool lockPlayers = false;

    private void Start()
    {
       timerSeconds = timerMinutes * 60.0f;
       //foreach (player in playerObjects)
    }

    void Update()
    {

      //Debug.Log($"Retrieving timerSeconds:{timerSeconds}");

      if (timerSeconds > 0.0) {
         timerSeconds -= Time.deltaTime;
         if(timerDisplay!=null)
        timerDisplay.UpdateTimer(timerSeconds);
        Debug.Log($"Checking timerDisplay:{timerDisplay}");

      } else {
        Debug.Log($"UpdateT()else branch → timerSeconds:{timerSeconds}");
        LockPlayers();
      }
       
    }

    /* private void UpdateTimerDisplay()
    {
        int minutes = (int)(timerSeconds / 60);
        int seconds = (int)(timerSeconds % 60);
        timerTimeText.text = $"{minutes:00}:{seconds:00}";
    } */

    private void LockPlayers()
    {
      if (!lockPlayers) {
       foreach (GameObject player in playerObjects) 
       {
        // Add a Rigidbody component
        Rigidbody rb = player.AddComponent<Rigidbody>();

        // Configure Rigidbody properties
        rb.constraints =  RigidbodyConstraints.FreezePositionZ |  RigidbodyConstraints.FreezePositionY |  RigidbodyConstraints.FreezePositionX;
       }
        
        lockPlayers = false;
      }
    }
}
