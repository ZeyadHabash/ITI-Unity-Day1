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
    private Rigidbody2D rb;

    private Vector2 inputDirection;
    private Vector2 currentVelocity;
    private Vector2 lastMoveDirection = Vector2.down; // Default facing down

    // Attack state
    private float primaryAttackTimer = 0f;
    private float secondaryAttackTimer = 0f;
    private bool isAttacking = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // Ensure Rigidbody2D is set up correctly for top-down
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }
    }

    private void Update()
    {
        if (primaryAttackTimer > 0f)
            primaryAttackTimer -= Time.deltaTime;
        if (secondaryAttackTimer > 0f)
            secondaryAttackTimer -= Time.deltaTime;

        HandleAttackInput();

        inputDirection.x = Input.GetAxisRaw("Horizontal");
        inputDirection.y = Input.GetAxisRaw("Vertical");

        if (inputDirection.magnitude > 1f)
        {
            inputDirection.Normalize();
        }

        if (inputDirection.magnitude > 0.1f)
        {
            lastMoveDirection = inputDirection.normalized;
        }

        // Calculate target velocity with acceleration/deceleration
        Vector2 targetVelocity = inputDirection * moveSpeed;
        float accelerationRate = (inputDirection.magnitude > 0.01f) ? acceleration : deceleration;
        currentVelocity = Vector2.MoveTowards(currentVelocity, targetVelocity, accelerationRate * Time.deltaTime);

        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        // Use Rigidbody2D for physics-based movement (respects colliders)
        if (rb != null)
        {
            rb.MovePosition(rb.position + currentVelocity * Time.fixedDeltaTime);
        }
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        // Set speed for blend tree or transition conditions
        animator.SetFloat("Speed", currentVelocity.magnitude / moveSpeed);

        // Set direction values for 4-directional animations
        animator.SetFloat("Horizontal", lastMoveDirection.x);
        animator.SetFloat("Vertical", lastMoveDirection.y);

        // Set the dominant direction for state machine transitions
        // This helps with 4-directional sprite selection
        animator.SetFloat("LastHorizontal", lastMoveDirection.x);
        animator.SetFloat("LastVertical", lastMoveDirection.y);

        // Boolean states for animation transitions
        animator.SetBool("IsMoving", currentVelocity.magnitude > 0.1f);
        animator.SetBool("IsAttacking", isAttacking);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(coinTag))
        {
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

    /// <summary>
    /// Returns the last move direction (useful for attack direction)
    /// </summary>
    public Vector2 GetFacingDirection()
    {
        return lastMoveDirection;
    }

    private void HandleAttackInput()
    {
        if ((Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.J)) && primaryAttackTimer <= 0f && !isAttacking)
        {
            PrimaryAttack();
        }

        if ((Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.K)) && secondaryAttackTimer <= 0f && !isAttacking)
        {
            SecondaryAttack();
        }
    }

    private void PrimaryAttack()
    {
        primaryAttackTimer = primaryAttackCooldown;
        isAttacking = true;

        if (animator != null)
        {
            animator.SetTrigger("PrimaryAttack");
        }

        Debug.Log("Primary Attack!");

        Invoke(nameof(ResetAttackState), 0.2f);
    }

    private void SecondaryAttack()
    {
        secondaryAttackTimer = secondaryAttackCooldown;
        isAttacking = true;

        if (animator != null)
        {
            animator.SetTrigger("SecondaryAttack");
        }

        Debug.Log("Secondary Attack!");

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