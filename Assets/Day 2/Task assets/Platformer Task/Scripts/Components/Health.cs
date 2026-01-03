using UnityEngine;
using UnityEngine.Events;

namespace Platformer
{
    /// <summary>
    /// Reusable health component that can be attached to any GameObject
    /// </summary>
    public class Health : MonoBehaviour, IDamageable
    {
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float invincibilityDuration = 1f;
        [SerializeField] private bool destroyOnDeath = false;
        [SerializeField] private float destroyDelay = 2f;

        [Header("Visual Feedback")]
        [SerializeField] private bool flashOnDamage = true;
        [SerializeField] private float flashAlpha = 0.5f;

        [Header("Events")]
        public UnityEvent<float> OnDamaged;
        public UnityEvent<float> OnHealed;
        public UnityEvent OnDeath;
        public UnityEvent OnInvincibilityStart;
        public UnityEvent OnInvincibilityEnd;

        private float currentHealth;
        private bool isDead;
        private bool isInvincible;
        private float invincibilityTimer;

        private SpriteRenderer spriteRenderer;
        private Animator animator;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsInvincible => isInvincible;
        public bool Dead => isDead;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            currentHealth = maxHealth;
        }

        private void Update()
        {
            HandleInvincibility();
        }

        private void HandleInvincibility()
        {
            if (!isInvincible) return;

            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0f)
            {
                isInvincible = false;
                ResetVisuals();
                OnInvincibilityEnd?.Invoke();
            }
        }

        public void TakeDamage(float damage)
        {
            if (isDead || isInvincible) return;

            currentHealth = Mathf.Max(0f, currentHealth - damage);
            Debug.Log($"{gameObject.name} took {damage} damage! HP: {currentHealth}/{maxHealth}");

            // Start invincibility
            StartInvincibility();

            // Visual feedback
            if (flashOnDamage)
            {
                FlashSprite();
            }

            // Trigger animation
            if (animator != null)
            {
                animator.SetTrigger("TakeHit");
            }

            OnDamaged?.Invoke(damage);

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            if (isDead) return;

            float previousHealth = currentHealth;
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            float actualHeal = currentHealth - previousHealth;

            if (actualHeal > 0f)
            {
                Debug.Log($"{gameObject.name} healed {actualHeal}! HP: {currentHealth}/{maxHealth}");
                OnHealed?.Invoke(actualHeal);
            }
        }

        public void SetMaxHealth(float newMaxHealth, bool healToFull = false)
        {
            maxHealth = newMaxHealth;
            if (healToFull)
            {
                currentHealth = maxHealth;
            }
            else
            {
                currentHealth = Mathf.Min(currentHealth, maxHealth);
            }
        }

        private void StartInvincibility()
        {
            if (invincibilityDuration <= 0f) return;

            isInvincible = true;
            invincibilityTimer = invincibilityDuration;
            OnInvincibilityStart?.Invoke();
        }

        private void FlashSprite()
        {
            if (spriteRenderer == null) return;

            Color color = spriteRenderer.color;
            color.a = flashAlpha;
            spriteRenderer.color = color;
        }

        private void ResetVisuals()
        {
            if (spriteRenderer == null) return;

            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }

        private void Die()
        {
            if (isDead) return; // Prevent multiple death calls

            isDead = true;
            isInvincible = false; // Reset invincibility on death
            ResetVisuals();

            Debug.Log($"{gameObject.name} died!");

            if (animator != null)
            {
                animator.SetTrigger("Die");
                animator.SetBool("IsDead", true);
            }

            // Disable physics interactions on death
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.simulated = false;
            }

            OnDeath?.Invoke();

            if (destroyOnDeath)
            {
                Destroy(gameObject, destroyDelay);
            }
        }

        /// <summary>
        /// Revive the entity with optional health amount
        /// </summary>
        public void Revive(float healthAmount = -1f)
        {
            isDead = false;
            currentHealth = healthAmount < 0f ? maxHealth : healthAmount;
            isInvincible = false;
            ResetVisuals();

            // Re-enable physics
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.simulated = true;
            }

            if (animator != null)
            {
                animator.SetBool("IsDead", false);
                animator.SetTrigger("Revive");
            }
        }

        // IDamageable implementation
        public float GetCurrentHealth() => currentHealth;
        public float GetMaxHealth() => maxHealth;
        public bool IsDead() => isDead;
    }
}
