using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to the GameOver scene Canvas.
/// Reads GameManager.playerWon and shows correct text.
/// </summary>
public class GameOverController : MonoBehaviour
{
    public Text resultText;

    void Start()
    {
        if (resultText == null)
        {
            // Try to auto-find the text in the scene (handles both tool names)
            GameObject sub = GameObject.Find("GameOverSub") ?? GameObject.Find("SubtitleText");
            if (sub != null) resultText = sub.GetComponent<Text>();
        }

        if (resultText == null) 
        {
            Debug.LogWarning("[GameOverController] Missing resultText reference!");
            return;
        }

        bool won = GameManager.Instance != null && GameManager.Instance.playerWon;

        if (won)
        {
            resultText.text = "YOU WIN! 🏆";
            resultText.color = new Color(0.9f, 0.8f, 0.1f); // Gold
        }
        else
        {
            resultText.text = "YOU LOSE 💀";
            resultText.color = new Color(0.9f, 0.15f, 0.15f); // Red
        }
    }
}
