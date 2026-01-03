using UnityEngine;
using UnityEngine.Events;

namespace Platformer
{
    /// <summary>
    /// Handles projectile spawning and shooting
    /// </summary>
    public class ProjectileShooter : MonoBehaviour
    {
        [Header("Projectile Settings")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float spawnOffset = 0.5f;

        [Header("Shooting Settings")]
        [SerializeField] private float cooldown = 0.5f;
        [SerializeField] private string targetTag = "Enemy";

        [Header("Events")]
        public UnityEvent OnShoot;
        public UnityEvent OnCooldownComplete;

        private FlipController flipController;
        private float cooldownTimer;

        public bool CanShoot => cooldownTimer <= 0f;
        public float CooldownRemaining => cooldownTimer;
        public float CooldownProgress => 1f - (cooldownTimer / cooldown);

        private void Awake()
        {
            flipController = GetComponent<FlipController>();
        }

        private void Update()
        {
            if (cooldownTimer > 0f)
            {
                cooldownTimer -= Time.deltaTime;
                if (cooldownTimer <= 0f)
                {
                    OnCooldownComplete?.Invoke();
                }
            }
        }

        /// <summary>
        /// Shoot a projectile in the facing direction
        /// </summary>
        public void Shoot()
        {
            if (!CanShoot || projectilePrefab == null) return;

            Vector2 direction = GetShootDirection();
            ShootInDirection(direction);
        }

        /// <summary>
        /// Shoot a projectile in a specific direction
        /// </summary>
        public void ShootInDirection(Vector2 direction)
        {
            if (!CanShoot || projectilePrefab == null)
            {
                if (projectilePrefab == null)
                {
                    Debug.LogWarning($"{gameObject.name}: Projectile prefab is not assigned!");
                }
                return;
            }

            Vector3 spawnPosition = GetSpawnPosition(direction);
            GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

            Projectile projectileScript = projectile.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                projectileScript.SetDirection(direction);
                projectileScript.TargetTag = targetTag;
            }

            cooldownTimer = cooldown;
            OnShoot?.Invoke();

            Debug.Log($"{gameObject.name} shot projectile!");
        }

        /// <summary>
        /// Shoot towards a specific target
        /// </summary>
        public void ShootAtTarget(Transform target)
        {
            if (target == null) return;

            Vector2 direction = (target.position - transform.position).normalized;
            ShootInDirection(direction);
        }

        private Vector2 GetShootDirection()
        {

            // Default to facing direction
            if (flipController != null)
            {
                return flipController.IsFacingRight ? Vector2.right : Vector2.left;
            }

            return Vector2.right;
        }

        private Vector3 GetSpawnPosition(Vector2 direction)
        {
            if (spawnPoint != null)
            {
                return spawnPoint.position;
            }

            // Calculate offset based on direction
            Vector3 offset = new Vector3(direction.x * spawnOffset, direction.y * spawnOffset, 0f);
            return transform.position + offset;
        }

        /// <summary>
        /// Set the projectile prefab at runtime
        /// </summary>
        public void SetProjectilePrefab(GameObject prefab)
        {
            projectilePrefab = prefab;
        }

        /// <summary>
        /// Reset cooldown (allow immediate shooting)
        /// </summary>
        public void ResetCooldown()
        {
            cooldownTimer = 0f;
        }
    }
}
