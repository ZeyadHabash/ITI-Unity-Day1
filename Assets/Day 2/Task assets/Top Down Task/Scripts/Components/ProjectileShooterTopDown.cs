using UnityEngine;
using UnityEngine.Events;

namespace TopDown
{
    /// <summary>
    /// Handles projectile spawning for top-down games (supports 4/8 directions)
    /// </summary>
    public class ProjectileShooterTopDown : MonoBehaviour
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

        private DirectionController directionController;
        private float cooldownTimer;

        public bool CanShoot => cooldownTimer <= 0f;
        public float CooldownRemaining => cooldownTimer;
        public float CooldownProgress => 1f - (cooldownTimer / cooldown);

        private void Awake()
        {
            directionController = GetComponent<DirectionController>();
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

            // Try to set direction on ProjectileTopDown
            ProjectileTopDown projectileTopDown = projectile.GetComponent<ProjectileTopDown>();
            if (projectileTopDown != null)
            {
                projectileTopDown.SetDirection(direction);
            }

            cooldownTimer = cooldown;
            OnShoot?.Invoke();

            Debug.Log($"{gameObject.name} shot projectile in direction {direction}!");
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

        /// <summary>
        /// Try to shoot at the nearest target with the specified tag
        /// </summary>
        public void ShootAtNearestTarget()
        {
            GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);

            Transform nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (GameObject target in targets)
            {
                float distance = Vector2.Distance(transform.position, target.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = target.transform;
                }
            }

            if (nearest != null)
            {
                ShootAtTarget(nearest);
            }
            else
            {
                Shoot();
            }
        }

        private Vector2 GetShootDirection()
        {
            // Default to facing direction
            if (directionController != null)
            {
                return directionController.FacingDirection;
            }

            return Vector2.down; // Default for top-down
        }

        private Vector3 GetSpawnPosition(Vector2 direction)
        {
            if (spawnPoint != null)
            {
                return spawnPoint.position;
            }

            Vector3 offset = new Vector3(direction.x * spawnOffset, direction.y * spawnOffset, 0f);
            return transform.position + offset;
        }

        public void SetProjectilePrefab(GameObject prefab)
        {
            projectilePrefab = prefab;
        }

        public void ResetCooldown()
        {
            cooldownTimer = 0f;
        }
    }
}
