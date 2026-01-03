using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace TopDown
{
    /// <summary>
    /// Reusable health component for top-down games with blink effect
    /// </summary>
    public class HealthTopDown : MonoBehaviour, IDamageable
    {
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float invincibilityDuration = 1f;
        [SerializeField] private bool destroyOnDeath = false;
        [SerializeField] private float destroyDelay = 2f;

        [Header("Blink Effect")]
        [SerializeField] private bool blinkOnDamage = true;
        [SerializeField] private int blinkCount = 5;
        [SerializeField] private float blinkInterval = 0.1f;

        [Header("Events")]
        public UnityEvent<float> OnDamaged;
        public UnityEvent<float> OnHealed;
        public UnityEvent OnDeath;
        public UnityEvent OnInvincibilityStart;
        public UnityEvent OnInvincibilityEnd;

        private float currentHealth;
        private bool isDead;
        private bool isInvincible;

        private SpriteRenderer spriteRenderer;
        private Animator animator;
        private Coroutine blinkCoroutine;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsInvincible => isInvincible;
        public bool Dead => isDead;

        private void Awake()
        {
            // Try to get SpriteRenderer on this object first, then search in children
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
            animator = GetComponent<Animator>();
            currentHealth = maxHealth;
        }

        public void TakeDamage(float damage)
        {
            if (isDead || isInvincible) return;

            currentHealth = Mathf.Max(0f, currentHealth - damage);
            Debug.Log($"{gameObject.name} took {damage} damage! HP: {currentHealth}/{maxHealth}");

            // Start invincibility with blink
            StartInvincibility();

            // Trigger animation
            if (animator != null)
            {
                animator.SetTrigger("Hurt");
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
            OnInvincibilityStart?.Invoke();

            if (blinkOnDamage && spriteRenderer != null)
            {
                if (blinkCoroutine != null)
                {
                    StopCoroutine(blinkCoroutine);
                }
                blinkCoroutine = StartCoroutine(BlinkCoroutine());
            }
            else
            {
                StartCoroutine(InvincibilityTimerCoroutine());
            }
        }

        private IEnumerator BlinkCoroutine()
        {
            for (int i = 0; i < blinkCount; i++)
            {
                spriteRenderer.enabled = false;
                yield return new WaitForSeconds(blinkInterval);
                spriteRenderer.enabled = true;
                yield return new WaitForSeconds(blinkInterval);
            }

            spriteRenderer.enabled = true;
            isInvincible = false;
            OnInvincibilityEnd?.Invoke();
        }

        private IEnumerator InvincibilityTimerCoroutine()
        {
            yield return new WaitForSeconds(invincibilityDuration);
            isInvincible = false;
            OnInvincibilityEnd?.Invoke();
        }

        private void Die()
        {
            isDead = true;
            Debug.Log($"{gameObject.name} died!");

            // Stop blinking
            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
            }
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
            }

            if (animator != null)
            {
                animator.SetTrigger("Die");
                animator.SetBool("IsDead", true);
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

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
            }

            if (animator != null)
            {
                animator.SetBool("IsDead", false);
            }
        }

        // IDamageable implementation
        public float GetCurrentHealth() => currentHealth;
        public float GetMaxHealth() => maxHealth;
        public bool IsDead() => isDead;
    }
}
