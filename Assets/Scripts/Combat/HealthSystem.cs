using UnityEngine;
using UnityEngine.Events;

public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 500f;  // Long epic battles!
    public float currentHealth;

    [Header("Events")]
    public UnityEvent onDeath = new UnityEvent();
    public UnityEvent<float> onHealthChanged = new UnityEvent<float>(); // passes normalized health (0-1)

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        onHealthChanged?.Invoke(1f);
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        onHealthChanged?.Invoke(currentHealth / maxHealth);

        // Notify vignette low-health pulse
        if (CompareTag("Player"))
            ScreenVignette.NotifyHealthChange(currentHealth / maxHealth);

        // Screen shake on player hit
        if (CompareTag("Player"))
            CameraShake.Instance?.Shake(0.15f, 0.12f);

        // Trigger hurt animation if this is an enemy
        EnemyAI enemy = GetComponent<EnemyAI>();
        if (enemy != null) enemy.OnHurt();

        if (currentHealth <= 0) Die();
    }

    public void Heal(float amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        onHealthChanged?.Invoke(currentHealth / maxHealth);
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        onDeath?.Invoke();

        Animator anim = GetComponent<Animator>();
        if (anim != null) anim.SetTrigger("Dead");

        // If it's the player who died — freeze controls
        if (CompareTag("Player"))
        {
            // Stop movement immediately
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null) { rb.velocity = Vector2.zero; rb.isKinematic = true; }

            PlayerMovement pm = GetComponent<PlayerMovement>();
            if (pm != null) pm.enabled = false;

            GameManager.Instance?.PlayerDied();
        }
    }

    public bool IsDead() => isDead;
    public float GetHealthPercent() => currentHealth / maxHealth;
}
