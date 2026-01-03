using UnityEngine;

namespace Platformer
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        [Header("Projectile Settings")]
        [SerializeField] private float speed = 10f;
        [SerializeField] private float lifetime = 3f;
        [SerializeField] private float damage = 10f;
        public string TargetTag { get; set; } = "Enemy";

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

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Check if hit an enemy
            if (other.CompareTag(TargetTag))
            {
                // Try to deal damage to the enemy using IDamageable interface
                IDamageable damageable = other.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(damage);
                }

                // Destroy the projectile
                Destroy(gameObject);
            }
            // Destroy on hitting ground or obstacles
            else if (other.CompareTag("Ground") || other.CompareTag("Wall"))
            {
                Destroy(gameObject);
            }
        }
    }
}
