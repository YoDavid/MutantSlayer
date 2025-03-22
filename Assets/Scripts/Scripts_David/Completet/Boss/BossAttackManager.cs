using System.Collections;
using UnityEngine;

public class BossAttackManager : MonoBehaviour
{
    [Header("Boss Components")]
    public Animator animator;
    private BossAI bossAI;
    private SpriteRenderer bossSpriteRenderer;

    [Header("Attack Hitboxes")]
    [SerializeField] private BossAttackHitbox comboAttackHitbox;

    [Header("Ranged Attack Settings")]
    [SerializeField] private GameObject spitParticlePrefab;
    [SerializeField] private float projectileSpeed = 5f;
    [SerializeField] private Transform spitSpawnPoint;
    [SerializeField] private float spitDelay = 0.5f;

    [Header("Jump Settings (Booleans & Flags)")]
    public bool isJumping = false;            // Flag to indicate jump is in progress
    public bool isJumpingSmash = false;       // Flag to ensure smash is triggered only once

    [Header("Jump Settings (Floats)")]
    public float jumpAnticipationTime = 0.7f;  // Time before jump starts after trigger
    public float jumpForce;                   // Upward force applied when jumping
    public float jumpHorizontalSpeed;         // Controls side movement speed
    public float jumpHeightMin;               // Minimum jump height
    public float jumpHeightMax;               // Maximum jump height
    public float jumpHeight;                  // Actual jump height (calculated)

    [Header("Jump Target Position")]
    public Vector2 jumpTargetPosition;        // Position where the boss will jump towards

    [Header("Jump Timer (Debug)")]
    public float jumpAttackDuration;          // Duration measured and shown in Inspector
    private float jumpStartTime;              // Time when jump attack starts
    private float jumpEndTime;                // Time when jump attack ends


    void Start()
    {
        bossAI = GetComponent<BossAI>();
        bossSpriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // For debugging: update timer while jump is active
        if (isJumping)
        {
            jumpEndTime = Time.time;
            jumpAttackDuration = jumpEndTime - jumpStartTime;
        }

        // Other attack triggers:
        if (Input.GetKeyDown(KeyCode.K) && !bossAI.IsAttacking())
        {
            ComboAttackBehavior();
        }
        if (Input.GetKeyDown(KeyCode.R) && !bossAI.IsAttacking())
        {
            RangedAttackBehavior();
        }
        if (Input.GetKeyDown(KeyCode.P) && !bossAI.IsAttacking())
        {
            JumpAttackBehavior();
        }
    }

    // 2. Attack Behaviors

    public void AOEAttackBehavior()
    {
        if (!bossAI.IsAttacking())
        {
            bossAI.SetAttacking(true);
            animator.SetTrigger("AOEAttackTrigger");
            Invoke(nameof(ResetAttackState), bossAI.aoeAttackDuration);
        }
    }

    public void ComboAttackBehavior()
    {
        if (!bossAI.IsAttacking())
        {
            bossAI.SetAttacking(true);
            animator.SetTrigger("ComboAttackTrigger");
            comboAttackHitbox.ActivateComboAttackCollider();
            Invoke(nameof(ResetAttackState), bossAI.comboAttackDuration);
            Debug.Log("Combo Attack: Timer Has Reset");
        }
    }

    public void RangedAttackBehavior()
    {
        if (!bossAI.IsAttacking())
        {
            bossAI.SetAttacking(true);
            animator.SetTrigger("RangedAttackTrigger");
            StartCoroutine(InstantiateSpitAfterDelay());
            Invoke(nameof(ResetAttackState), bossAI.rangedAttackDuration);
        }
    }

    // 3. Jump Attack Behavior (All Logic in One Method)
    public void JumpAttackBehavior()
    {
        if (!bossAI.IsAttacking())
        {
            bossAI.SetAttacking(true);
            bossAI.currentState = BossState.Jumping;

            // **Save player's position before jumping**
            jumpTargetPosition = bossAI.player.position;

            // **Calculate Jump Force and Horizontal Speed dynamically**
            CalculateJumpParameters();

            animator.SetTrigger("JumpAnticipation");
            StartCoroutine(JumpAnticipationRoutine());
        }
    }

    // Single coroutine that handles anticipation, upward movement, landing, and smash
    IEnumerator JumpAnticipationRoutine()
    {
        jumpStartTime = Time.time;
        yield return new WaitForSeconds(jumpAnticipationTime);

        // **Apply the calculated jump force and speed**
        bossAI.rb.velocity = new Vector2(jumpHorizontalSpeed, jumpForce);
        animator.SetTrigger("JumpUpwardMovement");

        // Wait until the boss starts falling
        yield return new WaitUntil(() => bossAI.rb.velocity.y <= 0);
        animator.SetTrigger("JumpLanding");

        // Wait until the boss touches the ground
        yield return new WaitUntil(() => bossAI.isGrounded);

        // **Trigger the Smash Attack immediately upon landing**
        if (!isJumpingSmash)
        {
            animator.SetTrigger("JumpGroundSmash");
            isJumpingSmash = true;

            // **Wait for the smash animation to complete before resetting state**
            float smashDuration = animator.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(smashDuration);
        }

        // **Reset state after the smash attack finishes**
        ResetJumpState();
        isJumpingSmash = false;
    }

    // 4. Helper Methods

    private IEnumerator InstantiateSpitAfterDelay()
    {
        yield return new WaitForSeconds(spitDelay);
        GameObject spit = Instantiate(spitParticlePrefab, spitSpawnPoint.position, Quaternion.identity);
        SpitProjectile spitProjectile = spit.GetComponent<SpitProjectile>();
        if (spitProjectile != null)
        {
            spitProjectile.SetDirection(bossAI.isFacingLeft);
        }
        StartCoroutine(MoveProjectile(spit, bossAI.isFacingLeft ? Vector2.left : Vector2.right));
    }

    private IEnumerator MoveProjectile(GameObject projectile, Vector2 direction)
    {
        while (projectile != null)
        {
            projectile.transform.Translate(direction * projectileSpeed * Time.deltaTime);
            yield return null;
        }
    }

    private void ResetAttackState()
    {
        StartCoroutine(ResetAfterDelay());
    }

    private IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(0.2f);
        bossAI.SetAttacking(false);
        float distanceToPlayer = Vector2.Distance(bossAI.transform.position, bossAI.player.position);
        bossAI.currentState = (distanceToPlayer < bossAI.attackRange) ? BossState.Idle : BossState.Moving;
        bossAI.attackCooldownTimer = Random.Range(bossAI.minAttackTime, bossAI.maxAttackTime);
    }

    private void ResetJumpState()
    {
        bossAI.SetAttacking(false);
        bossAI.currentState = BossState.Idle; // Ensure the boss returns to Idle after the jump
        bossAI.attackCooldownTimer = Random.Range(bossAI.minAttackTime, bossAI.maxAttackTime);
    }

    private void CalculateJumpParameters()
    {
        // Randomly pick a jump height between the minimum and maximum values
        jumpHeight = Random.Range(jumpHeightMin, jumpHeightMax);

        // Get gravity from Unity's physics settings
        float gravity = Mathf.Abs(Physics2D.gravity.y);

        // Calculate initial vertical velocity needed to reach the desired height
        jumpForce = Mathf.Sqrt(2 * gravity * jumpHeight);

        // Calculate time to reach peak and total time in air
        float timeToPeak = jumpForce / gravity;
        float totalAirTime = timeToPeak * 2; // Up + Down

        // Calculate horizontal distance to the player's last position
        float distanceToPlayer = jumpTargetPosition.x - transform.position.x;

        // Calculate horizontal speed required to land at player's saved position
        jumpHorizontalSpeed = distanceToPlayer / totalAirTime;
    }
}
