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

    private int lockedFacingDirection;


    [Header("Step Sound Settings")]
    [SerializeField] private float stepInterval = 0.4f; // How often steps play
    private bool isPlayingSteps = false;

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

        // Check for dash cancel (opposite direction input during dash)
        if (isDashing && Mathf.Sign(move) == -Mathf.Sign(lockedFacingDirection) && move != 0)
        {
            StopDashEarly();
        }

        Move(move);
        HandleStepSound(move);


        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isDashing)
            {
                StopCoroutine("StopDash"); 
                isDashing = false;
                lastDashEndTime = Time.time;

                playerHurtbox.SetInvincible(false);
            }

            if (isGrounded || isDashing)
                StartJump();
        }
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

    private Coroutine stepCoroutine;

    private void HandleStepSound(float move)
    {
        bool shouldPlaySteps = Mathf.Abs(move) > 0.1f && isGrounded && isMovementEnabled; // Added isMovementEnabled check

        if (shouldPlaySteps && !isPlayingSteps && !isDashing)
        {
            stepCoroutine = StartCoroutine(PlayStepSoundLoop());
            isPlayingSteps = true;
        }
        else if ((!shouldPlaySteps && isPlayingSteps) || isDashing || !isMovementEnabled) // Added !isMovementEnabled
        {
            if (stepCoroutine != null) StopCoroutine(stepCoroutine);
            isPlayingSteps = false;
        }
    }

    private IEnumerator PlayStepSoundLoop()
    {
        while (true)
        {
            // Use the new method that randomizes the pitch
            AudioManager.Instance.PlaySFXWithRandomPitch("PlayerOthers", "sfx_player_footsteps");

            // Wait for the next step interval
            yield return new WaitForSeconds(stepInterval);
        }
    }

    public void StopStepSounds()
    {
        if (isPlayingSteps)
        {
            if (stepCoroutine != null)
            {
                StopCoroutine(stepCoroutine);
            }
            isPlayingSteps = false;
        }
    }

    ///    Take CARE!!!!!!!!!!
    private bool CanJumpAfterDash()
    {
        return !isDashing && (lastDashEndTime < 0 || Time.time - lastDashEndTime >= dashJumpCooldown);
    }

    private void Move(float move)
    {
        HandleFlip(move);

        // Skip movement calculations if dashing
        if (isDashing) return;

        float targetSpeed = move * moveSpeed; // Removed dash multiplier from here

        if (Mathf.Abs(rb.velocity.x - targetSpeed) > 0.1f &&
            Mathf.Abs(rb.velocity.x) < moveSpeed * 3f)
        {
            float speedDiff = targetSpeed - rb.velocity.x;
            rb.AddForce(Vector2.right * speedDiff * 15f);
        }
    }


    private void HandleFlip(float move)
    {
        if (isDashing)
        {
            // Force maintain locked direction during dash
            transform.localScale = new Vector3(lockedFacingDirection, 1, 1);
            return;
        }

        if (move < 0) facingDirection = -1;
        else if (move > 0) facingDirection = 1;
        transform.localScale = new Vector3(facingDirection, 1, 1);
    }

    private void StartJump()
    {
        isJumping = true;
        jumpTimeCounter = maxJumpTime;
        rb.velocity = new Vector2(rb.velocity.x * 1f, jumpForce);
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
        lockedFacingDirection = facingDirection;

        playerHurtbox.SetInvincible(true);
        rb.velocity = new Vector2(lockedFacingDirection * dashSpeed, rb.velocity.y);

        AudioManager.Instance.PlayDash();
        StartCoroutine(StopDash());
    }

    private void StopDashEarly()
    {
        StopCoroutine("StopDash");
        isDashing = false;
        lastDashEndTime = Time.time;
        playerHurtbox.SetInvincible(false);

        // Preserve some momentum if desired (optional)
        rb.velocity = new Vector2(rb.velocity.x * 0.5f, rb.velocity.y);
    }


    private IEnumerator StopDash()
    {
        yield return new WaitForSeconds(dashDuration);

        // Only stop dash if it wasn't already cancelled
        if (isDashing)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            playerHurtbox.SetInvincible(false);
            isDashing = false;
            lastDashEndTime = Time.time;
        }
    }

    public void SetMovementEnabled(bool enabled)
    {
        isMovementEnabled = enabled;

        if (!enabled)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            playerAnimationController.SetSpeed(0);
            StopStepSounds(); // Add this line
        }
    }

    private void CheckIfGrounded()
    {
        isGrounded = Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckDistance, groundLayer);
        playerAnimationController.SetGroundedState(isGrounded);
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