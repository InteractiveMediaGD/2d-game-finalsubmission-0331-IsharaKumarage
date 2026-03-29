using System.Collections;
using UnityEngine;

/// <summary>
/// Screen shake effect triggered when the player takes damage.
/// Place on a GameObject in the scene (e.g. GameManager or Camera).
/// Call: CameraShake.Instance.Shake(duration, magnitude)
/// </summary>
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    private Camera mainCam;
    private Vector3 originalCamPos;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        mainCam = Camera.main;
        if (mainCam != null) originalCamPos = mainCam.transform.localPosition;
    }

    /// <summary>Shake the camera for `duration` seconds with the given `magnitude`.</summary>
    public void Shake(float duration = 0.15f, float magnitude = 0.12f)
    {
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam != null) StartCoroutine(DoShake(duration, magnitude));
    }

    IEnumerator DoShake(float duration, float magnitude)
    {
        Vector3 origin = mainCam.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            mainCam.transform.localPosition = new Vector3(origin.x + x, origin.y + y, origin.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCam.transform.localPosition = origin;
    }
}
