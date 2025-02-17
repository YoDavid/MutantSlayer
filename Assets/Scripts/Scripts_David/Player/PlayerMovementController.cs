using UnityEngine;

public class PlayerMovementController : MonoBehaviour
{
    private Rigidbody2D rb;
    private PlayerAnimationController playerAnimationController;
    private PlayerAttackController playerAttackController;

    [Header("Debugging")]
    public bool isGrounded = false;
    public bool isDashing = false;
    public bool isCollidingWithWall = false;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float maxJumpTime = 0.35f;
    [SerializeField] private float jumpCancelRate = 0.5f;
    [SerializeField] private KeyCode dashKey = KeyCode.LeftShift;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Wall Slide Settings")]
    [SerializeField] private float wallSlideSpeed = 2f;
    [SerializeField] private LayerMask wallLayer;

    [Header("Gravity Settings")]
    [SerializeField] private float gravityScale = 2.5f;

    private float lastDashTime = -999f;
    private int facingDirection = 1;
    private bool isJumping = false;
    private float jumpTimeCounter;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;
        playerAnimationController = GetComponent<PlayerAnimationController>();
        playerAttackController = GetComponent<PlayerAttackController>();
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
        float move = 0f;
        if (!playerAttackController.IsAttacking)
        {
            if (Input.GetKey(KeyCode.A)) move = -1f;
            else if (Input.GetKey(KeyCode.D)) move = 1f;
        }

        Move(move);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            StartJump();
        if (Input.GetKey(KeyCode.Space) && isJumping)
            ContinueJump();
        if (Input.GetKeyUp(KeyCode.Space) && isJumping)
            CancelJump();

        if (Input.GetKeyDown(dashKey) && Time.time - lastDashTime > dashCooldown)
            Dash();

        playerAnimationController.UpdateAnimationStates(move, isGrounded, isDashing);
    }

    private void Move(float move)
    {
        HandleFlip(move);
        rb.velocity = new Vector2(move * moveSpeed, rb.velocity.y);
    }

    private void HandleFlip(float move)
    {
        if (move < 0) facingDirection = -1;
        else if (move > 0) facingDirection = 1;
        transform.localScale = new Vector3(facingDirection, 1, 1);
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
        else isJumping = false;
    }

    private void CancelJump()
    {
        if (rb.velocity.y > 0)
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCancelRate);
        isJumping = false;
    }

    private void HandleJump()
    {
        if (!isGrounded)
            rb.gravityScale = gravityScale;
    }

    private void Dash()
    {
        isDashing = true;
        lastDashTime = Time.time;
        rb.velocity = new Vector2(facingDirection * dashDistance / dashDuration, rb.velocity.y);
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
            rb.velocity = new Vector2(0, rb.velocity.y < 0 ? -wallSlideSpeed : rb.velocity.y);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
            isCollidingWithWall = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
            isCollidingWithWall = false;
    }
}