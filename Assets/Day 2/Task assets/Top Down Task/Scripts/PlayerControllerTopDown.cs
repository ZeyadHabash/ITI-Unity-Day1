using UnityEngine;

public class PlayerControllerTopDown : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 10f;

    [Header("Attack Settings")]
    [SerializeField] private float primaryAttackCooldown = 0.5f;
    [SerializeField] private float secondaryAttackCooldown = 0.8f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float secondaryAttackDamage = 20f;

    [Header("Collectibles")]
    [SerializeField] private string coinTag = "Coin";

    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private Vector2 inputDirection;
    private Vector2 currentVelocity;
    private bool isFacingRight = true;
    private Vector2 lastMoveDirection;

    // Attack state
    private float primaryAttackTimer = 0f;
    private float secondaryAttackTimer = 0f;
    private bool isAttacking = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        // Update attack cooldown timers
        if (primaryAttackTimer > 0f)
            primaryAttackTimer -= Time.deltaTime;
        if (secondaryAttackTimer > 0f)
            secondaryAttackTimer -= Time.deltaTime;

        // Handle attack input (Z or J for primary, X or K for secondary)
        HandleAttackInput();

        // Get input for both axes (top-down movement)
        inputDirection.x = Input.GetAxisRaw("Horizontal");
        inputDirection.y = Input.GetAxisRaw("Vertical");

        // Normalize diagonal movement to prevent faster diagonal speed
        if (inputDirection.magnitude > 1f)
        {
            inputDirection.Normalize();
        }

        // Store last move direction for animation purposes
        if (inputDirection.magnitude > 0.1f)
        {
            lastMoveDirection = inputDirection.normalized;
        }

        // Calculate movement with acceleration/deceleration
        Vector2 targetVelocity = inputDirection * moveSpeed;
        float accelerationRate = (inputDirection.magnitude > 0.01f) ? acceleration : deceleration;
        currentVelocity = Vector2.MoveTowards(currentVelocity, targetVelocity, accelerationRate * Time.deltaTime);

        // Apply movement using Transform
        Vector3 movement = new Vector3(currentVelocity.x, currentVelocity.y, 0f) * Time.deltaTime;
        transform.Translate(movement);

        Flip();
        UpdateAnimations();
    }

    private void Flip()
    {
        // Flip the sprite based on horizontal movement direction
        if (inputDirection.x > 0f && !isFacingRight)
        {
            isFacingRight = true;
            spriteRenderer.flipX = false;
        }
        else if (inputDirection.x < 0f && isFacingRight)
        {
            isFacingRight = false;
            spriteRenderer.flipX = true;
        }
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        // Set speed based on movement magnitude (works for all directions)
        animator.SetFloat("Speed", currentVelocity.magnitude / moveSpeed);

        // Set directional parameters for blend trees (if using directional animations)
        animator.SetFloat("Horizontal", lastMoveDirection.x);
        animator.SetFloat("Vertical", lastMoveDirection.y);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(coinTag))
        {
            // increment score
            Destroy(other.gameObject);
        }
    }

    /// <summary>
    /// Returns the current velocity of the player (useful for camera look-ahead)
    /// </summary>
    public Vector2 GetVelocity()
    {
        return currentVelocity;
    }

    private void HandleAttackInput()
    {
        // Primary Attack: Z or J
        if ((Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.J)) && primaryAttackTimer <= 0f)
        {
            PrimaryAttack();
        }

        // Secondary Attack: X or K
        if ((Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.K)) && secondaryAttackTimer <= 0f)
        {
            SecondaryAttack();
        }
    }

    private void PrimaryAttack()
    {
        primaryAttackTimer = primaryAttackCooldown;
        isAttacking = true;

        // Trigger animation
        if (animator != null)
        {
            animator.SetTrigger("PrimaryAttack");
        }

        // TODO: Add attack logic here (e.g., raycast, hitbox detection, spawn projectile)
        Debug.Log("Primary Attack!");

        // Reset attacking state after a short delay (can be handled by animation event instead)
        Invoke(nameof(ResetAttackState), 0.2f);
    }

    private void SecondaryAttack()
    {
        secondaryAttackTimer = secondaryAttackCooldown;
        isAttacking = true;

        // Trigger animation
        if (animator != null)
        {
            animator.SetTrigger("SecondaryAttack");
        }

        // TODO: Add attack logic here (e.g., heavy attack, area damage, special ability)
        Debug.Log("Secondary Attack!");

        // Reset attacking state after a short delay (can be handled by animation event instead)
        Invoke(nameof(ResetAttackState), 0.3f);
    }

    private void ResetAttackState()
    {
        isAttacking = false;
    }

    public bool IsAttacking()
    {
        return isAttacking;
    }
}