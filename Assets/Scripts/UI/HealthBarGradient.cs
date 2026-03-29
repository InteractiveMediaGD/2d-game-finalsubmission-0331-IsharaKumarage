using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Assignment 02 - Creative Modification
/// Health bar decreases along a gradient of colors.
/// </summary>
public class HealthBarGradient : MonoBehaviour
{
    public Gradient healthGradient;
    public Image fillImage;
    public HealthSystem playerHealth;
    public string targetTag = "Player";

    void Start()
    {
        if (fillImage == null) fillImage = GetComponent<Image>();
        if (playerHealth == null && !string.IsNullOrEmpty(targetTag)) 
            playerHealth = GameObject.FindGameObjectWithTag(targetTag)?.GetComponent<HealthSystem>();

        if (playerHealth != null)
        {
            playerHealth.onHealthChanged.AddListener(UpdateColor);
            UpdateColor(playerHealth.GetHealthPercent());
        }
    }

    void UpdateColor(float normalizedHealth)
    {
        if (fillImage != null)
        {
            fillImage.color = healthGradient.Evaluate(normalizedHealth);
        }
    }
}