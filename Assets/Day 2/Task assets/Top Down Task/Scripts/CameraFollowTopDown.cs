using UnityEngine;

namespace TopDown
{
    public class CameraFollowTopDown : MonoBehaviour
    {
        private Transform target;

        [Header("Follow Settings")]
        [SerializeField] private float smoothSpeed = 5f;
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

        [Header("Boundaries (Optional)")]
        [SerializeField] private bool useBoundaries = false;
        [SerializeField] private float minX = -10f;
        [SerializeField] private float maxX = 10f;
        [SerializeField] private float minY = -5f;
        [SerializeField] private float maxY = 5f;

        [Header("Look Ahead")]
        [SerializeField] private bool useLookAhead = true;
        [SerializeField] private float lookAheadDistance = 2f;
        [SerializeField] private float lookAheadSpeed = 3f;

        private Vector2 currentLookAhead = Vector2.zero;
        private PlayerControllerTopDown playerController;

        private void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                SetTarget(player.transform);
                SnapToTarget();
            }
        }

        private void LateUpdate()
        {
            // We need this because player is instantiated after camera (due to PlayerInit script)
            if (target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    SetTarget(player.transform);
                }
                else
                {
                    return;
                }
            }

            Vector3 desiredPosition = target.position + offset;

            // Apply look-ahead in both X and Y directions for top-down movement
            if (useLookAhead && playerController != null)
            {
                Vector2 velocity = playerController.GetVelocity();
                Vector2 targetLookAhead = Vector2.zero;

                // Calculate look-ahead based on velocity direction
                if (velocity.magnitude > 0.5f)
                {
                    targetLookAhead = velocity.normalized * lookAheadDistance;
                }

                currentLookAhead = Vector2.Lerp(currentLookAhead, targetLookAhead, lookAheadSpeed * Time.deltaTime);

                desiredPosition.x += currentLookAhead.x;
                desiredPosition.y += currentLookAhead.y;
            }

            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

            if (useBoundaries)
            {
                smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, minX, maxX);
                smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, minY, maxY);
            }

            smoothedPosition.z = offset.z;

            transform.position = smoothedPosition;
        }

        public void SetTarget(Transform newTarget)
        {
            if (newTarget == null) return;
            if (newTarget == target) return;

            target = newTarget;
            playerController = target?.GetComponent<PlayerControllerTopDown>();
        }

        public void SnapToTarget()
        {
            if (target != null)
            {
                transform.position = target.position + offset;
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Visualize camera boundaries in editor
            if (useBoundaries)
            {
                Gizmos.color = Color.yellow;
                Vector3 bottomLeft = new Vector3(minX, minY, 0);
                Vector3 topLeft = new Vector3(minX, maxY, 0);
                Vector3 topRight = new Vector3(maxX, maxY, 0);
                Vector3 bottomRight = new Vector3(maxX, minY, 0);

                Gizmos.DrawLine(bottomLeft, topLeft);
                Gizmos.DrawLine(topLeft, topRight);
                Gizmos.DrawLine(topRight, bottomRight);
                Gizmos.DrawLine(bottomRight, bottomLeft);
            }
        }
    }
}
