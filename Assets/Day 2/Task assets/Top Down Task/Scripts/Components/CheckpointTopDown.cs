using UnityEngine;

namespace TopDown
{
    /// <summary>
    /// Checkpoint for top-down games
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class CheckpointTopDown : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private bool activateOnce = true;
        [SerializeField] private Transform respawnPosition;

        [Header("Visual Feedback")]
        [SerializeField] private SpriteRenderer indicatorRenderer;
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
                RespawnManagerTopDown respawnManager = other.GetComponent<RespawnManagerTopDown>();
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
            if (indicatorRenderer != null)
            {
                indicatorRenderer.color = isActivated ? activeColor : inactiveColor;
            }
        }

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
        }
    }
}
