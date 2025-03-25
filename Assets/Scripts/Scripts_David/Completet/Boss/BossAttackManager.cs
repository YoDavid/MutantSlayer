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
    [SerializeField] private BossJumpAttackHitbox bossJumpAttackHitbox;
    [SerializeField] private BossAOEAttack bossAOEAttack; 

    [Header("Ranged Attack Settings")]
    [SerializeField] private GameObject spitParticlePrefab;
    [SerializeField] private float projectileSpeed;
    [SerializeField] private Transform spitSpawnPoint;
    [SerializeField] private float spitDelay;

    [Header("Jump Settings (Floats)")]
    public float jumpAnticipationTime = 0.7f;
    public float jumpHeightMin;
    public float jumpHeightMax;

    [Header("Jump Target Position")]
    public Vector2 jumpTargetPosition;

    [Header("Jump Settings (Booleans & Flags)")]
    public bool isJumping = false;
    public bool isJumpingSmash = false;

    [Header("Jump Debug")]
    public float jumpForce;
    public float jumpHorizontalSpeed;
    public float jumpHeight;

    [Header("Jump Timer (Debug)")]
    public float jumpAttackDuration;
    private float jumpStartTime;
    private float jumpEndTime;

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
        comboAttackHitbox = transform.Find("BossComboAttackCollider")?.GetComponent<BossComboAttackHitbox>();
        spitSpawnPoint = transform.Find("Spit_Position_Instantiaion");
        bossAOEAttack = GetComponentInChildren<BossAOEAttack>(); 

    }

    private void Update()
    {
        if (isJumping)
        {
            jumpEndTime = Time.time;
            jumpAttackDuration = jumpEndTime - jumpStartTime;
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            JumpAttackBehavior();
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

    private IEnumerator InstantiateSpitAfterDelay()
    {
        yield return new WaitForSeconds(spitDelay);
        GameObject spit = Instantiate(spitParticlePrefab, spitSpawnPoint.position, Quaternion.identity);
        SpitProjectile spitProjectile = spit.GetComponent<SpitProjectile>();

        spitProjectile.SetDirection(bossAI.isFacingLeft);

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

    public void JumpAttackBehavior()
    {
        if (!bossAI.IsAttacking())
        {
            bossAI.SetAttacking(true);
            bossAI.currentState = BossState.Jumping;

            jumpTargetPosition = bossAI.player.position;

            CalculateJumpParameters();

            animator.SetTrigger("JumpAnticipation");
            StartCoroutine(JumpAnticipationRoutine());
        }
    }

    IEnumerator JumpAnticipationRoutine()
    {
        jumpStartTime = Time.time;
        yield return new WaitForSeconds(jumpAnticipationTime);

        bossAI.rb.velocity = new Vector2(jumpHorizontalSpeed, jumpForce);
        animator.SetTrigger("JumpUpwardMovement");

        yield return new WaitUntil(() => bossAI.rb.velocity.y <= 0);
        animator.SetTrigger("JumpLanding");

        yield return new WaitUntil(() => bossAI.isGrounded);

        if (!isJumpingSmash)
        {
            animator.SetTrigger("JumpGroundSmash");
            isJumpingSmash = true;
            bossJumpAttackHitbox.ActivateJumpAttackCollider(!bossAI.isFacingLeft);
            cameraShake.ShakeCameraJumpSmashAttack();

            float smashDuration = animator.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(smashDuration);
        }

        ResetJumpState();
        isJumpingSmash = false;
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
        bossAI.currentState = BossState.Idle; 
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
