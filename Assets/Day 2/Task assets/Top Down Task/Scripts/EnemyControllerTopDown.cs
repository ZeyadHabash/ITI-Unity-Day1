using UnityEngine;

namespace TopDown
{
    /// <summary>
    /// Enemy controller for top-down games using modular components
    /// Attach HealthTopDown, DirectionController, AIMovementTopDown, and ContactDamageTopDown components
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyControllerTopDown : MonoBehaviour
    {
        // Component references
        private Rigidbody2D rb;
        private Animator animator;
        private HealthTopDown health;
        private DirectionController directionController;
        private AIMovementTopDown aiMovement;
        private ContactDamageTopDown contactDamage;

        public bool IsDead => health != null && health.Dead;

        private void Awake()
        {
            // Get required components
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();

            // Get modular components
            health = GetComponent<HealthTopDown>();
            directionController = GetComponent<DirectionController>();
            aiMovement = GetComponent<AIMovementTopDown>();
            contactDamage = GetComponent<ContactDamageTopDown>();

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

            float speed = rb != null ? rb.linearVelocity.magnitude : 0f;
            animator.SetFloat("Speed", speed);
            animator.SetBool("IsDead", IsDead);

            if (directionController != null)
            {
                animator.SetFloat("LastHorizontal", directionController.Horizontal);
                animator.SetFloat("LastVertical", directionController.Vertical);
            }

            if (aiMovement != null)
            {
                animator.SetBool("IsChasing", aiMovement.IsChasing);
            }
        }

        private void OnDeath()
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }

            // Disable AI movement
            if (aiMovement != null)
            {
                aiMovement.SetPatrolEnabled(false);
                aiMovement.SetChaseEnabled(false);
            }

            // Disable contact damage
            if (contactDamage != null)
            {
                contactDamage.enabled = false;
            }

            // Disable collider
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.enabled = false;
            }

            Debug.Log($"{gameObject.name} died!");
        }

        // Public accessors
        public HealthTopDown GetHealth() => health;
        public DirectionController GetDirectionController() => directionController;
        public AIMovementTopDown GetAIMovement() => aiMovement;
        public ContactDamageTopDown GetContactDamage() => contactDamage;
    }
}
