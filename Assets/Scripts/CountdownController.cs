using UnityEngine;
using UnityEngine.SceneManagement;

public class CountdownController : MonoBehaviour
{
    [Header("Timer Settings")]
    [Tooltip("Start time in seconds")]
    public float startTime = 120f; // e.g. 2 minutes

    [Tooltip("Reference to the timer display script")]
    public TimerDisplay timerDisplay;

    [Header("Game Over Menu")]
    [Tooltip("Menu panel shown when timer reaches zero")]
    public GameObject gameOverMenu;

    private float currentTime;
    private bool isGameOver = false;

    private void Start()
    {
        currentTime = startTime;

        // Hide menu at start
        if (gameOverMenu != null)
        {
            gameOverMenu.SetActive(false);
        }

        // Initialize display
        if (timerDisplay != null)
        {
            timerDisplay.UpdateTimer(currentTime);
        }
    }

    private void Update()
    {
        if (isGameOver)
            return;

        // Decrease timer
        currentTime -= Time.deltaTime;
        if (currentTime < 0f)
        {
            currentTime = 0f;
        }

        // Update UI
        if (timerDisplay != null)
        {
            timerDisplay.UpdateTimer(currentTime);
        }

        // Check for game over
        if (currentTime <= 0f && !isGameOver)
        {
            OnTimerEnd();
        }
    }

    /// <summary>
    /// Called when the timer reaches zero.
    /// Shows the menu and pauses the game.
    /// </summary>
    private void OnTimerEnd()
    {
        isGameOver = true;

        // Pause the game
        Time.timeScale = 0f;

        // Show menu UI
        if (gameOverMenu != null)
        {
            gameOverMenu.SetActive(true);
        }
    }

    /// <summary>
    /// Restart the current scene. Can be called from a UI Button.
    /// </summary>
    public void RestartScene()
    {
        Time.timeScale = 1f; // resume time
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }

    /// <summary>
    /// Load a specific scene by name (optional).
    /// </summary>
    public void LoadSceneByName(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Quit application (works in build; in editor it does nothing).
    /// </summary>
    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}
