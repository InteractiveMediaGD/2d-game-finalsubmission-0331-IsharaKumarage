using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach to a SplashScreen scene Camera or a Canvas GameObject.
/// Shows BrokenWarrior_Logo for `displayTime` seconds, then loads the next scene.
/// </summary>
public class SplashScreen : MonoBehaviour
{
    [Header("Logo")]
    public Image logoImage;               // Drag BrokenWarrior_Logo sprite here
    public float displayTime = 2.5f;      // How long to show the logo

    [Header("Fade")]
    public float fadeInTime  = 0.5f;
    public float fadeOutTime = 0.5f;

    [Header("Next Scene")]
    public string nextScene = "MainMenu"; // Scene to load after splash

    void Start()
    {
        if (logoImage != null)
            StartCoroutine(RunSplash());
        else
        {
            Debug.LogWarning("[SplashScreen] logoImage not assigned! Skipping...");
            SceneManager.LoadScene(nextScene);
        }
    }

    IEnumerator RunSplash()
    {
        // Start invisible
        SetAlpha(0f);

        // Fade IN
        yield return Fade(0f, 1f, fadeInTime);

        // Hold
        yield return new WaitForSeconds(displayTime);

        // Fade OUT
        yield return Fade(1f, 0f, fadeOutTime);

        SceneManager.LoadScene(nextScene);
    }

    IEnumerator Fade(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(Mathf.Lerp(from, to, elapsed / duration));
            yield return null;
        }
        SetAlpha(to);
    }

    void SetAlpha(float a)
    {
        if (logoImage != null)
            logoImage.color = new Color(logoImage.color.r, logoImage.color.g, logoImage.color.b, a);
    }
}
