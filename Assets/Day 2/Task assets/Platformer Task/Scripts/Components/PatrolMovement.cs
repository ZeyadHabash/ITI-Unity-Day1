using UnityEngine;
using UnityEngine.Events;

namespace Platformer
{
    /// <summary>
    /// Handles patrol movement for AI characters
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PatrolMovement : MonoBehaviour
    {
        [Header("Patrol Settings")]
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float patrolDistance = 3f;
        [SerializeField] private bool startMovingRight = true;

        [Header("Behavior")]
        [SerializeField] private bool patrolEnabled = true;
        [SerializeField] private bool useWaypoints = false;
        [SerializeField] private Transform[] waypoints;

        [Header("Events")]
        public UnityEvent OnDirectionChanged;
        public UnityEvent OnWaypointReached;

        private Rigidbody2D rb;
        private FlipController flipController;

        private Vector3 startPosition;
        private bool movingRight;
        private int currentWaypointIndex;

        public bool IsMovingRight => movingRight;
        public bool IsPatrolling => patrolEnabled;
        public float MoveSpeed => moveSpeed;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            flipController = GetComponent<FlipController>();
            startPosition = transform.position;
            movingRight = startMovingRight;
        }

        private void FixedUpdate()
        {
            if (!patrolEnabled) return;

            if (useWaypoints && waypoints.Length > 0)
            {
                HandleWaypointPatrol();
            }
            else
            {
                HandleDistancePatrol();
            }
        }

        private void HandleDistancePatrol()
        {
            float distanceFromStart = transform.position.x - startPosition.x;

            if (movingRight && distanceFromStart >= patrolDistance)
            {
                ChangeDirection();
            }
            else if (!movingRight && distanceFromStart <= -patrolDistance)
            {
                ChangeDirection();
            }

            ApplyMovement();
        }

        private void HandleWaypointPatrol()
        {
            if (waypoints.Length == 0) return;

            Transform target = waypoints[currentWaypointIndex];
            float distanceToTarget = Vector2.Distance(transform.position, target.position);

            if (distanceToTarget < 0.1f)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
                OnWaypointReached?.Invoke();
            }

            // Move towards current waypoint
            Vector2 direction = (target.position - transform.position).normalized;
            rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);

            // Update facing direction
            if (flipController != null)
            {
                flipController.UpdateFacing(direction.x);
            }
        }

        private void ApplyMovement()
        {
            float direction = movingRight ? 1f : -1f;
            rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
        }

        private void ChangeDirection()
        {
            movingRight = !movingRight;

            if (flipController != null)
            {
                flipController.SetFacingRight(movingRight);
            }

            OnDirectionChanged?.Invoke();
        }

        /// <summary>
        /// Enable or disable patrol behavior
        /// </summary>
        public void SetPatrolEnabled(bool enabled)
        {
            patrolEnabled = enabled;

            if (!enabled && rb != null)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
        }

        /// <summary>
        /// Set patrol speed
        /// </summary>
        public void SetMoveSpeed(float speed)
        {
            moveSpeed = speed;
        }

        /// <summary>
        /// Reset patrol to start position
        /// </summary>
        public void ResetPatrol()
        {
            transform.position = startPosition;
            movingRight = startMovingRight;
            currentWaypointIndex = 0;
        }
    }
}
