using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimpleTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    public float startTime = 99f;
    public bool countDown = true;
    public bool autoStart = true;

    [Header("UI References")]
    public TextMeshProUGUI timerText; // 若用 TMP
    // public Text timerText; // 如果用普通 Text 就换这一行

    private float currentTime;
    private bool running = false;
    private bool flashing = false;
    private float flashTimer = 0f;

    void Start()
    {
        currentTime = startTime;
        if (autoStart) running = true;

        // 若没手动拖 text，就自动创建
        if (timerText == null)
        {
            GameObject textObj = new GameObject("TimerText");
            textObj.transform.SetParent(transform);
            timerText = textObj.AddComponent<TextMeshProUGUI>();
            timerText.fontSize = 30;
            timerText.alignment = TextAlignmentOptions.Center;
            timerText.color = Color.yellow;

            RectTransform rect = textObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0, -35); // 血条下方
            rect.sizeDelta = new Vector2(100, 50);
        }
    }

    void Update()
    {
        if (!running) return;

        if (countDown)
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0)
            {
                currentTime = 0;
                running = false;
                flashing = false;
                timerText.color = Color.red;
                Debug.Log("⏱ Timer finished!");
            }
        }
        else
        {
            currentTime += Time.deltaTime;
        }

        if (timerText != null)
        {
            timerText.text = Mathf.CeilToInt(currentTime).ToString("00");

            // 低于10秒开始闪烁
            if (currentTime <= 10f && currentTime > 0f)
            {
                flashing = true;
            }

            if (flashing)
            {
                flashTimer += Time.deltaTime;
                if (flashTimer >= 0.5f)
                {
                    flashTimer = 0f;
                    timerText.color = (timerText.color == Color.red) ? Color.white : Color.red;
                }
            }
        }
    }
}
