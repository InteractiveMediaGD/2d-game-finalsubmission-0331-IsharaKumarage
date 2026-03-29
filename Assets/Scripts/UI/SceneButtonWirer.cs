using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach to any Canvas in MainMenu, LevelSelect, and GameOver scenes.
/// Finds buttons by name and wires them up at runtime automatically.
/// </summary>
public class SceneButtonWirer : MonoBehaviour
{
    void Start()
    {
        // Find ALL buttons in this scene and wire them by name
        Button[] allButtons = Object.FindObjectsOfType<Button>();

        foreach (var btn in allButtons)
        {
            string n = btn.name.ToLower();

            // Remove existing listeners first to avoid duplicates
            btn.onClick.RemoveAllListeners();

            if (n.Contains("play"))
                btn.onClick.AddListener(() => LoadScene("LevelSelect"));

            else if (n.Contains("level1") || n.Contains("arena"))
                btn.onClick.AddListener(() => LoadScene("Level1"));

            else if (n.Contains("level2"))
                btn.onClick.AddListener(() => LoadScene("Level2"));

            else if (n.Contains("level3"))
                btn.onClick.AddListener(() => LoadScene("Level3"));

            else if (n.Contains("levelselect") || n.Contains("select"))
                btn.onClick.AddListener(() => LoadScene("LevelSelect"));

            else if (n.Contains("mainmenu") || n.Contains("back") || n.Contains("menu"))
                btn.onClick.AddListener(() => LoadScene("MainMenu"));

            else if (n.Contains("restart") || n.Contains("again"))
                btn.onClick.AddListener(() => RestartGame());

            else if (n.Contains("exit") || n.Contains("quit"))
                btn.onClick.AddListener(() => Application.Quit());

            Debug.Log($"[ButtonWirer] Wired button: {btn.name}");
        }
    }

    void LoadScene(string sceneName) => SceneManager.LoadScene(sceneName);

    void RestartGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.score = 0;
            GameManager.Instance.playerWon = false;
        }
        SceneManager.LoadScene("Level1");
    }
}
