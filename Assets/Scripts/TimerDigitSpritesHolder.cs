using UnityEngine;

[System.Serializable]
public class DigitSprites {
    public Sprite[] digits = new Sprite[10];
}

public class TimerDigitSpritesHolder : MonoBehaviour
{
    public DigitSprites digitSprites;
}

