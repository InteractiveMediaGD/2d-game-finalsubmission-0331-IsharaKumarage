using UnityEngine;

/// <summary>
/// Attach to the Controls Screen Canvas/Panel.
/// Call Show() / Hide() from UIManager or a button.
/// </summary>
public class ControlsMenu : MonoBehaviour
{
    [Header("Panel Reference")]
    public GameObject controlsPanel;

    [Header("Optional – fade group")]
    public CanvasGroup canvasGroup;

    void Start()
    {
        Hide();
    }

    public void Show()
    {
        controlsPanel.SetActive(true);
        if (canvasGroup != null) canvasGroup.alpha = 1f;
    }

    public void Hide()
    {
        controlsPanel.SetActive(false);
    }

    // Called by ESC key check
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && controlsPanel.activeSelf)
            Hide();
    }
}
