using UnityEngine;

namespace TopDown
{
    /// <summary>
    /// Projectile for top-down games - moves in any direction
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class ProjectileTopDown : MonoBehaviour
    {
        [Header("Projectile Settings")]
        [SerializeField] private float speed = 10f;
        [SerializeField] private float lifetime = 3f;
        [SerializeField] private float damage = 10f;

        [Header("Collision Settings")]
        [SerializeField] private string[] targetTags = { "Enemy" };
        [SerializeField] private string[] destroyOnHitTags = { "Wall", "Obstacle" };

        private Vector2 direction;
        private float lifetimeTimer;

        private void Start()
        {
            lifetimeTimer = lifetime;
        }

        private void Update()
        {
            // Move the projectile
            transform.Translate(direction * speed * Time.deltaTime, Space.World);

            // Update lifetime
            lifetimeTimer -= Time.deltaTime;
            if (lifetimeTimer <= 0f)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Set the direction the projectile should travel
        /// </summary>
        public void SetDirection(Vector2 newDirection)
        {
            direction = newDirection.normalized;

            // Rotate the projectile to face the direction
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        /// <summary>
        /// Set projectile damage
        /// </summary>
        public void SetDamage(float newDamage)
        {
            damage = newDamage;
        }

        /// <summary>
        /// Set projectile speed
        /// </summary>
        public void SetSpeed(float newSpeed)
        {
            speed = newSpeed;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Check if hit a target
            foreach (string tag in targetTags)
            {
                if (other.CompareTag(tag))
                {
                    // Try to deal damage using IDamageable interface
                    IDamageable damageable = other.GetComponent<IDamageable>();
                    if (damageable != null)
                    {
                        damageable.TakeDamage(damage);
                    }

                    Destroy(gameObject);
                    return;
                }
            }

            // Check if hit obstacle
            foreach (string tag in destroyOnHitTags)
            {
                Debug.Log($"Checking collision with tag: {tag}");
                if (other.CompareTag(tag))
                {
                    Destroy(gameObject);
                    return;
                }
            }
        }
    }
}
