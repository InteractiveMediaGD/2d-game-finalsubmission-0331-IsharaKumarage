using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Creates a professional dark-edge vignette overlay on the game camera UI.
/// Also handles a low-health red pulse effect.
/// Attach to any active Canvas or the GameManager.
/// </summary>
public class ScreenVignette : MonoBehaviour
{
    public static ScreenVignette Instance;

    [Header("Vignette")]
    public float vignetteStrength = 0.6f;   // 0 = none, 1 = very dark edges

    [Header("Low Health Pulse")]
    public float lowHealthThreshold = 0.3f;  // Pulse when health below 30%
    public Color pulseColor = new Color(0.8f, 0f, 0f, 0.35f);
    public float pulseSpeed = 1.5f;

    private Image vignetteImage;
    private Image pulseImage;
    private bool pulsing = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start() => BuildVignette();

    void BuildVignette()
    {
        // Find or create a Canvas
        Canvas canvas = Object.FindObjectOfType<Canvas>();

        if (canvas == null) return;

        // Outer Vignette (dark edges)
        var vigGO = new GameObject("Vignette");
        vigGO.transform.SetParent(canvas.transform, false);
        vigGO.transform.SetAsLastSibling(); // render on top of everything except panels

        var rt = vigGO.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

        vignetteImage = vigGO.AddComponent<Image>();
        vignetteImage.raycastTarget = false;

        // Build radial gradient texture programmatically
        vignetteImage.sprite = BuildVignetteSprite(256, vignetteStrength);
        vignetteImage.color  = Color.white;

        // Low-health pulse overlay (full screen red flash)
        var pulseGO = new GameObject("LowHealthPulse");
        pulseGO.transform.SetParent(canvas.transform, false);
        pulseGO.transform.SetAsLastSibling();

        var pRT = pulseGO.AddComponent<RectTransform>();
        pRT.anchorMin = Vector2.zero; pRT.anchorMax = Vector2.one;
        pRT.offsetMin = Vector2.zero; pRT.offsetMax = Vector2.zero;

        pulseImage = pulseGO.AddComponent<Image>();
        pulseImage.color = new Color(0.8f, 0f, 0f, 0f);
        pulseImage.raycastTarget = false;
    }

    // Called by HealthSystem when player health changes
    public void OnPlayerHealthChanged(float normalizedHealth)
    {
        if (normalizedHealth < lowHealthThreshold && !pulsing)
        {
            pulsing = true;
            StartCoroutine(PulseRoutine());
        }
        else if (normalizedHealth >= lowHealthThreshold)
        {
            pulsing = false;
            if (pulseImage != null) pulseImage.color = new Color(0.8f, 0f, 0f, 0f);
        }
    }

    IEnumerator PulseRoutine()
    {
        while (pulsing)
        {
            yield return Fade(0f, pulseColor.a, 0.4f);
            yield return Fade(pulseColor.a, 0f,          0.4f);
        }
    }

    IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            if (pulseImage != null)
                pulseImage.color = new Color(pulseColor.r, pulseColor.g, pulseColor.b, Mathf.Lerp(from, to, t / duration));
            yield return null;
        }
    }

    // Builds a radial dark vignette as a sprite at runtime
    static Sprite BuildVignetteSprite(int size, float strength)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        var cx = size * 0.5f;
        var cy = size * 0.5f;
        var maxDist = Mathf.Sqrt(cx * cx + cy * cy);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - cx, dy = y - cy;
                float dist = Mathf.Sqrt(dx * dx + dy * dy) / maxDist;
                float alpha = Mathf.Pow(dist, 1.5f) * strength;
                tex.SetPixel(x, y, new Color(0, 0, 0, Mathf.Clamp01(alpha)));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    public static void NotifyHealthChange(float normalized)
        => Instance?.OnPlayerHealthChanged(normalized);
}
