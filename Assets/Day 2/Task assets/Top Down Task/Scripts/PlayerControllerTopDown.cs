using UnityEngine;

namespace TopDown
{
    /// <summary>
    /// Refactored PlayerControllerTopDown using modular components
    /// Attach HealthTopDown, DirectionController, ProjectileShooterTopDown, and RespawnManagerTopDown components
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerControllerTopDown : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 8f;

        [Header("Attack Settings")]
        [SerializeField] private float primaryAttackCooldown = 0.5f;
        [SerializeField] private KeyCode primaryAttackKey1 = KeyCode.Z;
        [SerializeField] private KeyCode primaryAttackKey2 = KeyCode.J;
        [SerializeField] private KeyCode secondaryAttackKey1 = KeyCode.X;
        [SerializeField] private KeyCode secondaryAttackKey2 = KeyCode.K;

        [Header("Collectibles")]
        [SerializeField] private string coinTag = "Coin";
        [SerializeField] private float coinCollectionRadius = 0.5f;

        // Component references
        private Rigidbody2D rb;
        private Animator animator;
        private HealthTopDown health;
        private DirectionController directionController;
        private ProjectileShooterTopDown projectileShooter;

        // Movement state
        private Vector2 inputDirection;
        private Vector2 currentVelocity;

        // Attack state
        private float primaryAttackTimer = 0f;
        private bool isAttacking = false;

        // Collectibles
        private int coinsCollected = 0;

        public bool IsDead => health != null && health.Dead;
        public int CoinsCollected => coinsCollected;

        private void Awake()
        {
            // Get required components
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();

            // Get modular components
            health = GetComponent<HealthTopDown>();
            directionController = GetComponent<DirectionController>();
            projectileShooter = GetComponent<ProjectileShooterTopDown>();

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

            // Update cooldowns
            if (primaryAttackTimer > 0f)
                primaryAttackTimer -= Time.deltaTime;

            HandleInput();
            HandleAttackInput();
            UpdateAnimations();
        }

        private void FixedUpdate()
        {
            if (IsDead)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            rb.linearVelocity = currentVelocity;
        }

        private void HandleInput()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            inputDirection = new Vector2(horizontal, vertical);

            if (inputDirection.magnitude > 1f)
            {
                inputDirection.Normalize();
            }

            // Update direction controller
            if (directionController != null)
            {
                directionController.UpdateDirection(inputDirection);
            }

            currentVelocity = inputDirection * moveSpeed;
        }

        private void HandleAttackInput()
        {
            // Primary Attack
            if ((Input.GetKeyDown(primaryAttackKey1) || Input.GetKeyDown(primaryAttackKey2))
                && primaryAttackTimer <= 0f && !isAttacking)
            {
                PrimaryAttack();
            }

            // Secondary Attack (Projectile)
            if ((Input.GetKeyDown(secondaryAttackKey1) || Input.GetKeyDown(secondaryAttackKey2)) && !isAttacking)
            {
                if (projectileShooter != null && projectileShooter.CanShoot)
                {
                    SecondaryAttack();
                }
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
            isAttacking = true;

            if (animator != null)
            {
                animator.SetTrigger("SecondaryAttack");
            }

            if (projectileShooter != null)
            {
                projectileShooter.Shoot();
            }

            Debug.Log("Secondary Attack!");
            Invoke(nameof(ResetAttackState), 0.3f);
        }

        private void ResetAttackState()
        {
            isAttacking = false;
        }

        private void UpdateAnimations()
        {
            if (animator == null) return;

            animator.SetFloat("Speed", inputDirection.magnitude);

            if (directionController != null)
            {
                animator.SetFloat("LastHorizontal", directionController.Horizontal);
                animator.SetFloat("LastVertical", directionController.Vertical);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryCollectCoin(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryCollectCoin(other);
        }

        private void TryCollectCoin(Collider2D other)
        {
            if (other.gameObject.CompareTag(coinTag))
            {
                // Only collect if the coin is within the collection radius (not just the magnet range)
                float distance = Vector2.Distance(transform.position, other.transform.position);
                if (distance <= coinCollectionRadius)
                {
                    coinsCollected++;
                    Debug.Log($"Coin collected! Total coins: {coinsCollected}");
                    Destroy(other.gameObject);
                }
            }
        }

        private void OnDeath()
        {
            rb.linearVelocity = Vector2.zero;
            Debug.Log("Player died!");
        }

        // Public accessors
        public Vector2 GetVelocity() => currentVelocity;

        public Vector2 GetFacingDirection()
        {
            if (directionController != null)
            {
                return directionController.FacingDirection;
            }
            return Vector2.down;
        }

        public bool IsAttacking() => isAttacking;
        public void AddCoins(int amount) => coinsCollected += amount;

        public HealthTopDown GetHealth() => health;
        public DirectionController GetDirectionController() => directionController;
        public ProjectileShooterTopDown GetProjectileShooter() => projectileShooter;
    }
}
