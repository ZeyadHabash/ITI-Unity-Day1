using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 10f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.1f;

    [Header("Ground Check")]
    [SerializeField] private string groundTag = "Ground";

    [Header("Collectibles")]
    [SerializeField] private string coinTag = "Coin";

    [Header("Gravity")]
    [SerializeField] private float gravity = 30f;

    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private float horizontalInput;
    private float currentVelocity;
    private float verticalVelocity;
    private bool isFacingRight = true;
    private bool isGrounded;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            verticalVelocity = jumpForce;
            jumpBufferCounter = 0f;
        }

        if (Input.GetButtonUp("Jump") && verticalVelocity > 0f)
        {
            verticalVelocity *= 0.5f;
            coyoteTimeCounter = 0f;
        }

        // Apply gravity
        if (!isGrounded)
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }
        else if (verticalVelocity < 0f)
        {
            verticalVelocity = 0f;
        }

        // Calculate horizontal movement with acceleration/deceleration
        float targetVelocity = horizontalInput * moveSpeed;
        float accelerationRate = (Mathf.Abs(targetVelocity) > 0.01f) ? acceleration : deceleration;
        currentVelocity = Mathf.MoveTowards(currentVelocity, targetVelocity, accelerationRate * Time.deltaTime);

        // Apply movement using Transform
        Vector3 movement = new Vector3(currentVelocity, verticalVelocity, 0f) * Time.deltaTime;
        transform.Translate(movement);

        Flip();
        UpdateAnimations();
    }

    private void Flip()
    {
        // Flip the sprite based on movement direction
        if (horizontalInput > 0f && !isFacingRight)
        {
            isFacingRight = true;
            spriteRenderer.flipX = false;
        }
        else if (horizontalInput < 0f && isFacingRight)
        {
            isFacingRight = false;
            spriteRenderer.flipX = true;
        }
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        animator.SetFloat("Speed", Mathf.Abs(horizontalInput));
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetFloat("VerticalVelocity", verticalVelocity);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(groundTag))
        {
            isGrounded = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(coinTag))
        {
            // increment score
            Destroy(other.gameObject);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(groundTag))
        {
            isGrounded = false;
        }
    }
}