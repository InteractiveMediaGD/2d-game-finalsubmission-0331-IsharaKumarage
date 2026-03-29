using UnityEngine;

/// <summary>
/// Creative Feature (Assignment Requirement): 
/// Updates the color of all walls (Obstacles) according to the player's health.
/// </summary>
public class WallColorUpdater : MonoBehaviour
{
    [Header("Settings")]
    public Gradient healthGradient;
    public HealthSystem playerHealth;

    private SpriteRenderer[] allWalls;

    void Start()
    {
        if (playerHealth == null) playerHealth = GameObject.FindGameObjectWithTag("Player")?.GetComponent<HealthSystem>();

        // Find all walls/obstacles in the scene
        Obstacle[] obstacles = Object.FindObjectsOfType<Obstacle>();
        allWalls = new SpriteRenderer[obstacles.Length];
        for (int i = 0; i < obstacles.Length; i++)
        {
            allWalls[i] = obstacles[i].GetComponent<SpriteRenderer>();
        }

        if (playerHealth != null)
        {
            playerHealth.onHealthChanged.AddListener(UpdateWallColors);
            UpdateWallColors(playerHealth.GetHealthPercent());
        }
    }

    void UpdateWallColors(float normalizedHealth)
    {
        Color targetColor = healthGradient.Evaluate(normalizedHealth);

        foreach (var sr in allWalls)
        {
            if (sr != null)
            {
                // Tint the wall while keeping it somewhat visible/dark
                sr.color = targetColor;
            }
        }
    }
}
