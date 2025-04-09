using System.Collections;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;
    private PlayerMovementController movementController;
    private PlayerAttackController attackController;

    [Header("Hit Stun Settings")]
    [SerializeField] private float hitStunDuration = 0.3f;
    private bool isInHitStun = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        movementController = GetComponent<PlayerMovementController>();
        attackController = GetComponent<PlayerAttackController>();
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

    public void SetGroundedState(bool isGrounded)
    {
        animator.SetBool("IsGrounded", isGrounded);
    }

    public void TriggerTakenHit()
    {
        if (!isInHitStun && !animator.GetBool("IsDashing")) 
        {
            StartCoroutine(HitStunRoutine());
            FaceAnchor faceAnchor = GetComponentInChildren<FaceAnchor>();
            if (faceAnchor != null)
            {
                faceAnchor.OnHitAnimationTriggered();
            }
        }
    }

    private IEnumerator HitStunRoutine()
    {
        isInHitStun = true;
        animator.SetTrigger("TakenHit");

        // Freeze player controls and movement
        movementController.SetMovementEnabled(false);
        attackController.SetAttackEnabled(false);
        rb.velocity = Vector2.zero; // Stop any existing movement

        yield return new WaitForSeconds(hitStunDuration);

        movementController.SetMovementEnabled(true);
        attackController.SetAttackEnabled(true);

        isInHitStun = false;
    }

    public void UpdateAnimationStates(float move, bool isGrounded, bool isDashing)
    {
        SetGroundedState(isGrounded);

        bool isFalling = rb.velocity.y < 0 && !isGrounded;
        bool isJumping = rb.velocity.y > 0 && !isGrounded;

        if (isJumping)
        {
            SetJumpState(true);
            SetFallingState(false);
        }
        else if (isFalling)
        {
            SetJumpState(false);
            SetFallingState(true);
        }
        else if (isGrounded)
        {
            SetJumpState(false);
            SetFallingState(false);
            SetIdleState(move == 0);
        }

        SetSpeed(move);
        SetDashingState(isDashing);
    }
}