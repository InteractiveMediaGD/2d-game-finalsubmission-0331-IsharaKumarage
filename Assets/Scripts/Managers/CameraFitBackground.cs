using UnityEngine;

/// <summary>
/// Attach to the Background GameObject.
/// Automatically scales the sprite to fill the game camera exactly,
/// regardless of screen size or camera orthographic size.
/// </summary>
[ExecuteAlways]   // works in both Edit mode and Play mode
[RequireComponent(typeof(SpriteRenderer))]
public class CameraFitBackground : MonoBehaviour
{
    [Tooltip("Which camera to fit. Leave empty to use Camera.main.")]
    public Camera targetCamera;

    [Tooltip("Add extra margin so the background never shows gaps at edges (1 = exact fit).")]
    public float overScan = 1.05f;

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (targetCamera == null) targetCamera = Camera.main;
        Fit();
    }

    void Update()
    {
        // Re-fit every frame in Editor mode so it updates as you resize the camera
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            if (targetCamera == null) targetCamera = Camera.main;
            Fit();
        }
#endif
    }

    void Fit()
    {
        if (sr == null || sr.sprite == null || targetCamera == null) return;

        // World-space size of the camera viewport
        float camHeight = targetCamera.orthographicSize * 2f;
        float camWidth  = camHeight * targetCamera.aspect;

        // Size of the sprite in world units (before any scale)
        float spriteHeight = sr.sprite.bounds.size.y;
        float spriteWidth  = sr.sprite.bounds.size.x;

        if (spriteWidth  <= 0 || spriteHeight <= 0) return;

        // Scale so the sprite covers the full camera
        float scaleX = (camWidth  / spriteWidth)  * overScan;
        float scaleY = (camHeight / spriteHeight) * overScan;

        // Use the larger scale so the image always covers — never shows gaps
        float scale = Mathf.Max(scaleX, scaleY);

        transform.localScale = new Vector3(scale, scale, 1f);

        // Position at camera center, slightly behind everything
        Vector3 pos = targetCamera.transform.position;
        pos.z = transform.position.z;          // keep original Z for sorting
        transform.position = pos;
    }

    // Show a camera-shaped box in the Scene view
    void OnDrawGizmosSelected()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera == null) return;

        float h = targetCamera.orthographicSize * 2f;
        float w = h * targetCamera.aspect;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(targetCamera.transform.position, new Vector3(w, h, 0));
    }
}
