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
<<<<<<< HEAD:Assets/Scripts/Scripts_David/Completet/Boss/BossAttacks/Manager/OLD/BossAttackManager.cs
<<<<<<< HEAD:Assets/Scripts/Scripts_David/Completet/Boss/BossAttacks/Manager/OLD/BossAttackManager.cs
    [SerializeField] private BossJumpAttackHitbox jumpAttackCollider; // New reference
    [SerializeField] private BossAOEAttackHitbox bossAOEAttack;
=======
>>>>>>> parent of 33b28b5 (Fixed_Layers_For_Player):Assets/Scripts/Scripts_David/Completet/Boss/BossAttackManager.cs
=======
>>>>>>> parent of 33b28b5 (Fixed_Layers_For_Player):Assets/Scripts/Scripts_David/Completet/Boss/BossAttackManager.cs

    [Header("Ranged Attack Settings")]
    [SerializeField] private GameObject spitParticlePrefab;
    [SerializeField] private float projectileSpeed;
    [SerializeField] private Transform spitSpawnPoint;
    [SerializeField] private float spitDelay;

    [Header("Jump Settings (Floats)")]
    public float jumpAnticipationTime = 0.7f;
    public float jumpHeightMin;
    public float jumpHeightMax;
<<<<<<< HEAD:Assets/Scripts/Scripts_David/Completet/Boss/BossAttacks/Manager/OLD/BossAttackManager.cs
<<<<<<< HEAD:Assets/Scripts/Scripts_David/Completet/Boss/BossAttacks/Manager/OLD/BossAttackManager.cs
    [SerializeField] private float groundSmashDuration = 0.5f; // Specific duration for ground smash
    [SerializeField] private float postSmashRecovery = 0.3f; // Time before boss can move after smash
=======
>>>>>>> parent of 33b28b5 (Fixed_Layers_For_Player):Assets/Scripts/Scripts_David/Completet/Boss/BossAttackManager.cs
=======
>>>>>>> parent of 33b28b5 (Fixed_Layers_For_Player):Assets/Scripts/Scripts_David/Completet/Boss/BossAttackManager.cs

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

    [Header("AOE Attack Components")]
    [SerializeField] private BossAOEAttack bossAOEAttack;  // Reference to BossAOEAttack script

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
<<<<<<< HEAD:Assets/Scripts/Scripts_David/Completet/Boss/BossAttacks/Manager/OLD/BossAttackManager.cs
<<<<<<< HEAD:Assets/Scripts/Scripts_David/Completet/Boss/BossAttacks/Manager/OLD/BossAttackManager.cs
        jumpAttackCollider = transform.Find("BossJumpAttackCollider")?.GetComponent<BossJumpAttackHitbox>();

        spitSpawnPoint = transform.Find("Spit_Position_Instantiaion");
        bossAOEAttack = GetComponentInChildren<BossAOEAttackHitbox>();

=======
        spitSpawnPoint = transform.Find("Spit_Position_Instantiaion");
=======
        spitSpawnPoint = transform.Find("Spit_Position_Instantiaion");
>>>>>>> parent of 33b28b5 (Fixed_Layers_For_Player):Assets/Scripts/Scripts_David/Completet/Boss/BossAttackManager.cs
        bossAOEAttack = GetComponentInChildren<BossAOEAttack>();  // Make sure this is correctly referenced

        if (bossAI == null) Debug.LogWarning("BossAI not found!");
        if (bossSpriteRenderer == null) Debug.LogWarning("BossSpriteRenderer not found!");
        if (cameraShake == null) Debug.LogWarning("CameraDeadZoneFollow not found!");
        if (comboAttackHitbox == null) Debug.LogWarning("BossComboAttackCollider not found or ComboAttackHitbox component missing!");
        if (spitSpawnPoint == null) Debug.LogWarning("Spit_Position_Instantiaion not found!");
        if (animator == null) Debug.LogWarning("Animator is not assigned!");
        if (spitParticlePrefab == null) Debug.LogWarning("SpitParticlePrefab is not assigned!");
        if (bossAOEAttack == null) Debug.LogWarning("BossAOEAttack component not found in children!");
<<<<<<< HEAD:Assets/Scripts/Scripts_David/Completet/Boss/BossAttacks/Manager/OLD/BossAttackManager.cs
>>>>>>> parent of 33b28b5 (Fixed_Layers_For_Player):Assets/Scripts/Scripts_David/Completet/Boss/BossAttackManager.cs
=======
>>>>>>> parent of 33b28b5 (Fixed_Layers_For_Player):Assets/Scripts/Scripts_David/Completet/Boss/BossAttackManager.cs
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
            AOEAttackBehavior();
        }
    }

    public void AOEAttackBehavior()
    {
        if (!bossAI.IsAttacking())
        {
            bossAI.SetAttacking(true);  // Set the boss as attacking
            animator.SetTrigger("AOEAttackTrigger");  // Trigger the AOE attack animation

            // Activate the AOE Attack Collider (spikes)
            if (bossAOEAttack != null)
            {
                bossAOEAttack.ActivateAOEAttack();  // Activate the AOE spikes
            }

            // After the AOE attack duration, reset the attack state and deactivate the collider
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
            cameraShake.ShakeCameraJumpSmashAttack();

            // **Wait for the smash animation to complete before resetting state**
            float smashDuration = animator.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(smashDuration);
        }

        ResetJumpState();
        isJumpingSmash = false;
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
