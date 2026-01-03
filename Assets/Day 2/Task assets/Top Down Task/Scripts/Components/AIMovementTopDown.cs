using UnityEngine;
using UnityEngine.Events;

namespace TopDown
{
    /// <summary>
    /// Patrol/chase movement for top-down AI enemies
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class AIMovementTopDown : MonoBehaviour
    {
        public enum AIState
        {
            Idle,
            Patrol,
            Chase,
            Return
        }

        [Header("Movement Settings")]
        [SerializeField] private float patrolSpeed = 2f;
        [SerializeField] private float chaseSpeed = 4f;

        [Header("Patrol Settings")]
        [SerializeField] private bool patrolEnabled = true;
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private float waypointReachDistance = 0.2f;
        [SerializeField] private float waitTimeAtWaypoint = 1f;

        [Header("Chase Settings")]
        [SerializeField] private bool chaseEnabled = true;
        [SerializeField] private float detectionRange = 5f;
        [SerializeField] private float chaseRange = 8f;
        [SerializeField] private string playerTag = "Player";

        [Header("Events")]
        public UnityEvent<AIState> OnStateChanged;
        public UnityEvent OnPlayerDetected;
        public UnityEvent OnPlayerLost;

        private Rigidbody2D rb;
        private DirectionController directionController;
        private HealthTopDown health;

        private AIState currentState = AIState.Idle;
        private Vector3 startPosition;
        private int currentWaypointIndex;
        private float waitTimer;
        private Transform playerTarget;

        public AIState CurrentState => currentState;
        public bool IsChasing => currentState == AIState.Chase;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            directionController = GetComponent<DirectionController>();
            health = GetComponent<HealthTopDown>();
            startPosition = transform.position;
        }

        private void Start()
        {
            if (patrolEnabled && waypoints.Length > 0)
            {
                SetState(AIState.Patrol);
            }
        }

        private void Update()
        {
            if (health != null && health.Dead)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            // Check for player
            if (chaseEnabled)
            {
                CheckForPlayer();
            }

            // State machine
            switch (currentState)
            {
                case AIState.Idle:
                    HandleIdle();
                    break;
                case AIState.Patrol:
                    HandlePatrol();
                    break;
                case AIState.Chase:
                    HandleChase();
                    break;
                case AIState.Return:
                    HandleReturn();
                    break;
            }
        }

        private void CheckForPlayer()
        {
            GameObject player = GameObject.FindGameObjectWithTag(playerTag);
            if (player == null) return;

            float distance = Vector2.Distance(transform.position, player.transform.position);

            if (currentState != AIState.Chase)
            {
                if (distance <= detectionRange)
                {
                    playerTarget = player.transform;
                    SetState(AIState.Chase);
                    OnPlayerDetected?.Invoke();
                }
            }
            else
            {
                if (distance > chaseRange)
                {
                    playerTarget = null;
                    SetState(AIState.Return);
                    OnPlayerLost?.Invoke();
                }
            }
        }

        private void HandleIdle()
        {
            rb.linearVelocity = Vector2.zero;
        }

        private void HandlePatrol()
        {
            if (waypoints.Length == 0)
            {
                SetState(AIState.Idle);
                return;
            }

            if (waitTimer > 0f)
            {
                waitTimer -= Time.deltaTime;
                rb.linearVelocity = Vector2.zero;
                return;
            }

            Transform target = waypoints[currentWaypointIndex];
            Vector2 direction = (target.position - transform.position).normalized;
            float distance = Vector2.Distance(transform.position, target.position);

            if (distance < waypointReachDistance)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
                waitTimer = waitTimeAtWaypoint;
            }
            else
            {
                rb.linearVelocity = direction * patrolSpeed;
                UpdateDirection(direction);
            }
        }

        private void HandleChase()
        {
            if (playerTarget == null)
            {
                SetState(AIState.Return);
                return;
            }

            Vector2 direction = (playerTarget.position - transform.position).normalized;
            rb.linearVelocity = direction * chaseSpeed;
            UpdateDirection(direction);
        }

        private void HandleReturn()
        {
            Vector3 returnTarget = waypoints.Length > 0 ? waypoints[0].position : startPosition;
            Vector2 direction = (returnTarget - transform.position).normalized;
            float distance = Vector2.Distance(transform.position, returnTarget);

            if (distance < waypointReachDistance)
            {
                currentWaypointIndex = 0;
                if (patrolEnabled && waypoints.Length > 0)
                {
                    SetState(AIState.Patrol);
                }
                else
                {
                    SetState(AIState.Idle);
                }
            }
            else
            {
                rb.linearVelocity = direction * patrolSpeed;
                UpdateDirection(direction);
            }
        }

        private void UpdateDirection(Vector2 direction)
        {
            if (directionController != null)
            {
                directionController.UpdateDirection(direction);
            }
        }

        private void SetState(AIState newState)
        {
            if (currentState == newState) return;

            currentState = newState;
            OnStateChanged?.Invoke(newState);
        }

        public void SetPatrolEnabled(bool enabled)
        {
            patrolEnabled = enabled;
            if (!enabled && currentState == AIState.Patrol)
            {
                SetState(AIState.Idle);
            }
        }

        public void SetChaseEnabled(bool enabled)
        {
            chaseEnabled = enabled;
            if (!enabled && currentState == AIState.Chase)
            {
                SetState(AIState.Return);
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Detection range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            // Chase range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, chaseRange);

            // Waypoints
            if (waypoints != null && waypoints.Length > 0)
            {
                Gizmos.color = Color.cyan;
                for (int i = 0; i < waypoints.Length; i++)
                {
                    if (waypoints[i] != null)
                    {
                        Gizmos.DrawSphere(waypoints[i].position, 0.2f);
                        if (i < waypoints.Length - 1 && waypoints[i + 1] != null)
                        {
                            Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                        }
                    }
                }
                // Connect last to first
                if (waypoints.Length > 1 && waypoints[0] != null && waypoints[waypoints.Length - 1] != null)
                {
                    Gizmos.DrawLine(waypoints[waypoints.Length - 1].position, waypoints[0].position);
                }
            }
        }
    }
}
