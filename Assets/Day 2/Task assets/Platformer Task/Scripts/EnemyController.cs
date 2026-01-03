using UnityEngine;

namespace Platformer
{
    /// <summary>
    /// Refactored EnemyController using modular components
    /// Attach Health, FlipController, PatrolMovement, and ContactDamage components to the same GameObject
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyController : MonoBehaviour
    {
        // Component references
        private Rigidbody2D rb;
        private Animator animator;
        private Health health;
        private FlipController flipController;
        private PatrolMovement patrolMovement;
        private ContactDamage contactDamage;

        public bool IsDead => health != null && health.Dead;

        private void Awake()
        {
            // Get required components
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();

            // Get modular components
            health = GetComponent<Health>();
            flipController = GetComponent<FlipController>();
            patrolMovement = GetComponent<PatrolMovement>();
            contactDamage = GetComponent<ContactDamage>();

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

            UpdateAnimations();
        }

        private void UpdateAnimations()
        {
            if (animator == null) return;

            float speed = rb != null ? Mathf.Abs(rb.linearVelocity.x) : 0f;
            animator.SetFloat("Speed", speed);
            animator.SetBool("IsDead", IsDead);
        }

        private void OnDeath()
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }

            // Disable patrol
            if (patrolMovement != null)
            {
                patrolMovement.SetPatrolEnabled(false);
            }

            // Disable contact damage
            if (contactDamage != null)
            {
                contactDamage.enabled = false;
            }

            // Disable collider so player can walk through
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.enabled = false;
            }

            Debug.Log($"{gameObject.name} died!");
        }

        // Public accessors for components
        public Health GetHealth() => health;
        public FlipController GetFlipController() => flipController;
        public PatrolMovement GetPatrolMovement() => patrolMovement;
        public ContactDamage GetContactDamage() => contactDamage;
    }
}
