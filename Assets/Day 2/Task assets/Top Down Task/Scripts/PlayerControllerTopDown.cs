using UnityEngine;

public class PlayerControllerTopDown : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;

    [Header("Collision Settings")]
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float collisionBuffer = 0.1f;

    [Header("Attack Settings")]
    [SerializeField] private float primaryAttackCooldown = 0.5f;
    [SerializeField] private float secondaryAttackCooldown = 0.8f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float secondaryAttackDamage = 20f;

    [Header("Collectibles")]
    [SerializeField] private string coinTag = "Coin";

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private BoxCollider2D boxCollider;

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
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        if (primaryAttackTimer > 0f)
            primaryAttackTimer -= Time.deltaTime;
        if (secondaryAttackTimer > 0f)
            secondaryAttackTimer -= Time.deltaTime;

        HandleAttackInput();

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        inputDirection = new Vector2(horizontal, vertical);

        if (inputDirection.magnitude > 1f)
        {
            inputDirection.Normalize();
        }

        if (inputDirection.sqrMagnitude > 0.01f)
        {
            lastMoveDirection = inputDirection.normalized;
        }

        currentVelocity = inputDirection * moveSpeed;

        Move(currentVelocity * Time.deltaTime);

        UpdateAnimations();
    }

    private void Move(Vector2 movement)
    {

        if (movement.x != 0f)
        {
            float directionX = Mathf.Sign(movement.x);
            float distance = Mathf.Abs(movement.x) + collisionBuffer;

            RaycastHit2D hit = Physics2D.BoxCast(
                boxCollider.bounds.center,
                boxCollider.bounds.size,
                0f,
                new Vector2(directionX, 0f),
                distance,
                wallLayer
            );

            if (hit.collider != null)
            {
                movement.x = (hit.distance - collisionBuffer) * directionX;
            }
        }

        if (movement.y != 0f)
        {
            float directionY = Mathf.Sign(movement.y);
            float distance = Mathf.Abs(movement.y) + collisionBuffer;

            RaycastHit2D hit = Physics2D.BoxCast(
                boxCollider.bounds.center,
                boxCollider.bounds.size,
                0f,
                new Vector2(0f, directionY),
                distance,
                wallLayer
            );

            if (hit.collider != null)
            {
                movement.y = (hit.distance - collisionBuffer) * directionY;
            }
        }

        transform.Translate(movement);
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        animator.SetFloat("Speed", inputDirection.magnitude);
        animator.SetFloat("LastHorizontal", lastMoveDirection.x);
        animator.SetFloat("LastVertical", lastMoveDirection.y);
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