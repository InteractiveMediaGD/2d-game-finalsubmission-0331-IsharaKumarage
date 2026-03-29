using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyState { Idle, Chasing, Attacking, Hurt, Dead }

    [Header("Settings")]
    public float moveSpeed = 3f;
    public float attackRange = 2.5f; // Increased so animation plays before collision death
    public float detectionRange = 8f;
    public float attackCooldown = 1.5f;
    public float attackDamage = 50f;


    [Header("Attack Point")]
    public Transform attackPoint;
    public SwordHitDetector attackDetector;
    public LayerMask playerLayer;

    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;
    private HealthSystem health;
    private EnemyState currentState = EnemyState.Idle;
    private float lastAttackTime;
    private Vector3 initialScale;
    private float hurtTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        health = GetComponent<HealthSystem>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        if (attackPoint != null && attackDetector == null)
            attackDetector = attackPoint.GetComponent<SwordHitDetector>();

        initialScale = transform.localScale;
    }

    void Update()
    {
        // Destroy if player passed it (Requirement 5)
        // Increased distance to 15f to prevent premature disappearance
        if (player != null && player.position.x > transform.position.x + 15f)
        {
            Destroy(gameObject);
            return;
        }
        if (currentState == EnemyState.Dead) return;
        if (health != null && health.IsDead())
        {
            Die();
            return;
        }

        if (hurtTimer > 0)
        {
            hurtTimer -= Time.deltaTime;
            rb.velocity = new Vector2(Mathf.Lerp(rb.velocity.x, 0, Time.deltaTime * 5f), rb.velocity.y);
            return;
        }

        if (player == null) return;

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        if (distToPlayer <= attackRange)
            currentState = EnemyState.Attacking;
        else if (distToPlayer <= detectionRange)
            currentState = EnemyState.Chasing;
        else
            currentState = EnemyState.Idle;

        switch (currentState)
        {
            case EnemyState.Chasing: Chase(); break;
            case EnemyState.Attacking: Attack(); break;
            case EnemyState.Idle: Idle(); break;
        }
    }

    void Idle()
    {
        rb.velocity = new Vector2(0, rb.velocity.y);
        if (animator != null) animator.SetFloat("Speed", 0f);
    }

    void Chase()
    {
        float dir = player.position.x > transform.position.x ? 1f : -1f;
        rb.velocity = new Vector2(dir * moveSpeed, rb.velocity.y);

        float scaleX = Mathf.Abs(initialScale.x);
        transform.localScale = new Vector3(dir > 0 ? scaleX : -scaleX, initialScale.y, initialScale.z); 
        if (animator != null) animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
    }

    void Attack()
    {
        rb.velocity = new Vector2(0, rb.velocity.y);

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            lastAttackTime = Time.time;
            
            // Randomly pick an attack type for the enemy (from all 6 types)
            PlayerCombat.AttackType type = (PlayerCombat.AttackType)Random.Range(0, 6);
            
            if (animator != null) animator.SetTrigger("Attack");

            AudioManager.Instance?.PlaySwordSwing();
            StartCoroutine(CodeAttackAnimation(type));

            // Always apply fallback Overlap damage to ensure hit lands
            Vector3 point = attackPoint != null ? attackPoint.position : transform.position + new Vector3(transform.localScale.x > 0 ? 1f : -1f, 0, 0);
            Collider2D hit = Physics2D.OverlapCircle(point, attackRange, playerLayer);
            if (hit != null && hit.CompareTag("Player") && !hit.isTrigger)
            {
                HealthSystem playerHealth = hit.GetComponent<HealthSystem>();
                PlayerMovement pm = hit.GetComponent<PlayerMovement>();
                if (playerHealth != null && (pm == null || !pm.IsBlocking()))
                    playerHealth.TakeDamage(attackDamage);
            }

            if (attackDetector != null)
            {
                attackDetector.EnableHit();
                Invoke(nameof(DisableEnemyHit), 0.25f);
            }
        }
    }

    void DisableEnemyHit() { if (attackDetector != null) attackDetector.DisableHit(); }

    public void OnHurt()
    {
        currentState = EnemyState.Hurt;
        hurtTimer = 0.4f;
        if (animator != null) animator.SetTrigger("Hurt");

        if (player != null)
        {
            float dir = player.position.x > transform.position.x ? -1f : 1f;
            rb.velocity = new Vector2(dir * 6f, rb.velocity.y);
        }
    }

    void Die()
    {
        currentState = EnemyState.Dead;
        if (animator != null) animator.SetTrigger("Dead");
        rb.velocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;
        
        // Visual Polish: Death explosion!
        EffectsManager.Instance?.PlayDeath(transform.position);

        if (GameManager.Instance != null) GameManager.Instance.EnemyDefeated();
        Destroy(gameObject, 2f);
    }

    // ── Procedural Enemy Attack Animation (Smooth Lerp, 6 types) ──────────────────
    System.Collections.IEnumerator CodeAttackAnimation(PlayerCombat.AttackType type)
    {
        Transform pivot = transform;
        Transform torso = null;
        var skinManager = GetComponent<CharacterSkinManager>();

        if (skinManager != null)
        {
            if (skinManager.weaponPivot != null) pivot = skinManager.weaponPivot;
            if (skinManager.torso != null) torso = skinManager.torso.transform;
        }

        if (pivot == transform)
        {
            pivot = transform.Find("Sword") ?? transform.Find("Weapon") ?? transform.Find("R_Arm") ?? transform.Find("RightArm");
            if (pivot == null)
                foreach (Transform child in transform) { if (child.name.Contains("Sword")) { pivot = child; break; } }
        }
        if (pivot == null) pivot = transform;

        Quaternion restRot   = pivot.localRotation;
        Vector3    restPos   = pivot.localPosition;
        Quaternion torsoRest = torso != null ? torso.localRotation : Quaternion.identity;

        Quaternion windupRot  = restRot;
        Quaternion strikeRot  = restRot;
        Vector3    windupPos  = restPos;
        Vector3    strikePos  = restPos;
        float      windupTime = 0.08f;
        float      strikeTime = 0.07f;

        switch (type)
        {
            case PlayerCombat.AttackType.Slash:
                windupRot = restRot * Quaternion.Euler(0, 0, 55f);
                strikeRot = restRot * Quaternion.Euler(0, 0, -100f);
                windupTime = 0.07f; strikeTime = 0.07f; break;

            case PlayerCombat.AttackType.Chop:
                windupRot = restRot * Quaternion.Euler(0, 0, 100f);
                strikeRot = restRot * Quaternion.Euler(0, 0, -50f);
                windupTime = 0.08f; strikeTime = 0.07f; break;

            case PlayerCombat.AttackType.Stab:
                windupPos = restPos + new Vector3(-0.2f, -0.05f, 0);
                strikePos = restPos + new Vector3(1.4f,  -0.05f, 0);
                windupTime = 0.08f; strikeTime = 0.06f; break;

            case PlayerCombat.AttackType.Upper:
                windupRot = restRot * Quaternion.Euler(0, 0, -60f);
                strikeRot = restRot * Quaternion.Euler(0, 0, 135f);
                windupTime = 0.07f; strikeTime = 0.09f; break;

            case PlayerCombat.AttackType.Low:
                windupRot = restRot * Quaternion.Euler(0, 0, 55f);
                strikeRot = restRot * Quaternion.Euler(0, 0, -125f);
                windupTime = 0.07f; strikeTime = 0.08f; break;

            case PlayerCombat.AttackType.Power:
                windupPos = restPos + new Vector3(-0.4f, 0, 0);
                windupRot = restRot * Quaternion.Euler(0, 0, -30f);
                strikePos = restPos + new Vector3(0.4f, 0, 0);
                strikeRot = restRot * Quaternion.Euler(0, 0, -145f);
                windupTime = 0.10f; strikeTime = 0.08f; break;
        }

        // Lean torso into strike
        if (torso != null)
            torso.localRotation = torsoRest * Quaternion.Euler(0, 0, -12f);

        // WINDUP
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / windupTime;
            pivot.localRotation = Quaternion.Slerp(restRot, windupRot, Mathf.SmoothStep(0, 1, t));
            pivot.localPosition = Vector3.Lerp(restPos, windupPos, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }

        // STRIKE
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / strikeTime;
            pivot.localRotation = Quaternion.Slerp(windupRot, strikeRot, Mathf.SmoothStep(0, 1, t));
            pivot.localPosition = Vector3.Lerp(windupPos, strikePos, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }

        SpawnEnemySlashTrail(type);

        // RECOVER
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.20f;
            pivot.localRotation = Quaternion.Slerp(strikeRot, restRot, Mathf.SmoothStep(0, 1, t));
            pivot.localPosition = Vector3.Lerp(strikePos, restPos, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }

        pivot.localRotation = restRot;
        pivot.localPosition = restPos;
        if (torso != null) torso.localRotation = torsoRest;
    }

    void SpawnEnemySlashTrail(PlayerCombat.AttackType type)
    {
        GameObject vfx = new GameObject("EnemyCombatVFX");
        vfx.transform.SetParent(transform);
        vfx.transform.localPosition = new Vector3(1.2f, 0.2f, 0f);

        var sr = vfx.AddComponent<SpriteRenderer>();
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.red);
        tex.Apply();
        sr.sprite = Sprite.Create(tex, new Rect(0,0,1,1), new Vector2(0.5f, 0.5f));
        sr.color = new Color(1, 0.1f, 0.1f, 0.7f);
        sr.sortingOrder = 10;

        switch (type)
        {
            case PlayerCombat.AttackType.Slash:
                vfx.transform.localScale    = new Vector3(0.15f, 2.5f, 1);
                vfx.transform.localRotation = Quaternion.Euler(0, 0, 30); break;
            case PlayerCombat.AttackType.Chop:
                vfx.transform.localScale    = new Vector3(0.15f, 2.5f, 1);
                vfx.transform.localRotation = Quaternion.Euler(0, 0, 90); break;
            case PlayerCombat.AttackType.Upper:
                vfx.transform.localScale    = new Vector3(0.15f, 2.8f, 1);
                vfx.transform.localRotation = Quaternion.Euler(0, 0, 120);
                vfx.transform.localPosition = new Vector3(0.8f, 1.0f, 0); break;
            case PlayerCombat.AttackType.Low:
                vfx.transform.localScale    = new Vector3(0.15f, 2.8f, 1);
                vfx.transform.localRotation = Quaternion.Euler(0, 0, -30);
                vfx.transform.localPosition = new Vector3(0.8f, -0.7f, 0); break;
            case PlayerCombat.AttackType.Power:
                vfx.transform.localScale    = new Vector3(0.22f, 3.0f, 1);
                vfx.transform.localRotation = Quaternion.Euler(0, 0, 15);
                sr.color = new Color(1f, 0.4f, 0.1f, 0.8f); break; // orange for power
            case PlayerCombat.AttackType.Stab:
                vfx.transform.localScale    = new Vector3(1.6f, 0.12f, 1);
                vfx.transform.localPosition = new Vector3(1.8f, 0, 0); break;
        }

        Destroy(vfx, 0.12f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only die/damage if colliding with the player's ACTUAL body (not their sword triggger or detection radius)
        if (other.CompareTag("Player") && !other.isTrigger)
        {
            HealthSystem hs = other.GetComponent<HealthSystem>();
            if (hs != null) hs.TakeDamage(20f);
            
            // Removed instant Die() here so the enemy fights normally until health reaches 0!
        }
    }
}
