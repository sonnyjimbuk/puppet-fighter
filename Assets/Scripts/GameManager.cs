using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public GameObject pauseMenuUI; // Pause UI

    private bool isPaused = false;

    void Update()
    {
        // °´ÏÂ Esc ÇÐ»»ÔÝÍ£
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // Pause
        isPaused = true;
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f; // Restore
        isPaused = false;
    }

    // Restart
    public void RestartGame()
    {
        Time.timeScale = 1f;
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name); // reload current scene
    }

    // Quit game
    public void QuitGame()
    {
        Time.timeScale = 1f;
        Debug.Log("Quit Game");
        Application.Quit();
    }
}
