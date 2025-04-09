using System.Collections;
using UnityEngine;

public class BossJumpAttackBehaviorTesting : MonoBehaviour
{
    public float jumpAnticipationTime = 0.7f;
    public float jumpHeightMin = 3f;
    public float jumpHeightMax = 6f;

    private Animator animator;
    private CameraShake cameraShake;
    private BossJumpAttackHitbox jumpHitbox;
    private BossAttackManagerTesting attackManager;
    private BossGroundCheckHandlerTesting groundCheckHandler;
    private BossFlipHandlerTesting flipHandler;
    private BossMovementHandlerTesting movementHandler;  // Reference to BossMovementHandlerTesting

    private bool isSmashTriggered;
    private float jumpForce;
    private float jumpSpeed;
    private Vector2 targetPosition;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        cameraShake = Camera.main?.GetComponent<CameraShake>();
        jumpHitbox = GetComponentInChildren<BossJumpAttackHitbox>();
        attackManager = GetComponentInParent<BossAttackManagerTesting>();
        groundCheckHandler = GetComponentInParent<BossGroundCheckHandlerTesting>();
        flipHandler = GetComponentInParent<BossFlipHandlerTesting>();
        movementHandler = GetComponentInParent<BossMovementHandlerTesting>();  // Get reference to movement handler
    }

    public void TriggerJumpAttack()
    {
        if (!attackManager.IsAttacking())
        {
            attackManager.SetAttacking(true);
            targetPosition = attackManager.player.position;  // Access player position from attackManager
            CalculateJump();

            animator.SetTrigger("JumpAnticipation");
            StartCoroutine(JumpRoutine());
        }
    }

    private IEnumerator JumpRoutine()
    {
        yield return new WaitForSeconds(jumpAnticipationTime);

        // Access Rigidbody2D from the movement handler
        movementHandler.Rb.velocity = new Vector2(jumpSpeed, jumpForce);  // Use Rigidbody2D from the movement handler
        animator.SetTrigger("JumpUpwardMovement");

        yield return new WaitUntil(() => movementHandler.Rb.velocity.y <= 0);  // Wait until the boss starts coming down
        animator.SetTrigger("JumpLanding");

        yield return new WaitUntil(() => groundCheckHandler.IsGrounded());  // Wait until grounded

        if (!isSmashTriggered)
        {
            isSmashTriggered = true;
            animator.SetTrigger("JumpGroundSmash");
            jumpHitbox?.ActivateJumpAttackCollider(!flipHandler.IsFacingLeft);  // Use flipHandler to check facing direction
            cameraShake?.ShakeCameraJumpSmashAttack();

            // Wait for the smash duration before resetting
            float smashDuration = animator.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(smashDuration);
        }

        ResetState();
    }

    private void ResetState()
    {
        isSmashTriggered = false;
        attackManager.SetAttacking(false);  // Reset attacking flag
    }

    private void CalculateJump()
    {
        float gravity = Mathf.Abs(Physics2D.gravity.y);
        float height = Random.Range(jumpHeightMin, jumpHeightMax);

        jumpForce = Mathf.Sqrt(2 * gravity * height);
        float timeToPeak = jumpForce / gravity;
        float totalAirTime = timeToPeak * 2;

        float horizontalDistance = targetPosition.x - transform.position.x;
        jumpSpeed = horizontalDistance / totalAirTime;  // Calculate horizontal speed for the jump
    }
}
