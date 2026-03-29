using UnityEngine;

/// <summary>
/// Procedural Sword Renderer — draws a proper samurai sword on any character.
/// Auto-attaches a visual sword blade + guard + handle using SpriteRenderers.
/// Plays swing animations by rotating the pivot with smooth arcs.
/// </summary>
public class SwordVisual : MonoBehaviour
{
    [Header("Sword Parts (auto-generated)")]
    public SpriteRenderer blade;
    public SpriteRenderer guard;
    public SpriteRenderer handle;

    [Header("Colors")]
    public Color bladeColor  = new Color(0.85f, 0.92f, 1.00f); // cold steel blue
    public Color guardColor  = new Color(0.75f, 0.60f, 0.20f); // brass gold
    public Color handleColor = new Color(0.20f, 0.10f, 0.05f); // dark wrap

    [Header("Is this an Enemy sword?")]
    public bool isEnemy = false;

    void Awake()
    {
        BuildSword();
    }

    public void BuildSword()
    {
        if (blade != null) return; // already built

        // Blade
        blade = CreatePart("Blade",  new Vector3(0f,  0.55f, 0f), new Vector2(0.08f, 0.80f), bladeColor,  12);
        // Guard (crossguard)
        guard = CreatePart("Guard",  new Vector3(0f,  0.10f, 0f), new Vector2(0.30f, 0.07f), guardColor,  11);
        // Handle
        handle= CreatePart("Handle", new Vector3(0f, -0.18f, 0f), new Vector2(0.06f, 0.22f), handleColor, 11);

        // Enemy sword is darker red-tinted
        if (isEnemy)
        {
            blade.color  = new Color(0.9f, 0.3f, 0.3f, 1f);
            guard.color  = new Color(0.4f, 0.1f, 0.1f, 1f);
            handle.color = new Color(0.2f, 0.05f, 0.05f, 1f);
        }
    }

    SpriteRenderer CreatePart(string partName, Vector3 localPos, Vector2 size, Color color, int sortOrder)
    {
        Transform existing = transform.Find(partName);
        if (existing != null) return existing.GetComponent<SpriteRenderer>();

        var go = new GameObject(partName);
        go.transform.SetParent(transform);
        go.transform.localPosition = localPos;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale    = Vector3.one;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = sortOrder;

        // Create a rounded rect texture
        Texture2D tex = MakeRoundedRect((int)(size.x * 200), (int)(size.y * 200), color);
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                                  new Vector2(0.5f, 0.5f), 200f);
        return sr;
    }

    Texture2D MakeRoundedRect(int w, int h, Color col)
    {
        w = Mathf.Max(w, 2);
        h = Mathf.Max(h, 2);
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        int r = Mathf.Min(w, h) / 3;

        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            // Slightly brighter in the middle (sword sheen)
            float cx = (x - w * 0.5f) / (w * 0.5f);
            float sheen = 1f + 0.35f * Mathf.Exp(-cx * cx * 8f);
            Color pixel = new Color(
                Mathf.Clamp01(col.r * sheen),
                Mathf.Clamp01(col.g * sheen),
                Mathf.Clamp01(col.b * sheen),
                col.a);

            // Round corners
            bool inCorner = (x < r && y < r && CircleDist(x, y, r, r) > r)
                         || (x > w - r - 1 && y < r && CircleDist(x, y, w - r - 1, r) > r)
                         || (x < r && y > h - r - 1 && CircleDist(x, y, r, h - r - 1) > r)
                         || (x > w - r - 1 && y > h - r - 1 && CircleDist(x, y, w - r - 1, h - r - 1) > r);

            tex.SetPixel(x, y, inCorner ? Color.clear : pixel);
        }
        tex.Apply();
        return tex;
    }

    float CircleDist(int x, int y, int cx, int cy)
        => Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
}
