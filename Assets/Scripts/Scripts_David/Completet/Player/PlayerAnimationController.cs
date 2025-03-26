using System.Collections;
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
        if (attackCount > 0)
        {
            animator.SetInteger("AttackCount", attackCount);
            animator.SetBool("IsAttacking", true);
        }
        else
        {
            StartCoroutine(ResetAttackState());
        }
    }

    private IEnumerator ResetAttackState()
    {
        yield return null;
        animator.SetBool("IsAttacking", false);
        animator.SetInteger("AttackCount", 0);
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
        animator.SetBool("IsFalling", isFalling);
    }

    public void SetDashingState(bool isDashing)
    {
        animator.SetBool("IsDashing", isDashing);
    }

    public void SetGroundedState(bool isGrounded)  // Set the IsGrounded flag
    {
        animator.SetBool("IsGrounded", isGrounded);
    }

    public void UpdateAnimationStates(float move, bool isGrounded, bool isDashing)
    {
        // Update grounded state
        SetGroundedState(isGrounded);

        // If not grounded and the player is falling (negative velocity in the Y direction)
        bool isFalling = rb.velocity.y < 0 && !isGrounded; // Player is falling if they are not grounded and moving downward
        bool isJumping = rb.velocity.y > 0 && !isGrounded; // Player is jumping if they are not grounded and moving upward

        // Handle jumping state
        if (isJumping)
        {
            SetJumpState(true);
            SetFallingState(false);  // Prevent falling animation if jumping
        }
        // Handle falling state
        else if (isFalling)
        {
            SetJumpState(false);
            SetFallingState(true);
        }
        else if (isGrounded)  // When grounded, we switch to idle or running
        {
            SetJumpState(false);
            SetFallingState(false);

            // Idle if not moving
            SetIdleState(move == 0);
        }

        // Set movement speed (idle or running animation)
        SetSpeed(move);

        // Set dashing state
        SetDashingState(isDashing);
    }
}
