using UnityEngine;

/// <summary>
/// A Health Pack collectible shown as a first-aid kit sprite.
/// Hover-bobs to attract attention. Click or walk into it to heal.
/// </summary>
public class HealthPack : MonoBehaviour
{
    [Header("Settings")]
    public float healAmount = 30f;

    [Header("Sprite (auto-loaded if blank)")]
    public Sprite packSprite;

    private Vector3 startPos;
    private float bobTimer;
    private SpriteRenderer sr;

    void Awake()
    {
        // Auto-attach SpriteRenderer and load sprite
        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 5;

        if (packSprite == null)
            packSprite = Resources.Load<Sprite>("Sprites/HealthPack");

        if (packSprite == null)
            packSprite = LoadFromAssets();

        if (packSprite != null)
            sr.sprite = packSprite;
        else
            sr.color = new Color(0.1f, 0.8f, 0.2f); // fallback green square

        // Ensure a collider exists for clicking and trigger detection
        if (GetComponent<Collider2D>() == null)
        {
            var col = gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.9f, 0.9f);
        }

        // Initialize size
        transform.localScale = Vector3.one * 0.3f; // small pickup size
    }

    void Start()
    {
        // Set startPos in Start() so it happens AFTER Instantiate() sets the position
        startPos = transform.position;
    }


    void Update()
    {
        // Gentle hover bob
        bobTimer += Time.deltaTime * 2.0f;
        float bob = Mathf.Sin(bobTimer) * 0.12f;
        transform.position = startPos + new Vector3(0, bob, 0);

        // Subtle scale pulse (breathe)
        float pulse = 1f + Mathf.Sin(bobTimer * 1.5f) * 0.04f;
        transform.localScale = Vector3.one * 0.3f * pulse;
    }

    void OnMouseDown() => HealPlayer();

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        HealPlayer();
    }

    void HealPlayer()
    {
        HealthSystem hs = GameObject.FindGameObjectWithTag("Player")?.GetComponent<HealthSystem>();
        if (hs != null && !hs.IsDead())
        {
            hs.Heal(healAmount);
            EffectsManager.Instance?.PlayHeal(transform.position);
            AudioManager.Instance?.PlayHeal();
            Destroy(gameObject);
        }
    }

    // Tries to load from the Assets/Sprites folder directly at runtime in editor
    Sprite LoadFromAssets()
    {
#if UNITY_EDITOR
        return UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/HealthPack.png");
#else
        return null;
#endif
    }
}
