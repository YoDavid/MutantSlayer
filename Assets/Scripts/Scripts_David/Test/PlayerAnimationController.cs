using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private Animator animator;
    private int attackCount = 0;
    private float lastAttackTime = 0f;
    private float attackResetTime = 0.5f; // Reset combo after this time if no X press
    private bool isAttacking = false;

    private bool isGrounded = false; // To check if the player is on the ground
    private float jumpForce = 10f; // The force with which the player jumps
    private bool isJumping = false; // To track whether the player is in the air

    private Rigidbody2D rb; // Reference to the player's Rigidbody2D component

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>(); // Get the Rigidbody2D component
    }

    void Update()
    {
        float move = Input.GetAxisRaw("Horizontal");
        bool attackPressed = Input.GetKeyDown(KeyCode.X); // Detect X press
        bool jumpPressed = Input.GetKeyDown(KeyCode.Space); // Detect Space press for jump

        // Set movement speed for walking/running animation
        animator.SetFloat("Speed", Mathf.Abs(move));

        // Flip sprite based on movement
        if (move < 0)
            transform.localScale = new Vector3(-1, 1, 1);
        else if (move > 0)
            transform.localScale = new Vector3(1, 1, 1);

        // Attack Logic - pressing X starts or continues attack combo
        if (attackPressed)
        {
            // Reset attack count after 0.5 seconds if not pressed again
            if (Time.time - lastAttackTime > attackResetTime)
            {
                attackCount = 1; // Start a new combo if time has passed
            }
            else if (attackCount < 3) // Continue combo if within 0.5 seconds
            {
                attackCount++; // Increment the attack count
            }

            lastAttackTime = Time.time; // Update the time of the attack press
            animator.SetInteger("AttackCount", attackCount); // Set the AttackCount in the Animator
            animator.SetBool("IsAttacking", true); // Set IsAttacking to true
            isAttacking = true; // Mark the player as attacking
        }

        // If no X press within 0.5 seconds, reset combo and return to idle
        if (isAttacking && Time.time - lastAttackTime > attackResetTime)
        {
            ResetAttack();
        }

        // Jump Logic - pressing space to jump when grounded
        if (jumpPressed && isGrounded)
        {
            isJumping = true; // Set the player to be jumping
            rb.velocity = new Vector2(rb.velocity.x, jumpForce); // Apply upward velocity to simulate jumping
            animator.SetBool("IsJumping", true); // Set IsJumping to true to play jump animation
        }

        // If the player is not attacking or jumping, set the idle animation
        if (!isAttacking && isGrounded && !isJumping)
        {
            animator.SetBool("IsIdle", true); // Set IsIdle to true for idle animation
            animator.SetBool("IsJumping", false); // Reset IsJumping when on the ground
        }
    }

    // Reset the attack combo if the player hasn't pressed X within the reset time
    public void ResetAttack()
    {
        attackCount = 0;
        animator.SetInteger("AttackCount", 0); // Reset AttackCount in Animator
        animator.SetBool("IsAttacking", false); // Reset IsAttacking to false
        isAttacking = false; // Mark the player as no longer attacking
    }

    // Detect if the player is grounded by checking for collisions with "Ground" tagged objects
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true; // The player is grounded if they collide with an object tagged "Ground"
            isJumping = false; // Reset jumping status when landing
            animator.SetBool("IsJumping", false); // Set IsJumping to false when landing
            animator.SetBool("IsIdle", true); // Transition to idle when landing
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // While the player is still touching the ground, continue to set isGrounded to true
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // If the player leaves the ground, set isGrounded to false (indicating they are in the air)
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}
