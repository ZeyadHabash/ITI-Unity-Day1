using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace TopDown
{
    /// <summary>
    /// Handles respawning for top-down games (out of bounds or death)
    /// </summary>
    public class RespawnManagerTopDown : MonoBehaviour
    {
        [Header("Respawn Settings")]
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private bool useInitialPositionAsSpawn = true;
        [SerializeField] private float respawnDelay = 0.5f;
        [SerializeField] private bool resetVelocityOnRespawn = true;

        [Header("Out of Bounds (Optional)")]
        [SerializeField] private bool checkBounds = false;
        [SerializeField] private float minX = -50f;
        [SerializeField] private float maxX = 50f;
        [SerializeField] private float minY = -50f;
        [SerializeField] private float maxY = 50f;

        [Header("Damage Settings")]
        [SerializeField] private bool dealDamageOnOutOfBounds = true;
        [SerializeField] private float outOfBoundsDamage = 25f;

        [Header("Events")]
        public UnityEvent OnOutOfBounds;
        public UnityEvent OnRespawn;
        public UnityEvent OnCheckpointSet;

        private Vector3 initialPosition;
        private Vector3 currentCheckpoint;
        private Rigidbody2D rb;
        private HealthTopDown health;
        private bool isRespawning;

        public Vector3 CurrentCheckpoint => currentCheckpoint;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            health = GetComponent<HealthTopDown>();

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

            // Subscribe to death event
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

            // Check bounds
            if (checkBounds)
            {
                bool outOfBounds = transform.position.x < minX || transform.position.x > maxX ||
                                   transform.position.y < minY || transform.position.y > maxY;

                if (outOfBounds)
                {
                    HandleOutOfBounds();
                }
            }
        }

        private void HandleOutOfBounds()
        {
            isRespawning = true;
            OnOutOfBounds?.Invoke();

            Debug.Log($"{gameObject.name} went out of bounds!");

            // Deal damage
            if (health != null && dealDamageOnOutOfBounds)
            {
                health.TakeDamage(outOfBoundsDamage);
            }

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
            // if (respawnDelay > 0)
            //     Invoke(nameof(Respawn), respawnDelay);
            // else
            //     Respawn();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        /// <summary>
        /// Respawn at current checkpoint
        /// </summary>
        public void Respawn()
        {
            transform.position = currentCheckpoint;

            if (resetVelocityOnRespawn && rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }

            // Revive if dead
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
        /// Reset to initial spawn
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

        private void OnDrawGizmosSelected()
        {
            if (checkBounds)
            {
                Gizmos.color = Color.red;
                Vector3 center = new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, 0f);
                Vector3 size = new Vector3(maxX - minX, maxY - minY, 0f);
                Gizmos.DrawWireCube(center, size);
            }

            // Draw checkpoint
            Gizmos.color = Color.green;
            Vector3 checkpoint = Application.isPlaying ? currentCheckpoint : (spawnPoint != null ? spawnPoint.position : transform.position);
            Gizmos.DrawWireSphere(checkpoint, 0.5f);
        }
    }
}
