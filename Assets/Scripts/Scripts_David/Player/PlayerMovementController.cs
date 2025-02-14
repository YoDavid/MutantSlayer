using UnityEngine;

public class PlayerMovementController : MonoBehaviour
{
    private Rigidbody2D rb;
    private PlayerAnimationController playerAnimationController;
    private PlayerAttackController playerAttackController;  // Reference to PlayerAttackController

    [Header("Debugging")]
    public bool isGrounded = false;
    public bool isDashing = false;
    public bool isCollidingWithWall = false;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 8f;  // Adjusted value
    [SerializeField] private float maxJumpTime = 0.35f;  // Adjusted value
    [SerializeField] private float jumpCancelRate = 0.5f;  // Adjusted value
    [SerializeField] private KeyCode dashKey = KeyCode.LeftShift;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Wall Slide Settings")]
    [SerializeField] private float wallSlideSpeed = 2f;
    [SerializeField] private LayerMask wallLayer;

    [Header("Gravity Settings")]  // New section
    [SerializeField] private float gravityScale = 2.5f;  // Adjusted value

    private float lastDashTime = -999f;
    private int facingDirection = 1;
    private bool isJumping = false;
    private float jumpTimeCounter;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;  // Set gravity scale from Inspector
        playerAnimationController = GetComponent<PlayerAnimationController>();
        playerAttackController = GetComponent<PlayerAttackController>();  // Get the PlayerAttackController attached to the player
    }

    private void Update()
    {
        HandleInput();
        CheckIfGrounded();
        HandleWallSlide();
        HandleJump();
    }

    private void HandleInput()
    {
        bool isMovingLeft = Input.GetKey(KeyCode.A);
        bool isMovingRight = Input.GetKey(KeyCode.D);

        bool jumpPressed = Input.GetKeyDown(KeyCode.Space);
        bool jumpHeld = Input.GetKey(KeyCode.Space);
        bool jumpReleased = Input.GetKeyUp(KeyCode.Space);
        bool dashPressed = Input.GetKeyDown(dashKey);

        float move = 0f;
        if (!playerAttackController.IsAttacking)  // Check if player is attacking
        {
            if (isMovingLeft) move = -1f;
            else if (isMovingRight) move = 1f;
        }

        Move(move);

        if (jumpPressed && isGrounded)
        {
            StartJump();
        }

        if (jumpHeld && isJumping)
        {
            ContinueJump();
        }

        if (jumpReleased && isJumping)
        {
            CancelJump();
        }

        if (dashPressed && Time.time - lastDashTime > dashCooldown)
        {
            Dash(move);
        }

        playerAnimationController.UpdateAnimationStates(move, isGrounded, isDashing);  // No longer passing attack state here
    }

    private void Move(float move)
    {
        HandleFlip(move);
        rb.velocity = new Vector2(move * moveSpeed, rb.velocity.y);
    }

    private void HandleFlip(float move)
    {
        if (move < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
            facingDirection = -1;
        }
        else if (move > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
            facingDirection = 1;
        }
    }

    private void StartJump()
    {
        isJumping = true;
        jumpTimeCounter = maxJumpTime;
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    }

    private void ContinueJump()
    {
        if (jumpTimeCounter > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpTimeCounter -= Time.deltaTime;
        }
        else
        {
            isJumping = false;
        }
    }

    private void CancelJump()
    {
        if (rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCancelRate);
        }
        isJumping = false;
    }

    private void HandleJump()
    {
        if (!isGrounded)
        {
            rb.gravityScale = gravityScale;  // Reset gravity scale when not grounded
        }
    }

    private void Dash(float move)
    {
        isDashing = true;
        lastDashTime = Time.time;
        rb.velocity = new Vector2(dashSpeed * move, rb.velocity.y);
        StartCoroutine(StopDash());
    }

    private System.Collections.IEnumerator StopDash()
    {
        yield return new WaitForSeconds(dashDuration);
        isDashing = false;
    }

    private void CheckIfGrounded()
    {
        isGrounded = Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckDistance, groundLayer);
    }

    private void HandleWallSlide()
    {
        if (isCollidingWithWall)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            if (rb.velocity.y < 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            isCollidingWithWall = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            isCollidingWithWall = false;
        }
    }
}
