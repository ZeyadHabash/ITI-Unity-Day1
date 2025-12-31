using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 10f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.1f;

    [Header("Ground Check")]
    [SerializeField] private string groundTag = "Ground";

    [Header("Collectibles")]
    [SerializeField] private string coinTag = "Coin";

    [Header("Primary Attack Settings")]
    [SerializeField] private float primaryAttackCooldown = 0.5f;
    [SerializeField] private KeyCode primaryAttackKey1 = KeyCode.Z;
    [SerializeField] private KeyCode primaryAttackKey2 = KeyCode.J;

    [Header("Secondary Attack Settings")]
    [SerializeField] private float secondaryAttackCooldown = 0.8f;
    [SerializeField] private KeyCode secondaryAttackKey1 = KeyCode.X;
    [SerializeField] private KeyCode secondaryAttackKey2 = KeyCode.K;

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private KeyCode dashKey = KeyCode.LeftShift;

    [Header("Long Idle Settings")]
    [SerializeField] private float longIdleTime = 30f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private float horizontalInput;
    private float currentHorizontalVelocity;
    private bool isFacingRight = true;
    private bool isGrounded;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    private bool isPrimaryAttacking;
    private bool isSecondaryAttacking;
    private float primaryAttackCooldownTimer;
    private float secondaryAttackCooldownTimer;

    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;
    private float dashDirection;

    private bool isDead;

    private float idleTimer;
    private bool isLongIdle;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (isDead) return;

        HandleAttack();
        HandleDash();

        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
            coyoteTimeCounter = 0f;
        }

        Flip();
        UpdateAnimations();
        UpdateCooldowns();
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        if (isSecondaryAttacking)
        {
            // Keep current velocity during secondary attack (gravity handled by Rigidbody2D)
            return;
        }

        if (isDashing)
        {
            rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);

            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0f)
            {
                isDashing = false;
            }
            return;
        }

        // Handle jump in FixedUpdate for physics consistency
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
        }

        // Calculate horizontal movement with acceleration/deceleration
        float targetVelocity = horizontalInput * moveSpeed;
        float accelerationRate = (Mathf.Abs(targetVelocity) > 0.01f) ? acceleration : deceleration;
        currentHorizontalVelocity = Mathf.MoveTowards(currentHorizontalVelocity, targetVelocity, accelerationRate * Time.fixedDeltaTime);

        // Apply movement using Rigidbody2D
        rb.linearVelocity = new Vector2(currentHorizontalVelocity, rb.linearVelocity.y);
    }

    private void HandleAttack()
    {
        if ((Input.GetKeyDown(primaryAttackKey1) || Input.GetKeyDown(primaryAttackKey2))
            && primaryAttackCooldownTimer <= 0f && !isPrimaryAttacking && !isSecondaryAttacking)
        {
            isPrimaryAttacking = true;
            primaryAttackCooldownTimer = primaryAttackCooldown;

            if (animator != null)
            {
                animator.SetTrigger("PrimaryAttack");
            }

            Debug.Log("Player used primary attack!");
        }

        if ((Input.GetKeyDown(secondaryAttackKey1) || Input.GetKeyDown(secondaryAttackKey2))
            && secondaryAttackCooldownTimer <= 0f && !isPrimaryAttacking && !isSecondaryAttacking)
        {
            isSecondaryAttacking = true;
            secondaryAttackCooldownTimer = secondaryAttackCooldown;

            if (animator != null)
            {
                animator.SetTrigger("LongAttack");
            }

            Debug.Log("Player used secondary attack!");
        }
    }

    private void HandleDash()
    {
        if (Input.GetKeyDown(dashKey) && dashCooldownTimer <= 0f && !isDashing)
        {
            isDashing = true;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;

            if (Mathf.Abs(horizontalInput) > 0.01f)
            {
                dashDirection = Mathf.Sign(horizontalInput);
            }
            else
            {
                dashDirection = isFacingRight ? 1f : -1f;
            }

            if (animator != null)
            {
                animator.SetTrigger("Dash");
            }

            Debug.Log("Player dashed!");
        }
    }

    private void UpdateCooldowns()
    {
        if (primaryAttackCooldownTimer > 0f)
        {
            primaryAttackCooldownTimer -= Time.deltaTime;
        }

        if (primaryAttackCooldownTimer <= 0f && isPrimaryAttacking)
        {
            isPrimaryAttacking = false;
        }

        if (secondaryAttackCooldownTimer > 0f)
        {
            secondaryAttackCooldownTimer -= Time.deltaTime;
        }

        if (secondaryAttackCooldownTimer <= 0f && isSecondaryAttacking)
        {
            isSecondaryAttacking = false;
        }

        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
    }



    private void Flip()
    {
        // Flip the sprite based on movement direction
        if (horizontalInput > 0f && !isFacingRight)
        {
            isFacingRight = true;
            spriteRenderer.flipX = false;
        }
        else if (horizontalInput < 0f && isFacingRight)
        {
            isFacingRight = false;
            spriteRenderer.flipX = true;
        }
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        animator.SetFloat("Speed", Mathf.Abs(horizontalInput));
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsDashing", isDashing);
        animator.SetBool("IsPrimaryAttacking", isPrimaryAttacking);
        animator.SetBool("IsSecondaryAttacking", isSecondaryAttacking);
        animator.SetBool("IsDead", isDead);

        bool isJumping = !isGrounded && rb.linearVelocity.y > 0f;
        bool isFalling = !isGrounded && rb.linearVelocity.y < 0f;
        animator.SetBool("IsJumping", isJumping);
        animator.SetBool("IsFalling", isFalling);

        bool isStopping = isGrounded && Mathf.Abs(horizontalInput) < 0.01f && Mathf.Abs(currentHorizontalVelocity) > 0.1f;
        animator.SetBool("IsStopping", isStopping);

        bool isIdle = isGrounded && Mathf.Abs(horizontalInput) < 0.01f && Mathf.Abs(currentHorizontalVelocity) < 0.1f
                      && !isPrimaryAttacking && !isSecondaryAttacking && !isDashing;

        if (isIdle)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= longIdleTime && !isLongIdle)
            {
                isLongIdle = true;
                animator.SetTrigger("LongIdle");
            }
        }
        else
        {
            idleTimer = 0f;
            isLongIdle = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(groundTag))
        {
            // Check if we're hitting from above (standing on ground) vs from the side
            foreach (ContactPoint2D contact in collision.contacts)
            {
                // Normal pointing up means we're standing on top
                if (contact.normal.y > 0.5f)
                {
                    isGrounded = true;
                    return;
                }
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(groundTag))
        {
            // Continuously check ground contact while colliding
            bool foundGround = false;
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    foundGround = true;
                    break;
                }
            }
            isGrounded = foundGround;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(coinTag))
        {
            // increment score
            Destroy(other.gameObject);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(groundTag))
        {
            isGrounded = false;
        }
    }
}