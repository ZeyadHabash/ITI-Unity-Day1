using UnityEngine;

namespace Platformer
{
    public class CameraFollow : MonoBehaviour
    {
        private Transform target;

        [Header("Follow Settings")]
        [SerializeField] private float smoothSpeed = 5f;
        [SerializeField] private Vector3 offset = new Vector3(0f, 1f, -10f);

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

        private Vector3 velocity = Vector3.zero;
        private float currentLookAhead = 0f;
        private Rigidbody2D targetRb;

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
            // we need this bec player is instantiated after camera (due to PlayerInit script)
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

            if (useLookAhead && targetRb != null)
            {
                float targetLookAhead = Mathf.Sign(targetRb.linearVelocity.x) * lookAheadDistance;

                if (Mathf.Abs(targetRb.linearVelocity.x) > 0.5f)
                {
                    currentLookAhead = Mathf.Lerp(currentLookAhead, targetLookAhead, lookAheadSpeed * Time.deltaTime);
                }
                else
                {
                    currentLookAhead = Mathf.Lerp(currentLookAhead, 0f, lookAheadSpeed * Time.deltaTime);
                }

                desiredPosition.x += currentLookAhead;
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
            targetRb = target?.GetComponent<Rigidbody2D>();
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
