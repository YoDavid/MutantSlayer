using System.Collections;
using UnityEngine;

public class BossAttackManager : MonoBehaviour
{
    // === CORE REFERENCES ===
    [Header("Camera Components")]
    [SerializeField] private CameraShake cameraShake;

    [Header("Boss Components")]
    private Animator animator;
    private BossAI bossAI;
    private SpriteRenderer bossSpriteRenderer;

    // === ATTACK SYSTEM ===
    [Header("Attack Hitboxes")]
    [SerializeField] private BossComboAttackHitbox comboAttackHitbox;
    [SerializeField] private BossJumpAttackHitbox bossJumpAttackHitbox;
    [SerializeField] private BossAOEAttack bossAOEAttack;

    // === RANGED ATTACK SETTINGS ===
    [Header("Ranged Attack Settings")]
    [SerializeField] private GameObject spitParticlePrefab;
    [SerializeField] private float projectileSpeed;
    [SerializeField] private Transform spitSpawnPoint;
    [SerializeField] private float spitDelay;

    // === JUMP ATTACK SETTINGS ===
    [Header("Jump Settings (Anticipation & Timing)")]
    [SerializeField] private float jumpAnticipationTime = 0.7f;
    [SerializeField] private float timeOffscreenBeforeDrop = 2f;
    [SerializeField] private float maxTimeInAir = 3f;

    [Header("Jump Settings (Physics)")]
    [SerializeField] private float jumpHeight = 50f;
    [SerializeField] private float dropSpeed = -30f;

    [Header("Jump Settings (Booleans & Flags)")]
    public bool isJumping = false;
    public bool isJumpingSmash = false;

    [Header("Jump Target Position")]
    public Vector2 jumpTargetPosition;

    // === INTERNAL VALUES ===
    private float jumpStartTime;

    private Vector3 spitPositionFacingLeft = new Vector3(-40f, -21.4f, 0f);
    private Vector3 spitPositionFacingRight = new Vector3(40f, -21.4f, 0f);

    [Header("Falling Spikes")]
    [SerializeField] private FallingSpikeSpawner fallingSpikeSpawner;



    void Start()
    {
        AssignReferences();
    }

    private void AssignReferences()
    {
        bossAI = GetComponent<BossAI>();
        if (bossAI == null) Debug.LogError("BossAI component missing!");

        animator = GetComponent<Animator>();
        if (animator == null) Debug.LogError("Animator component missing!");

        bossSpriteRenderer = GetComponent<SpriteRenderer>();
        if (bossSpriteRenderer == null) Debug.LogError("SpriteRenderer component missing!");

        // Get camera shake - try both methods but prefer the one that works
        cameraShake = FindObjectOfType<CameraShake>();
        if (cameraShake == null)
        {
            cameraShake = Camera.main?.GetComponent<CameraShake>();
        }
        if (cameraShake == null) Debug.LogError("CameraShake component missing!");

        comboAttackHitbox = transform.Find("BossComboAttackCollider")?.GetComponent<BossComboAttackHitbox>();
        if (comboAttackHitbox == null) Debug.LogError("ComboAttackHitbox missing!");

        spitSpawnPoint = transform.Find("Spit_Position_Instantiaion");
        if (spitSpawnPoint == null) Debug.LogError("Spit spawn point missing!");

        bossAOEAttack = GetComponentInChildren<BossAOEAttack>();
        if (bossAOEAttack == null) Debug.LogError("BossAOEAttack missing!");

        fallingSpikeSpawner = FindAnyObjectByType<FallingSpikeSpawner>();
        if (fallingSpikeSpawner == null) Debug.LogError("FallingSpikeSpawner missing!");
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

        // 1. Instantiate the projectile first
        GameObject spit = Instantiate(spitParticlePrefab, spitSpawnPoint.position, Quaternion.identity);
        SpitProjectile spitProjectile = spit.GetComponent<SpitProjectile>();

        // 2. Calculate direction and set it
        bool isFacingLeft = transform.position.x > bossAI.player.position.x;
        if (spitProjectile != null)
        {
            spitProjectile.SetDirection(isFacingLeft);

            // 3. Start moving the projectile
            StartCoroutine(MoveProjectile(spit, isFacingLeft ? Vector2.left : Vector2.right));
        }
        else
        {
            Debug.LogError("SpitProjectile component missing on spit prefab!");
        }
    }

    public void UpdateSpitPosition(bool isFacingLeft)
    {
        if (spitSpawnPoint != null)
        {
            spitSpawnPoint.localPosition = isFacingLeft ? spitPositionFacingLeft : spitPositionFacingRight;
        }
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

            StartCoroutine(JumpAttackSequence());
        }
    }

    private IEnumerator JumpAttackSequence()
    {
        jumpStartTime = Time.time;
        bool forcedDrop = false;

        // Step 1: Anticipation before jump
        animator.SetTrigger("JumpAnticipation");
        yield return new WaitForSeconds(jumpAnticipationTime);

        // Step 2: Jump vertically out of screen
        float jumpForce = Mathf.Sqrt(2 * Mathf.Abs(Physics2D.gravity.y) * jumpHeight);
        bossAI.rb.velocity = new Vector2(0, jumpForce);
        animator.SetTrigger("JumpUpwardMovement");
        cameraShake.ShakeCameraAOEAttack();

        yield return new WaitUntil(() =>
        {
            if (Time.time - jumpStartTime > maxTimeInAir && bossAI.rb.velocity.y > 0)
            {
                forcedDrop = true;
                return true;
            }
            return bossAI.rb.velocity.y <= 0;
        });

        if (forcedDrop)
        {
            bossAI.rb.velocity = new Vector2(bossAI.rb.velocity.x, 0);
        }

        bossAI.rb.velocity = Vector2.zero;
        bossAI.rb.isKinematic = true;
        bossSpriteRenderer.enabled = false;

        yield return new WaitForSeconds(timeOffscreenBeforeDrop);

        jumpTargetPosition = bossAI.player.position;
        transform.position = new Vector3(jumpTargetPosition.x, transform.position.y, transform.position.z);
        bossSpriteRenderer.enabled = true;
        bossAI.rb.isKinematic = false;

        bossAI.rb.velocity = new Vector2(0, dropSpeed); // Now using the serialized dropSpeed
        animator.SetTrigger("JumpFalling");

        yield return new WaitUntil(() => bossAI.isGrounded);

        if (!isJumpingSmash)
        {
            animator.SetTrigger("JumpGroundSmash");
            isJumpingSmash = true;
            bossJumpAttackHitbox.ActivateJumpAttackCollider(!bossAI.isFacingLeft);
            cameraShake.ShakeCameraJumpSmashAttack();

            fallingSpikeSpawner?.SpawnSpikesSmash();

            float smashDuration = animator.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(smashDuration);
        }

        ResetJumpState();
    }



    private void ResetAttackState()
    {
        StartCoroutine(ResetAfterDelay());
    }

    private IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(0.2f);
        bossAI.SetAttacking(false);
    }

    private void ResetJumpState()
    {
        isJumpingSmash = false; // Reset early
        bossAI.SetAttacking(false);
        bossAI.currentState = BossState.Idle;
    }

}