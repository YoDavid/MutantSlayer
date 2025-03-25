using System.Collections;
using UnityEngine;

public class BossAttackManager : MonoBehaviour
{
    [Header("Camera Components")]
    private CameraShake cameraShake;

    [Header("Boss Components")]
    private Animator animator;
    private BossAI bossAI;
    private SpriteRenderer bossSpriteRenderer;

    [Header("Attack Hitboxes")]
    [SerializeField] private BossComboAttackHitbox comboAttackHitbox;
    [SerializeField] private BossJumpAttackHitbox jumpAttackCollider; // New reference
    [SerializeField] private BossAOEAttackHitbox bossAOEAttack;

    [Header("Ranged Attack Settings")]
    [SerializeField] private GameObject spitParticlePrefab;
    [SerializeField] private float projectileSpeed;
    [SerializeField] private Transform spitSpawnPoint;
    [SerializeField] private float spitDelay;

    [Header("Jump Attack Settings")]
    public float jumpAnticipationTime = 0.7f;
    public float jumpHeightMin;
    public float jumpHeightMax;
    [SerializeField] private float groundSmashDuration = 0.5f; // Specific duration for ground smash
    [SerializeField] private float postSmashRecovery = 0.3f; // Time before boss can move after smash

    [Header("Jump Target Position")]
    public Vector2 jumpTargetPosition;

    [Header("Jump State Flags")]
    public bool isJumping = false;
    public bool isJumpingSmash = false;
    public bool hasLanded = false;

    [Header("Jump Debug Info")]
    public float jumpForce;
    public float jumpHorizontalSpeed;
    public float jumpHeight;
    public float jumpAttackDuration;
    private float jumpStartTime;



    void Start()
    {
        AssignReferences();
    }

    private void AssignReferences()
    {
        bossAI = GetComponent<BossAI>();
        animator = GetComponent<Animator>();
        bossSpriteRenderer = GetComponent<SpriteRenderer>();
        cameraShake = FindObjectOfType<CameraShake>();

        // Find attack colliders
        comboAttackHitbox = transform.Find("BossComboAttackCollider")?.GetComponent<BossComboAttackHitbox>();
        jumpAttackCollider = transform.Find("BossJumpAttackCollider")?.GetComponent<BossJumpAttackHitbox>();

        spitSpawnPoint = transform.Find("Spit_Position_Instantiaion");
        bossAOEAttack = GetComponentInChildren<BossAOEAttackHitbox>();

    }

    private void Update()
    {
        if (isJumping)
        {
            jumpAttackDuration = Time.time - jumpStartTime;
        }
    }

    public void AOEAttackBehavior()
    {
        if (!bossAI.IsAttacking())
        {
            bossAI.SetAttacking(true);
            animator.SetTrigger("AOEAttackTrigger");

            if (bossAOEAttack != null)
            {
                bossAOEAttack.ActivateAOEAttack();
            }

            Invoke(nameof(ResetAttackState), bossAI.aoeAttackDuration);
        }
    }

    public void ComboAttackBehavior()
    {
        if (!bossAI.IsAttacking())
        {
            bossAI.SetAttacking(true);
            animator.SetTrigger("ComboAttackTrigger");
            //comboAttackHitbox.ActivateComboAttackCollider();
            Invoke(nameof(ResetAttackState), bossAI.comboAttackDuration);
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

    public void JumpAttackBehavior()
    {
        if (!bossAI.IsAttacking())
        {
            bossAI.SetAttacking(true);
            bossAI.currentState = BossState.Jumping;
            jumpTargetPosition = bossAI.player.position;
            CalculateJumpParameters();

            animator.SetTrigger("JumpAnticipation");
            StartCoroutine(JumpAttackSequence());
        }
    }

    IEnumerator JumpAttackSequence()
    {
        // Anticipation phase
        jumpStartTime = Time.time;
        yield return new WaitForSeconds(jumpAnticipationTime);

        // Jump upward phase
        bossAI.rb.velocity = new Vector2(jumpHorizontalSpeed, jumpForce);
        animator.SetTrigger("JumpUpwardMovement");
        isJumping = true;

        // Wait until starting to fall
        yield return new WaitUntil(() => bossAI.rb.velocity.y <= 0);
        animator.SetTrigger("JumpLanding");

        // Wait until landed
        yield return new WaitUntil(() => bossAI.isGrounded);
        hasLanded = true;

        // Ground smash phase
        if (!isJumpingSmash)
        {
            animator.SetTrigger("JumpGroundSmash");
            isJumpingSmash = true;
            cameraShake.ShakeCameraJumpSmashAttack();

            // Activate jump attack collider
            if (jumpAttackCollider != null)
            {
                jumpAttackCollider.ActivateJumpAttackCollider();
            }

            // Wait for smash to complete
            yield return new WaitForSeconds(groundSmashDuration);
        }

        // Recovery phase
        yield return new WaitForSeconds(postSmashRecovery);
        ResetJumpState();
    }

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
        isJumping = false;
        isJumpingSmash = false;
        hasLanded = false;
        bossAI.SetAttacking(false);
        bossAI.currentState = BossState.Idle;
        bossAI.attackCooldownTimer = Random.Range(bossAI.minAttackTime, bossAI.maxAttackTime);
    }

    private void CalculateJumpParameters()
    {
        jumpHeight = Random.Range(jumpHeightMin, jumpHeightMax);
        float gravity = Mathf.Abs(Physics2D.gravity.y);
        jumpForce = Mathf.Sqrt(2 * gravity * jumpHeight);

        float timeToPeak = jumpForce / gravity;
        float totalAirTime = timeToPeak * 2;

        float distanceToPlayer = jumpTargetPosition.x - transform.position.x;
        jumpHorizontalSpeed = distanceToPlayer / totalAirTime;
    }
}