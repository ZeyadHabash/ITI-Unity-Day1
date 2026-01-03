using UnityEngine;
using UnityEngine.SceneManagement;

namespace Platformer
{
    /// <summary>
    /// Refactored PlayerController using modular components
    /// Attach Health, FlipController, GroundCheck, and ProjectileShooter components to the same GameObject
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float acceleration = 10f;
        [SerializeField] private float deceleration = 10f;

        [Header("Jump Settings")]
        [SerializeField] private float jumpForce = 12f;
        [SerializeField] private float jumpBufferTime = 0.1f;

        [Header("Collectibles")]
        [SerializeField] private string coinTag = "Coin";

        [Header("Water Settings")]
        [SerializeField] private string waterTag = "Water";

        [Header("Primary Attack Settings")]
        [SerializeField] private float primaryAttackCooldown = 0.5f;
        [SerializeField] private KeyCode primaryAttackKey1 = KeyCode.Z;
        [SerializeField] private KeyCode primaryAttackKey2 = KeyCode.J;

        [Header("Secondary Attack Settings")]
        [SerializeField] private float secondaryAttackCooldown = 1f;
        [SerializeField] private KeyCode secondaryAttackKey1 = KeyCode.X;
        [SerializeField] private KeyCode secondaryAttackKey2 = KeyCode.K;

        [Header("Dash Settings")]
        [SerializeField] private float dashSpeed = 20f;
        [SerializeField] private float dashDuration = 0.2f;
        [SerializeField] private float dashCooldown = 1f;
        [SerializeField] private KeyCode dashKey = KeyCode.LeftShift;

        [Header("Long Idle Settings")]
        [SerializeField] private float longIdleTime = 30f;

        [Header("Ladder Settings")]
        [SerializeField] private string ladderTag = "Ladder";
        [SerializeField] private float climbSpeed = 8f;

        // Component references
        private Rigidbody2D rb;
        private Animator animator;
        private Health health;
        private FlipController flipController;
        private GroundCheck groundCheck;
        private ProjectileShooter projectileShooter;

        // Movement state
        private float horizontalInput;
        private float currentHorizontalVelocity;
        private float jumpBufferCounter;

        // Attack state
        private bool isPrimaryAttacking;
        private bool isSecondaryAttacking;
        private float primaryAttackCooldownTimer;
        private float secondaryAttackCooldownTimer;

        // Dash state
        private bool isDashing;
        private float dashTimer;
        private float dashCooldownTimer;
        private float dashDirection;

        // Idle state
        private float idleTimer;
        private bool isLongIdle;

        // Ladder state
        private bool isOnLadder;
        private float verticalInput;
        private float baseGravityScale;
        // Collectibles
        private int coinsCollected = 0;

        // Water state
        private bool isInWater;

        public bool IsDead => health != null && health.Dead;
        public int CoinsCollected => coinsCollected;

        private void Awake()
        {

            // Get required components
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();

            // Get modular components
            health = GetComponent<Health>();
            flipController = GetComponent<FlipController>();
            groundCheck = GetComponent<GroundCheck>();
            projectileShooter = GetComponent<ProjectileShooter>();

            // Store base gravity scale
            baseGravityScale = rb.gravityScale;

            // Subscribe to health events
            if (health != null)
            {
                health.OnDeath.AddListener(OnDeath);
            }
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.OnDeath.RemoveListener(OnDeath);
            }
        }

        private void Update()
        {
            if (IsDead) return;

            HandleInput();
            HandleAttack();
            HandleDash();
            UpdateCooldowns();
            UpdateAnimations();
        }

        private void FixedUpdate()
        {
            if (IsDead) return;

            if (isSecondaryAttacking) return;

            // Handle ladder climbing
            if (isOnLadder)
            {
                HandleLadderClimbing();
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

            HandleJump();
            HandleMovement();
        }

        private void HandleInput()
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");
            verticalInput = Input.GetAxisRaw("Vertical");

            // Update flip controller
            if (flipController != null)
            {
                flipController.UpdateFacing(horizontalInput);
            }

            // Jump buffer
            if (Input.GetButtonDown("Jump"))
            {
                jumpBufferCounter = jumpBufferTime;
            }
            else
            {
                jumpBufferCounter -= Time.deltaTime;
            }

            // Variable jump height
            if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
                if (groundCheck != null)
                {
                    groundCheck.ConsumeCoyoteTime();
                }
            }
        }

        private void HandleJump()
        {
            bool canJump = (groundCheck != null && groundCheck.CanJump) || isInWater;

            if (jumpBufferCounter > 0f && canJump)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                jumpBufferCounter = 0f;
                if (groundCheck != null)
                {
                    groundCheck.ConsumeCoyoteTime();
                }
            }
        }

        private void HandleMovement()
        {
            float targetVelocity = horizontalInput * moveSpeed;
            float accelerationRate = (Mathf.Abs(targetVelocity) > 0.01f) ? acceleration : deceleration;
            currentHorizontalVelocity = Mathf.MoveTowards(currentHorizontalVelocity, targetVelocity, accelerationRate * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector2(currentHorizontalVelocity, rb.linearVelocity.y);
        }

        private void HandleLadderClimbing()
        {
            // Handle vertical climbing
            float verticalVelocity = verticalInput * climbSpeed;
            rb.linearVelocity = new Vector2(horizontalInput * moveSpeed * 0.5f, verticalVelocity);
            rb.gravityScale = 0f;

            Debug.Log($"Climbing ladder - Vertical: {verticalInput}");
        }

        private void HandleAttack()
        {
            // Primary Attack
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

            // Secondary Attack (Projectile)
            if ((Input.GetKeyDown(secondaryAttackKey1) || Input.GetKeyDown(secondaryAttackKey2))
                && secondaryAttackCooldownTimer <= 0f && !isPrimaryAttacking && !isSecondaryAttacking)
            {
                if (projectileShooter != null)
                {
                    isSecondaryAttacking = true;
                    secondaryAttackCooldownTimer = secondaryAttackCooldown;

                    if (animator != null)
                    {
                        animator.SetTrigger("LongAttack");
                    }

                    // moved to animation event
                    // projectileShooter.Shoot();
                    Debug.Log("Player used secondary attack!");
                }
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
                else if (flipController != null)
                {
                    dashDirection = flipController.FacingDirection;
                }
                else
                {
                    dashDirection = 1f;
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

        private void UpdateAnimations()
        {
            if (animator == null) return;

            bool isGrounded = groundCheck != null && groundCheck.IsGrounded;

            animator.SetFloat("Speed", Mathf.Abs(horizontalInput));
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetBool("IsDashing", isDashing);
            animator.SetBool("IsPrimaryAttacking", isPrimaryAttacking);
            animator.SetBool("IsSecondaryAttacking", isSecondaryAttacking);
            animator.SetBool("IsDead", IsDead);
            animator.SetBool("IsClimbing", isOnLadder && Mathf.Abs(verticalInput) > 0.01f); // in case I add a climbing animation

            bool isJumping = !isGrounded && rb.linearVelocity.y > 0f;
            bool isFalling = !isGrounded && rb.linearVelocity.y < 0f;
            animator.SetBool("IsJumping", isJumping);
            animator.SetBool("IsFalling", isFalling);

            bool isStopping = isGrounded && Mathf.Abs(horizontalInput) < 0.01f && Mathf.Abs(currentHorizontalVelocity) > 0.1f;
            animator.SetBool("IsStopping", isStopping);

            // Long idle handling
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

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag(coinTag))
            {
                coinsCollected++;
                Debug.Log($"Coin collected! Total coins: {coinsCollected}");
                Destroy(other.gameObject);
            }

            if (other.gameObject.CompareTag(waterTag))
            {
                isInWater = true;
            }

            if (other.gameObject.CompareTag("End"))
            {
                Debug.Log("you win!!!");
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }

            if (other.gameObject.CompareTag(ladderTag))
            {
                isOnLadder = true;
                Debug.Log("Entered ladder");
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.CompareTag(waterTag))
            {
                isInWater = false;
            }

            if (other.gameObject.CompareTag(ladderTag))
            {
                isOnLadder = false;
                rb.gravityScale = baseGravityScale;
                Debug.Log("Exited ladder");
            }
        }

        private void OnDeath()
        {
            rb.linearVelocity = Vector2.zero;
            Debug.Log("Player died!");
        }

        // Public methods for external access
        public void AddCoins(int amount)
        {
            coinsCollected += amount;
        }

        public Health GetHealth() => health;
        public FlipController GetFlipController() => flipController;
        public GroundCheck GetGroundCheck() => groundCheck;
        public ProjectileShooter GetProjectileShooter() => projectileShooter;
    }
}
