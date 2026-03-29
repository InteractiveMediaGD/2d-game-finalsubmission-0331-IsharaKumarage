using UnityEngine;

/// <summary>
/// Attach to the SwordHitbox child GameObject under Player.
/// The hitbox Collider2D is DISABLED by default.
/// PlayerCombat calls EnableHit() to activate it during an attack swing,
/// then DisableHit() after the hit window (0.25 s) to deactivate it.
///
///   Hierarchy:
///     Player
///       └─ SwordHitbox   ← this script + BoxCollider2D (Is Trigger ✅)
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class SwordHitDetector : MonoBehaviour
{
    [Tooltip("Damage per sword hit")]
    public float damage = 20f;
    [Tooltip("Tag of the target to hit")]
    public string targetTag = "Enemy";

    private Collider2D hitbox;

    void Awake()
    {
        hitbox = GetComponent<Collider2D>();
        hitbox.enabled = false;   // disabled until attack swing
    }

    /// <summary>Call at start of attack swing to open the hit window.</summary>
    public void EnableHit()  => hitbox.enabled = true;

    /// <summary>Call after hit window ends to close the hitbox.</summary>
    public void DisableHit() => hitbox.enabled = false;

    /// <summary>
    /// Fires when hitbox enters a target trigger.
    /// Deals damage and triggers the Hurt animation on the target.
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(targetTag)) return;

        HealthSystem hs = other.GetComponent<HealthSystem>();
        if (hs == null || hs.IsDead()) return;

        PlayerMovement pm = other.GetComponent<PlayerMovement>();
        if (pm != null && pm.IsBlocking()) return; // block successful

        // Reduce target health and check for kill
        bool wasKilled = hs.currentHealth <= damage;
        hs.TakeDamage(damage);

        // Creative Feature: Hit Stop, Screen Shake, and Sounds!
        if (targetTag == "Enemy")
        {
            CameraShake.Instance?.Shake(0.1f, 0.05f);
            EffectsManager.Instance?.PlayHitSparks(other.transform.position);
            GameManager.Instance?.AddScore(10);
            AudioManager.Instance?.PlaySwordHit();
            StartCoroutine(HitStop(0.05f));
        }

        // Award BONUS score if we killed the enemy
        if (wasKilled && targetTag == "Enemy")
        {
            EffectsManager.Instance?.PlayDeath(other.transform.position);
            GameManager.Instance?.AddScore(10);
            AudioManager.Instance?.PlayEnemyDeath();
        }

        // Trigger Hurt animation
        EnemyAI enemy = other.GetComponent<EnemyAI>();
        if (enemy != null) enemy.OnHurt();

        Animator anim = other.GetComponent<Animator>();
        if (anim != null && enemy == null) anim.SetTrigger("Hurt");

        // Flash enemy red for visual feedback
        StartCoroutine(FlashRed(other.GetComponent<SpriteRenderer>()));

        // Close hitbox immediately so one swing only hits once
        DisableHit();

        Debug.Log($"[SwordHitbox] Hit '{other.name}' for {damage} dmg!");
    }

    System.Collections.IEnumerator HitStop(float duration)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }

    System.Collections.IEnumerator FlashRed(SpriteRenderer sr)
    {
        if (sr == null) yield break;
        Color orig = sr.color;
        sr.color = Color.red;
        yield return new WaitForSecondsRealtime(0.1f);
        sr.color = orig;
    }

    // Show hitbox bounds in Scene view
    void OnDrawGizmosSelected()
    {
        var col = GetComponent<BoxCollider2D>();
        if (col == null) return;
        Gizmos.color = hitbox != null && hitbox.enabled
            ? new Color(1, 0, 0, 0.5f)
            : new Color(1, 0.5f, 0, 0.25f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(col.offset, col.size);
    }
}
