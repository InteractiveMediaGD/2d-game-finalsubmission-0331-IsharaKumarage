using UnityEngine;

/// <summary>
/// Player Movement — Boken Warrior
/// Controls: A/D = Move, Space = Jump, S = Block, J or LMB = Attack
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed    = 5f;
    public float maxMoveSpeed = 9f;   // speed cap
    public float speedGrowth  = 0.3f; // units per minute
    public float jumpForce    = 11f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Animator    animator;
    private PlayerCombat combat;

    private bool isGrounded;
    private bool isBlocking;
    private Vector3 initialScale;

    void Start()
    {
        rb      = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        combat   = GetComponent<PlayerCombat>();

        // Auto-find GroundCheck child if not assigned
        if (groundCheck == null)
        {
            var gc = transform.Find("GroundCheck");
            if (gc != null) groundCheck = gc;
        }

        // Auto-assign Ground layer if not set
        if (groundLayer.value == 0)
            groundLayer = LayerMask.GetMask("Ground");

        initialScale = transform.localScale;
    }

    void Update()
    {
        CheckGround();
        HandleMovement();
        HandleJump();
        HandleBlock();
        HandleAttack();
        UpdateAnimator();

        // Smoothly increase speed over time (roughly +1 unit every 30 seconds)
        float timeBonus = Time.timeSinceLevelLoad * 0.033f; 
        
        // Small score bonus to reward progress without breaking difficulty (+0.1 per 10 points)
        int currentScore = GameManager.Instance != null ? GameManager.Instance.score : 0;
        float scoreBonus = currentScore * 0.01f;
        
        float baseSpeed = 5f;
        moveSpeed = Mathf.Clamp(baseSpeed + timeBonus + scoreBonus, baseSpeed, maxMoveSpeed);
    }

    // ── Ground Detection ─────────────────────────────────────────────────
    void CheckGround()
    {
        if (groundCheck == null) { isGrounded = true; return; }
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position, groundCheckRadius, groundLayer);
    }

    // ── A / D Movement ───────────────────────────────────────────────────
    void HandleMovement()
    {
        float h = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) h = -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) h =  1f;

        if (isBlocking) h = 0f;   // can't walk while blocking

        rb.velocity = new Vector2(h * moveSpeed, rb.velocity.y);

        // Flip sprite to face direction
        if      (h > 0) transform.localScale = new Vector3( Mathf.Abs(initialScale.x), initialScale.y, initialScale.z);
        else if (h < 0) transform.localScale = new Vector3(-Mathf.Abs(initialScale.x), initialScale.y, initialScale.z);
    }

    // ── Spacebar Jump ────────────────────────────────────────────────────
    void HandleJump()
    {
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow)) && isGrounded && !isBlocking)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            if (animator != null) animator.SetTrigger("Jump");
        }
    }

    // ── S = Block ────────────────────────────────────────────────────────
    void HandleBlock()
    {
        isBlocking = Input.GetKey(KeyCode.S);
        if (animator != null) animator.SetBool("IsBlocking", isBlocking);
    }

    // ── J key only = Melee Attack ────────────────────────────────
    void HandleAttack()
    {
        if (combat == null) return;
        if (Input.GetKeyDown(KeyCode.J))
            combat.Slash();
    }

    // ── Update Animator Parameters ───────────────────────────────────────
    void UpdateAnimator()
    {
        if (animator == null) return;
        animator.SetFloat("Speed",      Mathf.Abs(rb.velocity.x));
        animator.SetBool ("IsGrounded", isGrounded);
    }

    public bool IsBlocking() => isBlocking;

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
