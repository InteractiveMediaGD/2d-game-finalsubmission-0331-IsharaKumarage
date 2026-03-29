using UnityEngine;

/// <summary>
/// Shuriken / projectile fired by the player.
/// Moves in the launch direction, destroys itself on any collision,
/// and deals damage to enemies it hits.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [Header("Settings")]
    public float speed  = 14f;
    public float damage = 25f;
    public float maxLifetime = 4f;   // auto-destroy if nothing hit

    private Vector2 direction;

    public void Launch(Vector2 dir)
    {
        direction = dir.normalized;
        Destroy(gameObject, maxLifetime);
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Ignore the player who fired it
        if (other.CompareTag("Player")) return;

        // Damage enemy if hit
        if (other.CompareTag("Enemy"))
        {
            CameraShake.Instance?.Shake(0.1f, 0.05f);
            EffectsManager.Instance?.PlayProjectileHit(transform.position); // 💨 Cyan burst

            HealthSystem hs = other.GetComponent<HealthSystem>();
            if (hs != null && !hs.IsDead())
            {
                bool killed = hs.currentHealth <= damage;
                hs.TakeDamage(damage);
                if (killed)
                {
                    EffectsManager.Instance?.PlayDeath(other.transform.position); // 💥
                    GameManager.Instance?.AddScore(10);
                }
            }
        }

        // Destroy projectile on contact with anything solid
        Destroy(gameObject);
    }
}
