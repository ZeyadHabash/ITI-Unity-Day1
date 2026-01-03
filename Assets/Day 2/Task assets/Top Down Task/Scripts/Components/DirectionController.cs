using UnityEngine;
using UnityEngine.Events;

namespace TopDown
{
    /// <summary>
    /// Handles 4-directional or 8-directional facing for top-down games
    /// </summary>
    public class DirectionController : MonoBehaviour
    {
        public enum DirectionMode
        {
            FourWay,    // Up, Down, Left, Right
            EightWay    // Includes diagonals
        }

        [Header("Settings")]
        [SerializeField] private DirectionMode mode = DirectionMode.FourWay;
        [SerializeField] private Vector2 initialDirection = Vector2.down;

        [Header("Events")]
        public UnityEvent<Vector2> OnDirectionChanged;

        private Vector2 currentDirection;
        private Vector2 lastNonZeroDirection;

        public Vector2 CurrentDirection => currentDirection;
        public Vector2 FacingDirection => lastNonZeroDirection;

        // Convenience properties for animation blend trees
        public float Horizontal => lastNonZeroDirection.x;
        public float Vertical => lastNonZeroDirection.y;

        private void Awake()
        {
            currentDirection = initialDirection.normalized;
            lastNonZeroDirection = initialDirection.normalized;
        }

        /// <summary>
        /// Update direction based on input
        /// </summary>
        public void UpdateDirection(Vector2 input)
        {
            if (input.sqrMagnitude < 0.01f)
            {
                currentDirection = Vector2.zero;
                return;
            }

            Vector2 newDirection = input.normalized;

            if (mode == DirectionMode.FourWay)
            {
                newDirection = GetCardinalDirection(input);
            }

            if (newDirection != lastNonZeroDirection)
            {
                lastNonZeroDirection = newDirection;
                OnDirectionChanged?.Invoke(newDirection);
            }

            currentDirection = newDirection;
        }

        /// <summary>
        /// Directly set facing direction
        /// </summary>
        public void SetDirection(Vector2 direction)
        {
            if (direction.sqrMagnitude < 0.01f) return;

            Vector2 newDirection = direction.normalized;

            if (mode == DirectionMode.FourWay)
            {
                newDirection = GetCardinalDirection(direction);
            }

            lastNonZeroDirection = newDirection;
            currentDirection = newDirection;
            OnDirectionChanged?.Invoke(newDirection);
        }

        /// <summary>
        /// Face towards a target position
        /// </summary>
        public void FaceTowards(Vector3 targetPosition)
        {
            Vector2 direction = (targetPosition - transform.position).normalized;
            SetDirection(direction);
        }

        /// <summary>
        /// Get the cardinal (4-way) direction from input
        /// </summary>
        private Vector2 GetCardinalDirection(Vector2 input)
        {
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            {
                return input.x > 0 ? Vector2.right : Vector2.left;
            }
            else
            {
                return input.y > 0 ? Vector2.up : Vector2.down;
            }
        }

        /// <summary>
        /// Get angle in degrees (0 = right, 90 = up, etc.)
        /// </summary>
        public float GetAngle()
        {
            return Mathf.Atan2(lastNonZeroDirection.y, lastNonZeroDirection.x) * Mathf.Rad2Deg;
        }

        /// <summary>
        /// Check if facing a specific cardinal direction
        /// </summary>
        public bool IsFacing(Vector2 direction)
        {
            return Vector2.Dot(lastNonZeroDirection, direction.normalized) > 0.9f;
        }

        public bool IsFacingUp() => IsFacing(Vector2.up);
        public bool IsFacingDown() => IsFacing(Vector2.down);
        public bool IsFacingLeft() => IsFacing(Vector2.left);
        public bool IsFacingRight() => IsFacing(Vector2.right);
    }
}
