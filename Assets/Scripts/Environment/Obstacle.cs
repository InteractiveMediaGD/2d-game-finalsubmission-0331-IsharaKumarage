using UnityEngine;

/// <summary>
/// Assignment 02 - Obstacle Logic
/// Handles player collision (decrement health, pass through) 
/// and score increment (passage trigger).
/// </summary>
public class Obstacle : MonoBehaviour
{
    [Header("Settings")]
    public float healthPenalty = 20f;
    public bool isPassage = false; // Set to true for the 'hole' trigger

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (isPassage)
        {
            // Successfully went through the hole
            GameManager.Instance?.AddScore(1);
            Debug.Log("[Obstacle] Passed through hole! Score +1");
            // Disable to prevent multiple scores
            GetComponent<Collider2D>().enabled = false;
        }
        else
        {
            // Hit the wall
            HealthSystem hs = other.GetComponent<HealthSystem>();
            if (hs != null)
            {
                hs.TakeDamage(healthPenalty);
                Debug.Log("[Obstacle] Hit wall! Health -" + healthPenalty);
            }
            // Logic to pass through: briefly disable collider or just rely on Trigger
            // If this is a trigger, the player naturally passes through.
        }
    }
}