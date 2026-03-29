using UnityEngine;

/// <summary>
/// Fires a projectile (Shuriken) on Left Mouse Button.
/// Attach to the Player GameObject alongside PlayerMovement.
/// </summary>
public class ProjectileLauncher : MonoBehaviour
{
    [Header("Projectile")]
    public GameObject projectilePrefab;   // assign the Shuriken prefab in Inspector
    public Transform  firePoint;          // spawn position (e.g. AttackPoint child)
    public float      cooldown = 0.4f;

    private float lastFireTime = -999f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            TryFire();
    }

    void TryFire()
    {
        if (Time.time - lastFireTime < cooldown) return;
        if (projectilePrefab == null) { Debug.LogWarning("[Launcher] Assign projectilePrefab!"); return; }

        lastFireTime = Time.time;

        // Requirement 4: The projectile is created at the player/mouse position
        // Determine direction towards mouse
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0; // ensure 2D

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        Vector2 direction = ((Vector2)mousePos - (Vector2)spawnPos).normalized;

        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        // Rotate projectile to face flight direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        proj.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        Projectile p = proj.GetComponent<Projectile>();
        if (p != null) p.Launch(direction);
    }
}
