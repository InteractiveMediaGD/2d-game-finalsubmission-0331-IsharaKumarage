using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Player UI")]
    public Slider playerHealthBar;
    public Slider enemyHealthBar;

    [Header("Score")]
    public Text scoreText;

    [Header("Game Over / Win UI")]
    public GameObject winPanel;
    public GameObject losePanel;

    [Header("Main Menu UI")]
    public GameObject mainMenuPanel;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        winPanel?.SetActive(false);
        losePanel?.SetActive(false);
        UpdateScore(0);

        // 1. Auto-Link Player Health updates securely at runtime
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            var pHealth = player.GetComponent<HealthSystem>();
            if (pHealth != null) pHealth.onHealthChanged.AddListener(UpdatePlayerHealth);
        }

        // 2. Auto-Link Enemy Health updates securely at runtime
        GameObject enemy = GameObject.FindWithTag("Enemy");
        if (enemy != null)
        {
            var eHealth = enemy.GetComponent<HealthSystem>();
            if (eHealth != null) eHealth.onHealthChanged.AddListener(UpdateEnemyHealth);
        }
    }

    private float lastHealthPercent = 1f;

    public void UpdatePlayerHealth(float normalizedHealth)
    {
        if (playerHealthBar != null)
            playerHealthBar.value = normalizedHealth;

        // Visual enhancement: Screen flash red on damage
        if (normalizedHealth < lastHealthPercent)
        {
            TriggerRedFlash();
        }
        lastHealthPercent = normalizedHealth;
    }

    void TriggerRedFlash()
    {
        GameObject flash = new GameObject("DamageFlashOverlay");
        flash.transform.SetParent(transform, false); // Keep local transform clean
        flash.transform.SetAsLastSibling();          // Render on top of other UI

        var img = flash.AddComponent<Image>();
        img.color = new Color(1, 0, 0, 0.5f);
        img.raycastTarget = false;                   // Don't block clicks

        RectTransform rt = flash.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.sizeDelta = Vector2.zero;

        Destroy(flash, 0.2f); // Flash duration
    }

    public void UpdateEnemyHealth(float normalizedHealth)
    {
        if (enemyHealthBar != null)
            enemyHealthBar.value = normalizedHealth;
    }

    public void UpdateScore(int value)
    {
        // Animated display
        if (ScoreDisplay.Instance != null) ScoreDisplay.Instance.Refresh(value);
        // Fallback plain text
        else if (scoreText != null) scoreText.text = $"SCORE  {value:D5}";
    }

    public void ShowWinScreen()  => winPanel?.SetActive(true);
    public void ShowLoseScreen() => losePanel?.SetActive(true);

    // Button callbacks
    public void OnStartButton()    => SceneLoader.LoadScene("Level1");
    public void OnRestartButton()  => GameManager.Instance?.RestartGame();
    public void OnMainMenuButton() => GameManager.Instance?.GoToMainMenu();
    public void OnQuitButton()     => Application.Quit();
}
