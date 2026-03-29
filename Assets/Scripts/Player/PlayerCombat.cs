using UnityEngine;

/// <summary>
/// Player Combat — Boken Warrior
/// Handles sword slash attacks triggered by J key or Left Mouse Button.
/// Works with both:
///   a) SwordHitDetector on child GameObject (tag "PlayerSword") — physics-based
///   b) OverlapCircle fallback for enemies without trigger colliders
/// </summary>
public class PlayerCombat : MonoBehaviour
{
    [Header("Damage")]
    public float slashDamage = 20f;
    public float attackCooldown = 0.4f;

    [Header("Overlap Fallback (if no sword child)")]
    public Transform attackPoint;
    public float attackRange = 1.4f;
    public LayerMask enemyLayer;

    [Header("Sword child (optional — has SwordHitDetector)")]
    public SwordHitDetector swordDetector;

    // Legacy property names (used by wizard serialization)
    public float punchDamage { get => slashDamage; set => slashDamage = value; }
    public float kickDamage  { get => slashDamage; set => slashDamage = value; }

    private Animator animator;
    private float lastAttackTime = -999f;
    private int comboStep = 0;
    private float lastComboClickTime = 0f;
    private float comboResetTime = 0.8f;

    public enum AttackType { Slash, Chop, Stab, Upper, Low, Power }

    void Start()
    {
        animator = GetComponent<Animator>();

        // Auto-find SwordHitDetector on any child
        if (swordDetector == null)
        {
            swordDetector = GetComponentInChildren<SwordHitDetector>();

            // If it still doesn't exist, generate it dynamically!
            if (swordDetector == null)
            {
                GameObject swordHitbox = new GameObject("SwordHitbox");
                swordHitbox.transform.SetParent(transform);
                swordHitbox.transform.localPosition = new Vector3(0.9f, 0.1f, 0f); // Default swipe position

                BoxCollider2D col = swordHitbox.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = new Vector2(1.2f, 1.5f);

                try { swordHitbox.tag = "PlayerSword"; } catch { }

                swordDetector = swordHitbox.AddComponent<SwordHitDetector>();
                swordDetector.targetTag = "Enemy";
                swordDetector.damage = slashDamage;
            }
        }

        // Auto-assign enemy layer
        if (enemyLayer.value == 0)
            enemyLayer = LayerMask.GetMask("Enemy");

        // Auto-find AttackPoint child
        if (attackPoint == null)
        {
            var ap = transform.Find("AttackPoint");
            if (ap != null) attackPoint = ap;
        }
    }

    // ── Called by PlayerMovement (J) ───────────────────────────────
    public void Slash()
    {
        if (Time.time - lastComboClickTime > comboResetTime) comboStep = 0;
        if (Time.time - lastAttackTime < attackCooldown) return;

        lastAttackTime = Time.time;
        lastComboClickTime = Time.time;

        // Requirement 4 & UX Update: Face the mouse when attacking!
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float directionToMouse = mousePos.x - transform.position.x;
        if (directionToMouse > 0.1f) transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else if (directionToMouse < -0.1f) transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

        // Simple Combo Scaling
        AttackType currentType = (AttackType)(comboStep % 3);
        comboStep++;

        if (animator != null) animator.SetTrigger("Attack");
        AudioManager.Instance?.PlaySwordSwing();
        
        StartCoroutine(CodeAttackAnimation(currentType));

        // Always rely on OverlapDamage to ensure it hits! (Procedural hitboxes can miss)
        DealOverlapDamage(slashDamage);

        if (swordDetector != null)
        {
            swordDetector.EnableHit();
            Invoke(nameof(DisableSword), 0.3f);
        }
    }

    // ── Procedural Sword Animation (Smooth Lerp Version) ──────────────────
    System.Collections.IEnumerator CodeAttackAnimation(AttackType type)
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
            Transform weaponChild = transform.Find("Sword") ?? transform.Find("Weapon") ?? transform.Find("weapon") ?? transform.Find("R_Arm");
            if (weaponChild != null) pivot = weaponChild;
        }

        Quaternion restRot   = pivot.localRotation;
        Vector3    restPos   = pivot.localPosition;
        Quaternion torsoRest = torso != null ? torso.localRotation : Quaternion.identity;

        // ── define WINDUP → STRIKE → RECOVER angles per attack ──────────
        Quaternion windupRot   = restRot;
        Quaternion strikeRot   = restRot;
        Vector3    windupPos   = restPos;
        Vector3    strikePos   = restPos;
        float      windupTime  = 0.07f;
        float      strikeTime  = 0.07f;

        switch (type)
        {
            case AttackType.Upper:
                windupRot  = restRot * Quaternion.Euler(0, 0, -55f);
                strikeRot  = restRot * Quaternion.Euler(0, 0, 130f);
                windupTime = 0.07f; strikeTime = 0.09f; break;

            case AttackType.Low:
                windupRot  = restRot * Quaternion.Euler(0, 0, 55f);
                strikeRot  = restRot * Quaternion.Euler(0, 0, -125f);
                windupTime = 0.06f; strikeTime = 0.08f; break;

            case AttackType.Power:
                windupPos  = restPos + new Vector3(-0.4f, 0, 0);
                windupRot  = restRot * Quaternion.Euler(0, 0, -30f);
                strikePos  = restPos + new Vector3(0.5f, 0, 0);
                strikeRot  = restRot * Quaternion.Euler(0, 0, -140f);
                windupTime = 0.10f; strikeTime = 0.08f; break;

            case AttackType.Slash:
                windupRot  = restRot * Quaternion.Euler(0, 0, 50f);
                strikeRot  = restRot * Quaternion.Euler(0, 0, -95f);
                windupTime = 0.06f; strikeTime = 0.07f; break;

            case AttackType.Chop:
                windupRot  = restRot * Quaternion.Euler(0, 0, 100f);
                strikeRot  = restRot * Quaternion.Euler(0, 0, -50f);
                windupTime = 0.07f; strikeTime = 0.07f; break;

            case AttackType.Stab:
                windupPos  = restPos + new Vector3(-0.2f, -0.05f, 0);
                strikePos  = restPos + new Vector3(1.6f, -0.05f, 0);
                windupTime = 0.07f; strikeTime = 0.06f; break;
        }

        // Lean torso into strike
        if (torso != null)
            torso.localRotation = torsoRest * Quaternion.Euler(0, 0, -12f);

        // WINDUP — smoothly move to start of swing
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / windupTime;
            pivot.localRotation = Quaternion.Slerp(restRot, windupRot, Mathf.SmoothStep(0, 1, t));
            pivot.localPosition = Vector3.Lerp(restPos,  windupPos, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }

        // STRIKE — fast snap of swing
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / strikeTime;
            pivot.localRotation = Quaternion.Slerp(windupRot, strikeRot, Mathf.SmoothStep(0, 1, t));
            pivot.localPosition = Vector3.Lerp(windupPos,  strikePos, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }

        SpawnSlashTrail(type);

        // RECOVER — smoothly return to rest
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.18f;
            pivot.localRotation = Quaternion.Slerp(strikeRot, restRot, Mathf.SmoothStep(0, 1, t));
            pivot.localPosition = Vector3.Lerp(strikePos,  restPos, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }

        pivot.localRotation = restRot;
        pivot.localPosition = restPos;
        if (torso != null) torso.localRotation = torsoRest;
    }

    void SpawnSlashTrail(AttackType type)
    {
        GameObject vfx = new GameObject("CombatVFX");
        vfx.transform.SetParent(transform);
        vfx.transform.localPosition = new Vector3(1.3f, 0.4f, 0f);
        
        var sr = vfx.AddComponent<SpriteRenderer>();
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        sr.sprite = Sprite.Create(tex, new Rect(0,0,1,1), new Vector2(0.5f, 0.5f));
        sr.color = new Color(1, 1, 1, 0.65f);
        sr.sortingOrder = 12;

        switch (type)
        {
            case AttackType.Upper: 
                vfx.transform.localScale = new Vector3(0.18f, 3f, 1); 
                vfx.transform.localRotation = Quaternion.Euler(0, 0, 120); 
                vfx.transform.localPosition = new Vector3(0.8f, 1.2f, 0);
                break;
            case AttackType.Low: 
                vfx.transform.localScale = new Vector3(0.18f, 3f, 1); 
                vfx.transform.localRotation = Quaternion.Euler(0, 0, -30); 
                vfx.transform.localPosition = new Vector3(0.8f, -0.8f, 0);
                break;
            case AttackType.Power: 
                vfx.transform.localScale = new Vector3(0.25f, 3.5f, 1); 
                vfx.transform.localRotation = Quaternion.Euler(0, 0, 15); 
                sr.color = new Color(1, 0.9f, 0.5f, 0.8f); // Goldish power slash
                break;
            case AttackType.Slash: 
                vfx.transform.localScale = new Vector3(0.15f, 2.5f, 1); 
                vfx.transform.localRotation = Quaternion.Euler(0,0,30); 
                break;
            case AttackType.Chop: 
                vfx.transform.localScale = new Vector3(0.15f, 2.5f, 1); 
                vfx.transform.localRotation = Quaternion.Euler(0,0,80); 
                break;
            case AttackType.Stab: 
                vfx.transform.localScale = new Vector3(1.8f, 0.12f, 1); 
                vfx.transform.localPosition = new Vector3(2.0f, 0, 0); 
                break;
        }

        Destroy(vfx, 0.15f);
    }

    // Legacy methods so existing wizard serialization still works
    public void Punch() => Slash();
    public void Kick()  => Slash();

    void DisableSword() { if (swordDetector != null) swordDetector.DisableHit(); }

    void DealOverlapDamage(float damage)
    {
        if (attackPoint == null) return;
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            attackPoint.position, attackRange, enemyLayer);

        foreach (var h in hits)
        {
            var hs = h.GetComponent<HealthSystem>();
            if (hs != null && !hs.IsDead())
            {
                hs.TakeDamage(damage);
                var enemy = h.GetComponent<EnemyAI>();
                if (enemy != null) enemy.OnHurt();
                Debug.Log($"[Combat] Overlap hit '{h.name}' for {damage} dmg.");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = new Color(1, 0.3f, 0.3f, 0.4f);
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
