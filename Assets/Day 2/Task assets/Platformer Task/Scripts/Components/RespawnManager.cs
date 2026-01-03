using UnityEngine;
using UnityEngine.Events;

namespace Platformer
{
    /// <summary>
    /// Handles respawning when falling out of bounds or dying
    /// </summary>
    public class RespawnManager : MonoBehaviour
    {
        [Header("Out of Bounds Settings")]
        [SerializeField] private float fallThreshold = -10f;
        [SerializeField] private bool respawnOnFall = true;

        [Header("Respawn Settings")]
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private bool useInitialPositionAsSpawn = true;
        [SerializeField] private float respawnDelay = 0.5f;
        [SerializeField] private float deathRespawnDelay = 2f;
        [SerializeField] private bool resetVelocityOnRespawn = true;

        [Header("Damage Settings")]
        [SerializeField] private bool dealDamageOnFall = true;
        [SerializeField] private float fallDamage = 25f;
        [SerializeField] private bool instantKillOnFall = false;

        [Header("Events")]
        public UnityEvent OnFellOutOfBounds;
        public UnityEvent OnRespawn;
        public UnityEvent OnCheckpointSet;

        private Vector3 initialPosition;
        private Vector3 currentCheckpoint;
        private Rigidbody2D rb;
        private Health health;
        private bool isRespawning;

        public Vector3 CurrentCheckpoint => currentCheckpoint;

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            health = GetComponent<Health>();

            initialPosition = transform.position;

            // Set initial checkpoint
            if (spawnPoint != null)
            {
                currentCheckpoint = spawnPoint.position;
            }
            else if (useInitialPositionAsSpawn)
            {
                currentCheckpoint = initialPosition;
            }

            // Subscribe to death event for respawn
            if (health != null)
            {
                health.OnDeath.AddListener(OnPlayerDeath);
            }
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.OnDeath.RemoveListener(OnPlayerDeath);
            }
        }

        private void Update()
        {
            if (isRespawning) return;

            // Check if player fell out of bounds
            if (respawnOnFall && transform.position.y < fallThreshold)
            {
                HandleFallOutOfBounds();
            }
        }

        private void HandleFallOutOfBounds()
        {
            OnFellOutOfBounds?.Invoke();

            Debug.Log($"{gameObject.name} fell out of bounds!");

            // Deal damage or instant kill
            if (health != null)
            {
                if (instantKillOnFall)
                {
                    health.TakeDamage(health.MaxHealth);
                }
                else if (dealDamageOnFall)
                {
                    health.TakeDamage(fallDamage);
                }

                // If the fall killed the player, OnPlayerDeath will handle respawn at stage start
                if (health.Dead)
                {
                    return;
                }
            }

            // Player survived the fall - respawn at current checkpoint
            isRespawning = true;

            // Respawn after delay
            if (respawnDelay > 0f)
            {
                Invoke(nameof(Respawn), respawnDelay);
            }
            else
            {
                Respawn();
            }
        }

        private void OnPlayerDeath()
        {
            // Respawn at the initial spawn point (beginning of stage) on death
            if (!isRespawning)
            {
                isRespawning = true;

                // Reset checkpoint to initial position so player respawns at stage start
                currentCheckpoint = initialPosition;

                if (deathRespawnDelay > 0f)
                {
                    Invoke(nameof(Respawn), deathRespawnDelay);
                }
                else
                {
                    Respawn();
                }
            }
        }

        /// <summary>
        /// Respawn the player at the current checkpoint
        /// </summary>
        public void Respawn()
        {
            transform.position = currentCheckpoint;

            if (resetVelocityOnRespawn && rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }

            // Revive if health component exists and player is dead
            if (health != null && health.Dead)
            {
                health.Revive();
            }

            isRespawning = false;
            OnRespawn?.Invoke();

            Debug.Log($"{gameObject.name} respawned at checkpoint!");
        }

        /// <summary>
        /// Respawn at a specific position
        /// </summary>
        public void RespawnAt(Vector3 position)
        {
            currentCheckpoint = position;
            Respawn();
        }

        /// <summary>
        /// Set a new checkpoint position
        /// </summary>
        public void SetCheckpoint(Vector3 position)
        {
            currentCheckpoint = position;
            OnCheckpointSet?.Invoke();
            Debug.Log($"Checkpoint set at {position}");
        }

        /// <summary>
        /// Set checkpoint from a Transform
        /// </summary>
        public void SetCheckpoint(Transform checkpoint)
        {
            if (checkpoint != null)
            {
                SetCheckpoint(checkpoint.position);
            }
        }

        /// <summary>
        /// Reset checkpoint to initial spawn
        /// </summary>
        public void ResetCheckpoint()
        {
            if (spawnPoint != null)
            {
                currentCheckpoint = spawnPoint.position;
            }
            else
            {
                currentCheckpoint = initialPosition;
            }
        }

        /// <summary>
        /// Set the fall threshold at runtime
        /// </summary>
        public void SetFallThreshold(float threshold)
        {
            fallThreshold = threshold;
        }

        private void OnDrawGizmosSelected()
        {
            // Draw fall threshold line
            Gizmos.color = Color.red;
            Vector3 left = new Vector3(transform.position.x - 10f, fallThreshold, 0f);
            Vector3 right = new Vector3(transform.position.x + 10f, fallThreshold, 0f);
            Gizmos.DrawLine(left, right);

            // Draw checkpoint
            Gizmos.color = Color.green;
            Vector3 checkpoint = Application.isPlaying ? currentCheckpoint : (spawnPoint != null ? spawnPoint.position : transform.position);
            Gizmos.DrawWireSphere(checkpoint, 0.5f);
        }
    }
}
