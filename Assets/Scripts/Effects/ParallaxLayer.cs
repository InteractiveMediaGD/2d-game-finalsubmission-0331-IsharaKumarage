using UnityEngine;

/// <summary>
/// Attach to each background layer. Moves at a fraction of the camera speed
/// to create a depth/parallax illusion.
/// </summary>
public class ParallaxLayer : MonoBehaviour
{
    [Range(0f, 1f)]
    [Tooltip("0 = moves with camera (foreground). 1 = stays still (far background).")]
    public float parallaxFactor = 0.5f;

    private Transform cam;
    private Vector3 lastCamPos;

    void Start()
    {
        cam = Camera.main?.transform;
        if (cam != null) lastCamPos = cam.position;
    }

    void LateUpdate()
    {
        if (cam == null) return;
        Vector3 delta = cam.position - lastCamPos;
        transform.position += new Vector3(delta.x * (1f - parallaxFactor), 0, 0);
        lastCamPos = cam.position;
    }
}
