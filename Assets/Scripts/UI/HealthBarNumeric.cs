using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Assignment 02 - Core Requirement 1
/// Displays the player's health as a numeric value.
/// </summary>
[RequireComponent(typeof(Text))]
public class HealthBarNumeric : MonoBehaviour
{
    [Header("Settings")]
    public string targetTag = "Player";
    public HealthSystem targetHealth;

    private Text text;

    void Start()
    {
        text = GetComponent<Text>();
        
        if (targetHealth == null && !string.IsNullOrEmpty(targetTag))
            targetHealth = GameObject.FindGameObjectWithTag(targetTag)?.GetComponent<HealthSystem>();

        if (targetHealth != null)
        {
            targetHealth.onHealthChanged.AddListener(OnHealthChanged);
            UpdateDisplay(targetHealth.currentHealth, targetHealth.maxHealth);
        }
    }

    void OnHealthChanged(float normalizedHealth)
    {
        if (targetHealth != null)
        {
            UpdateDisplay(targetHealth.currentHealth, targetHealth.maxHealth);
        }
    }

    void UpdateDisplay(float current, float max)
    {
        if (text != null)
        {
            text.text = $"{(int)current} / {(int)max}";
        }
    }
}
