using System.Collections;
using UnityEngine;

public class PlayerControllerTopDown : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;

    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float invincibilityDuration = 1f;
    [SerializeField] private int blinkCount = 5;
    [SerializeField] private float blinkInterval = 0.1f;

    [Header("Attack Settings")]
    [SerializeField] private float primaryAttackCooldown = 0.5f;
    [SerializeField] private float secondaryAttackCooldown = 0.8f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float secondaryAttackDamage = 20f;

    [Header("Collectibles")]
    [SerializeField] private string coinTag = "Coin";

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private Vector2 inputDirection;
    private Vector2 currentVelocity;
    private Vector2 lastMoveDirection = Vector2.down; // Default facing down

    // Attack state
    private float primaryAttackTimer = 0f;
    private float secondaryAttackTimer = 0f;
    private bool isAttacking = false;

    // Collectibles tracking
    private int coinsCollected = 0;

    // Health state
    private float currentHealth;
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;
    private bool isDead = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (isDead) return;

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

        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        // Apply movement using Rigidbody2D - direct velocity for snappy movement
        rb.linearVelocity = currentVelocity;
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
            coinsCollected++;
            Debug.Log($"Coin collected! Total coins: {coinsCollected}");
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

    /// <summary>
    /// Deal damage to the player
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (isDead || isInvincible) return;

        currentHealth -= damage;
        Debug.Log($"Player took {damage} damage! Current HP: {currentHealth}/{maxHealth}");

        // Start invincibility
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;

        // Visual feedback - blink sprite
        StartCoroutine(BlinkCoroutine());

        if (animator != null)
        {
            animator.SetTrigger("Hurt");
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        currentHealth = 0f;
        rb.linearVelocity = Vector2.zero;

        // Stop blinking and ensure sprite is visible
        StopAllCoroutines();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        Debug.Log("Player died!");

        if (animator != null)
        {
            animator.SetTrigger("Die");
            animator.SetBool("IsDead", true);
        }
    }

    private IEnumerator BlinkCoroutine()
    {
        if (spriteRenderer == null) yield break;

        for (int i = 0; i < blinkCount; i++)
        {
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(blinkInterval);
            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(blinkInterval);
        }

        // Ensure sprite is visible and invincibility ends
        spriteRenderer.enabled = true;
        isInvincible = false;
    }

    /// <summary>
    /// Returns current health
    /// </summary>
    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    /// <summary>
    /// Returns max health
    /// </summary>
    public float GetMaxHealth()
    {
        return maxHealth;
    }

    /// <summary>
    /// Returns whether the player is dead
    /// </summary>
    public bool IsDead()
    {
        return isDead;
    }
}