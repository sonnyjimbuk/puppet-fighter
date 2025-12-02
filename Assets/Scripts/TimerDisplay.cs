using UnityEngine;
using UnityEngine.UI;

public class TimerDisplay : MonoBehaviour
{
    public Image minuteTens;
    public Image minuteOnes;
    public Image secondTens;
    public Image secondOnes;

    public Sprite[] digitSprites = new Sprite[10];

    public void UpdateTimer(float timerSeconds) {
        int total = Mathf.FloorToInt(timerSeconds);

        int minutes = total / 60;
        int seconds = total % 60;
        
        int mT = minutes / 10;
        int mO = minutes % 10; 
        int sT = seconds / 10; 
        int sO = seconds % 10;
        Debug.Log(
    $"UpdateTimer() → total:{total} | " +
    $"minutes:{minutes} ({mT}{mO}) | " +
    $"seconds:{seconds} ({sT}{sO})"
);

        minuteTens.sprite = digitSprites[mT];
        minuteOnes.sprite = digitSprites[mO];
        secondTens.sprite = digitSprites[sT];
        secondOnes.sprite = digitSprites[sO];

    }
}
