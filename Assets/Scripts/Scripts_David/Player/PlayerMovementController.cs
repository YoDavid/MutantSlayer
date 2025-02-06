using UnityEngine;

public class PlayerMovementController : MonoBehaviour
{
    private Rigidbody2D rb;
    private GameObject playerParent; // Reference to the parent object
    private PlayerAnimationController playerAnimationController;

    private bool isGrounded = false;
    private bool isDashing = false;

    // Expose movement settings to the Inspector
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f; // Normal movement speed
    [SerializeField] private float dashSpeed = 20f; // Dash movement speed
    [SerializeField] private float dashDuration = 0.2f; // Duration of the dash
    [SerializeField] private float dashCooldown = 1f; // Cooldown between dashes

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 10f; // Jump height
    [SerializeField] private KeyCode dashKey = KeyCode.LeftShift; // Dash key

    private float dashTime = 0f;
    private float lastDashTime = -999f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerAnimationController = GetComponent<PlayerAnimationController>();
      
    }

    private void Update()
    {
        bool isMovingLeft = Input.GetKey(KeyCode.A);
        bool isMovingRight = Input.GetKey(KeyCode.D);
        bool jumpPressed = Input.GetKeyDown(KeyCode.Space);
        bool dashPressed = Input.GetKeyDown(dashKey);

        float move = 0f;
        
        if (isMovingLeft)
        {
            move = -1f;
        }
        else if (isMovingRight)
        {
            move = 1f;
        }

        if (!isDashing)
        {
            Move(move);
        }

        if (jumpPressed && isGrounded)
        {
            Jump();
        }

        if (dashPressed && Time.time - lastDashTime > dashCooldown)
        {
            Dash(move);
        }

        playerAnimationController.UpdateAnimationStates(move, isGrounded, isDashing);
    }

    private void Move(float move)
    {
        // Flip the player sprite when moving left or right
        if (move < 0)
           transform.localScale = new Vector3(-1, 1, 1);
        else if (move > 0)
            transform.localScale = new Vector3(1, 1, 1);

        rb.velocity = new Vector2(move * moveSpeed, rb.velocity.y); // Use moveSpeed for movement
    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce); // Use jumpForce for jumping
    }

    private void Dash(float move)
    {
        isDashing = true;
        lastDashTime = Time.time;
        dashTime = Time.time;

        rb.velocity = new Vector2(dashSpeed * move, rb.velocity.y); // Use dashSpeed for dashing

        StartCoroutine(StopDash());
    }

    private System.Collections.IEnumerator StopDash()
    {
        yield return new WaitForSeconds(dashDuration);
        isDashing = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            playerAnimationController.SetJumpState(false);
            playerAnimationController.SetFallingState(false);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}
