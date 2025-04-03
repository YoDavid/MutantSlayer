using System.Collections;
using UnityEngine;

public class PlayerMovementController : MonoBehaviour
{
    private Rigidbody2D rb;
    private PlayerAnimationController playerAnimationController;
    private PlayerAttackController playerAttackController;
    private PlayerHurtbox playerHurtbox;

    [Header("Debugging")]
    public bool isGrounded = false;
    public bool isDashing = false;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float dashMoveSpeedMultiplier = 2.4f;

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 25f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;

    [Header("Dash Jump Settings")]
    [SerializeField] private float dashJumpCooldown = 0.2f;
    private float lastDashEndTime = -999f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float maxJumpTime = 0.35f;
    [SerializeField] private float jumpCancelRate = 0.5f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Gravity Settings")]
    [SerializeField] private float gravityScale = 2.5f;

    [Header("Gizmos Settings")]
    public bool drawGizmos = false;

    [SerializeField] private float lastDashTime = -999f;
    private int facingDirection = 1;
    private bool isJumping = false;
    private float jumpTimeCounter;
    private bool isMovementEnabled = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;
        playerAnimationController = GetComponent<PlayerAnimationController>();
        playerAttackController = GetComponent<PlayerAttackController>();
        playerHurtbox = GetComponentInChildren<PlayerHurtbox>();
    }

    private void Update()
    {
        HandleInput();
        CheckIfGrounded();
        HandleJump();
    }

    private void HandleInput()
    {
        if (!isMovementEnabled) return;

        float move = 0f;
        if (!playerAttackController.IsAttacking)
        {
            if (Input.GetKey(KeyCode.A)) move = -1f;
            else if (Input.GetKey(KeyCode.D)) move = 1f;
        }

        Move(move);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && CanJumpAfterDash())
            StartJump();
        if (Input.GetKey(KeyCode.Space) && isJumping)
            ContinueJump();
        if (Input.GetKeyUp(KeyCode.Space) && isJumping)
            CancelJump();

        if (Input.GetKeyDown(KeyCode.LeftShift) &&
            Time.time - lastDashTime > dashCooldown &&
            !isJumping &&
            isGrounded)
        {
            Dash();
        }

        playerAnimationController.UpdateAnimationStates(move, isGrounded, isDashing);
    }

    private bool CanJumpAfterDash()
    {
        return !isDashing && (lastDashEndTime < 0 || Time.time - lastDashEndTime >= dashJumpCooldown);
    }

    private void Move(float move)
    {
        HandleFlip(move);
        float targetSpeed = move * (isDashing ? moveSpeed * dashMoveSpeedMultiplier : moveSpeed);

        if (Mathf.Abs(rb.velocity.x - targetSpeed) > 0.1f &&
            Mathf.Abs(rb.velocity.x) < moveSpeed * 3f)
        {
            float speedDiff = targetSpeed - rb.velocity.x;
            rb.AddForce(Vector2.right * speedDiff * 15f);
        }
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
        if (isDashing || isJumping || !isGrounded) return;

        isDashing = true;
        lastDashTime = Time.time;

        playerHurtbox.SetInvincible(true);
        rb.velocity = new Vector2(facingDirection * dashSpeed, rb.velocity.y);
        StartCoroutine(StopDash());
    }

    private IEnumerator StopDash()
    {
        yield return new WaitForSeconds(dashDuration);

        playerHurtbox.SetInvincible(false);
        rb.velocity = new Vector2(0, rb.velocity.y);
        isDashing = false;
        lastDashEndTime = Time.time;
    }

    public void SetMovementEnabled(bool enabled)
    {
        isMovementEnabled = enabled;
        if (!enabled)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    private void CheckIfGrounded()
    {
        isGrounded = Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckDistance, groundLayer);
    }

    private void OnDrawGizmos()
    {
        if (drawGizmos && groundCheckPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(groundCheckPoint.position, 0.1f);
            Gizmos.DrawLine(groundCheckPoint.position, groundCheckPoint.position + Vector3.down * groundCheckDistance);
        }
    }
}