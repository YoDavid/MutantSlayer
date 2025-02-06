using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetSpeed(float speed)
    {
        animator.SetFloat("Speed", Mathf.Abs(speed));
    }

    public void SetAttackState(int attackCount)
    {
        animator.SetInteger("AttackCount", attackCount);
        animator.SetBool("IsAttacking", attackCount > 0);
    }

    public void SetJumpState(bool isJumping)
    {
        animator.SetBool("IsJumping", isJumping);
    }

    public void SetIdleState(bool isIdle)
    {
        animator.SetBool("IsIdle", isIdle);
    }

    public void SetFallingState(bool isFalling)
    {
        animator.SetBool("IsFalling", isFalling);  // This could be a new parameter for falling
    }

    public void SetDashingState(bool isDashing)
    {
        animator.SetBool("IsDashing", isDashing);  // Add this line for dash animation
    }

    public void UpdateAnimationStates(float move, bool isGrounded, bool isDashing)
    {
        // Transition to jump/fall or idle/run based on velocity
        if (!isGrounded)
        {
            // Check if falling (negative vertical velocity)
            bool isFalling = rb.velocity.y < 0;
            SetFallingState(isFalling);
            SetJumpState(!isFalling);
        }
        else
        {
            // Player is grounded, transition to idle or running
            SetJumpState(false);
            SetFallingState(false);
            SetIdleState(move == 0);
        }

        // Handle running/idle
        SetSpeed(move);

        // Update dash animation state
        SetDashingState(isDashing);
    }
}
