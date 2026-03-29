using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    /// <summary>Load a scene by name immediately.</summary>
    public static void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>Load a scene with a delay (useful for death / win cutscenes).</summary>
    public static IEnumerator LoadSceneDelayed(MonoBehaviour caller, string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }

    // ── Convenience wrappers for UI buttons ────────────────────────────
    public void LoadMainMenu()   => LoadScene("MainMenu");
    public void LoadLevelSelect() => LoadScene("LevelSelect");
    public void LoadLevel1() => LoadLevel("Level1");
    public void LoadLevel2() => LoadLevel("Level2");
    public void LoadLevel3() => LoadLevel("Level3");

    private void LoadLevel(string sceneName)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.score = 0;
            GameManager.Instance.playerWon = false;
        }
        LoadScene("Level1");
    }
    public void LoadGameOver()  => LoadScene("GameOver");
    public void QuitGame()      => Application.Quit();
}
