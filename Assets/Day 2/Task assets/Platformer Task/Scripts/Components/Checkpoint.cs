using UnityEngine;

namespace Platformer
{
    /// <summary>
    /// Checkpoint trigger - sets the player's respawn point when entered
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Checkpoint : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private bool activateOnce = true;
        [SerializeField] private Transform respawnPosition;

        [Header("Visual Feedback")]
        [SerializeField] private SpriteRenderer flagRenderer;
        [SerializeField] private Color activeColor = Color.green;
        [SerializeField] private Color inactiveColor = Color.gray;

        private bool isActivated = false;

        public bool IsActivated => isActivated;

        private void Awake()
        {
            if (respawnPosition == null)
            {
                respawnPosition = transform;
            }

            UpdateVisuals();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (activateOnce && isActivated) return;

            if (other.CompareTag(playerTag))
            {
                RespawnManager respawnManager = other.GetComponent<RespawnManager>();
                if (respawnManager != null)
                {
                    respawnManager.SetCheckpoint(respawnPosition.position);
                    isActivated = true;
                    UpdateVisuals();

                    Debug.Log($"Checkpoint activated: {gameObject.name}");
                }
            }
        }

        private void UpdateVisuals()
        {
            if (flagRenderer != null)
            {
                flagRenderer.color = isActivated ? activeColor : inactiveColor;
            }
        }

        /// <summary>
        /// Reset the checkpoint to inactive state
        /// </summary>
        public void ResetCheckpoint()
        {
            isActivated = false;
            UpdateVisuals();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = isActivated ? Color.green : Color.yellow;
            Vector3 pos = respawnPosition != null ? respawnPosition.position : transform.position;
            Gizmos.DrawWireSphere(pos, 0.3f);
            Gizmos.DrawIcon(pos, "Checkpoint", true);
        }
    }
}
