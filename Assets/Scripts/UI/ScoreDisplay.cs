using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Drop onto any Score Text and it will animate (pop/bounce) when the score changes.
/// UIManager calls UpdateScore() which this script also handles.
/// </summary>
[RequireComponent(typeof(Text))]
public class ScoreDisplay : MonoBehaviour
{
    public static ScoreDisplay Instance;

    [Header("Animation")]
    public float popScale = 1.4f;
    public float popDuration = 0.25f;

    private Text label;
    private int displayedScore = 0;
    private Coroutine popCoroutine;

    void Awake()
    {
        label = GetComponent<Text>();
        if (Instance == null) Instance = this;
    }

    void Start() => Refresh(0);

    public void Refresh(int score)
    {
        displayedScore = score;
        if (label != null) label.text = $"{score:D5}";

        if (popCoroutine != null) StopCoroutine(popCoroutine);
        if (gameObject.activeInHierarchy)
            popCoroutine = StartCoroutine(PopAnim());
    }

    IEnumerator PopAnim()
    {
        float elapsed = 0;
        while (elapsed < popDuration)
        {
            float t = elapsed / popDuration;
            float scale = Mathf.Lerp(popScale, 1f, t);
            transform.localScale = Vector3.one * scale;
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        transform.localScale = Vector3.one;
    }
}
