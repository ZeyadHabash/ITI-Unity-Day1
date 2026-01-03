using UnityEngine;
using UnityEngine.Events;

namespace Platformer
{
    /// <summary>
    /// Handles sprite flipping based on movement direction
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class FlipController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool startFacingRight = true;
        [SerializeField] private bool useScaleFlip = false;

        [Header("Events")]
        public UnityEvent<bool> OnFlip;

        private SpriteRenderer spriteRenderer;
        private bool isFacingRight;

        public bool IsFacingRight => isFacingRight;
        public float FacingDirection => isFacingRight ? 1f : -1f;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            isFacingRight = startFacingRight;
            ApplyFlip();
        }

        /// <summary>
        /// Update facing direction based on horizontal input
        /// </summary>
        public void UpdateFacing(float horizontalInput)
        {
            if (horizontalInput > 0.01f && !isFacingRight)
            {
                SetFacingRight(true);
            }
            else if (horizontalInput < -0.01f && isFacingRight)
            {
                SetFacingRight(false);
            }
        }

        /// <summary>
        /// Directly set facing direction
        /// </summary>
        public void SetFacingRight(bool facingRight)
        {
            if (isFacingRight == facingRight) return;

            isFacingRight = facingRight;
            ApplyFlip();
            OnFlip?.Invoke(isFacingRight);
        }

        /// <summary>
        /// Flip to face the opposite direction
        /// </summary>
        public void Flip()
        {
            SetFacingRight(!isFacingRight);
        }

        /// <summary>
        /// Face towards a target position
        /// </summary>
        public void FaceTowards(Vector3 targetPosition)
        {
            bool shouldFaceRight = targetPosition.x > transform.position.x;
            SetFacingRight(shouldFaceRight);
        }

        private void ApplyFlip()
        {
            if (useScaleFlip)
            {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x) * (isFacingRight ? 1f : -1f);
                transform.localScale = scale;
            }
            else if (spriteRenderer != null)
            {
                spriteRenderer.flipX = !isFacingRight;
            }
        }
    }
}
