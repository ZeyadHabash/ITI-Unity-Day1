using UnityEngine;
using UnityEngine.Events;

namespace Platformer
{
    /// <summary>
    /// Handles ground detection for platformer characters
    /// </summary>
    public class GroundCheck : MonoBehaviour
    {
        [Header("Ground Check Settings")]
        [SerializeField] private string groundTag = "Ground";
        [SerializeField] private float normalThreshold = 0.5f;

        [Header("Coyote Time")]
        [SerializeField] private float coyoteTime = 0.1f;

        [Header("Events")]
        public UnityEvent OnGrounded;
        public UnityEvent OnAirborne;

        private bool isGrounded;
        private float coyoteTimeCounter;
        private bool wasGrounded;

        public bool IsGrounded => isGrounded;
        public bool CanJump => coyoteTimeCounter > 0f;

        private void Update()
        {
            // Update coyote time
            if (isGrounded)
            {
                coyoteTimeCounter = coyoteTime;
            }
            else
            {
                coyoteTimeCounter -= Time.deltaTime;
            }

            // Fire events on state change
            if (isGrounded && !wasGrounded)
            {
                OnGrounded?.Invoke();
            }
            else if (!isGrounded && wasGrounded)
            {
                OnAirborne?.Invoke();
            }

            wasGrounded = isGrounded;
        }

        /// <summary>
        /// Consume coyote time (call when jumping)
        /// </summary>
        public void ConsumeCoyoteTime()
        {
            coyoteTimeCounter = 0f;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag(groundTag))
            {
                CheckGroundContact(collision);
            }
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag(groundTag))
            {
                CheckGroundContact(collision);
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag(groundTag))
            {
                isGrounded = false;
            }
        }

        private void CheckGroundContact(Collision2D collision)
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                // Normal pointing up means we're standing on top
                if (contact.normal.y > normalThreshold)
                {
                    isGrounded = true;
                    return;
                }
            }
            isGrounded = false;
        }
    }
}
