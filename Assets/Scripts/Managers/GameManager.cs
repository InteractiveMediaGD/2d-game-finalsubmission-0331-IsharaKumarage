using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Scene Names")]
    public string mainMenuScene = "MainMenu";
    public string gameOverScene = "GameOver";
    public string level1Scene   = "Level1";

    [Header("Game State")]
    public bool playerWon = false;

    [Header("Score")]
    public int score = 0;

    private float startGraceTime = 1.5f;
    private float sceneStartTime;
    private bool gameOverPending = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        sceneStartTime = Time.time;
        score = 0;
        gameOverPending = false;
        UIManager.Instance?.UpdateScore(0);
    }

    public void AddScore(int amount)
    {
        score += amount;
        UIManager.Instance?.UpdateScore(score);

        // Win Condition: Reach 100 points
        if (score >= 100 && !gameOverPending)
        {
            gameOverPending = true;
            playerWon = true;
            Debug.Log("[GameManager] Player Won! Score reached 100. Loading Game Over...");
            Invoke(nameof(LoadGameOver), 2f);
        }
    }

    public void PlayerDied()
    {
        if (gameOverPending) return;                           // prevent double trigger
        if (Time.time - sceneStartTime < startGraceTime) return; // grace period

        gameOverPending = true;
        playerWon = false;
        Debug.Log("[GameManager] Player died — loading Game Over in 2s...");
        Invoke(nameof(LoadGameOver), 2f);
    }

    void LoadGameOver()
    {
        // Force load the GameOver scene
        SceneManager.LoadScene(gameOverScene);
    }

    public void EnemyDefeated()
    {
        // In endless runner mode — score already added in Projectile.cs
        // This is kept for compatibility but does not trigger win condition
        AddScore(5);
    }

    public void RestartGame()
    {
        score = 0;
        gameOverPending = false;
        SceneLoader.LoadScene(level1Scene);
    }

    public void GoToMainMenu() => SceneLoader.LoadScene(mainMenuScene);
}
