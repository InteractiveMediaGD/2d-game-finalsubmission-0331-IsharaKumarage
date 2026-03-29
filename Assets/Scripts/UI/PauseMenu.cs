using UnityEngine;

/// <summary>
/// Attach to a GameObject in Level1 scene.
/// Handles ESC to pause and resume.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;
    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        pausePanel.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void ResumeGame()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        SceneLoader.LoadScene("MainMenu");
    }
}
