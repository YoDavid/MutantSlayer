using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class BossAttackManager : MonoBehaviour
{
    // References to Components
    [Header("Boss Components")]
    public Animator animator;          // The animator for boss animations
    private BossAI bossAI;             // The BossAI component controlling the boss's behavior
    private SpriteRenderer bossSpriteRenderer; // Reference to the boss's sprite renderer

    // Reference to Boss's Attack Hitbox
    [Header("Attack Hitboxes")]
    [SerializeField] private BossAttackHitbox comboAttackHitbox; // The hitbox for the combo attack

    // Reference to Ranged Attack Particle Effect
    [Header("Ranged Attack Effects")]
    [SerializeField] private GameObject spitParticlePrefab;      // The particle effect prefab for the spit attack
    [SerializeField] private float projectileSpeed = 5f;         // Speed of the projectile

    // Reference to Spit Attack Spawn Position
    [Header("Spit Attack Spawn Position")]
    [SerializeField] private Transform spitSpawnPoint;

    // Time delay after the animation starts before instantiating the spit
    [Header("Spit Timing")]
    [SerializeField] private float spitDelay = 0.5f;  // Adjust this to fit your animation timing


    void Start()
    {
        bossAI = GetComponent<BossAI>();
        bossSpriteRenderer = GetComponent<SpriteRenderer>(); // Initialize sprite renderer
    }

    public void AOEAttackBehavior()
    {
        if (!bossAI.IsAttacking())
        {
            bossAI.SetAttacking(true);
            animator.SetTrigger("AOEAttackTrigger");

            // Use the AOE attack duration from BossAI
            Invoke(nameof(ResetAttackState), bossAI.aoeAttackDuration);
        }
    }
    public void ComboAttackBehavior()
    {
        if (!bossAI.IsAttacking())
        {
            bossAI.SetAttacking(true);
            animator.SetTrigger("ComboAttackTrigger");

            // Activate the combo attack hitbox
            comboAttackHitbox.ActivateComboAttackCollider();

            // Use the combo attack duration from BossAI
            Invoke(nameof(ResetAttackState), bossAI.comboAttackDuration);
        }
    }

    public void RangedAttackBehavior()
    {
        if (!bossAI.IsAttacking())
        {
            bossAI.SetAttacking(true);
            animator.SetTrigger("RangedAttackTrigger");

            // Start the delayed instantiation of the spit particle
            StartCoroutine(InstantiateSpitAfterDelay());

            // Use the ranged attack duration from BossAI
            Invoke(nameof(ResetAttackState), bossAI.rangedAttackDuration);
        }
    }


    private IEnumerator InstantiateSpitAfterDelay()
    {
        yield return new WaitForSeconds(spitDelay);

        // Instantiate the projectile with the correct direction
        GameObject spit = Instantiate(spitParticlePrefab, spitSpawnPoint.position, Quaternion.identity);

        // Get the SpitProjectile component and set the direction
        SpitProjectile spitProjectile = spit.GetComponent<SpitProjectile>();
        if (spitProjectile != null)
        {
            // Pass the boss's facing direction to SpitProjectile
            spitProjectile.SetDirection(bossAI.isFacingLeft); // Set direction based on the boss's facing direction
        }

        // Start moving the projectile (we are no longer using shootDirection here)
        StartCoroutine(MoveProjectile(spit, bossAI.isFacingLeft ? Vector2.left : Vector2.right));
    }

    private IEnumerator MoveProjectile(GameObject projectile, Vector2 direction)
    {
        while (projectile != null)
        {
            // Move the projectile in the set direction
            projectile.transform.Translate(direction * projectileSpeed * Time.deltaTime);
            yield return null; // Wait for the next frame
        }
    }

    private void ResetAttackState()
    {
        StartCoroutine(ResetAfterDelay());
    }

    private IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(0.2f); // Small buffer to ensure attack finishes properly

        bossAI.SetAttacking(false);
        float distanceToPlayer = Vector2.Distance(bossAI.transform.position, bossAI.player.position);
        if (distanceToPlayer < bossAI.attackRange)
        {
            bossAI.currentState = BossAI.BossState.Idle;
        }
        else
        {
            bossAI.currentState = BossAI.BossState.Moving;
        }
        bossAI.attackCooldownTimer = Random.Range(bossAI.minAttackTime, bossAI.maxAttackTime);
    }
}
