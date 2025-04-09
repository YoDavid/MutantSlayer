using System.Collections;
using UnityEngine;

public class BossRangedAttackBehaviorTesting : MonoBehaviour
{
    public float attackDuration = 1f;  // Duration of the ranged attack, adjust as needed
    public GameObject spitPrefab;
    public Transform spitSpawnPoint;
    public Transform player;
    public float rangedAttackDelay = 2f;  // Time between ranged attacks
    public float projectileSpeed = 5f;

    private Animator animator;
    private BossAttackManagerTesting bossAttackManager;  // Reference to the new attack manager

    private void Awake()
    {
        animator = GetComponent<Animator>();
        bossAttackManager = GetComponent<BossAttackManagerTesting>();  // Get the attack manager
    }

    public void TriggerRangedAttack()
    {
        // Use the attack manager to check if the boss is already attacking
        if (!bossAttackManager.IsAttacking())
        {
            bossAttackManager.SetAttacking(true);  // Set attacking flag
            animator.SetTrigger("RangedAttackTrigger");  // Trigger the ranged attack animation
            StartCoroutine(SpitProjectileRoutine());
        }
    }

    private IEnumerator SpitProjectileRoutine()
    {
        // Implement delay for ranged attack
        yield return new WaitForSeconds(rangedAttackDelay);

        GameObject spit = Instantiate(spitPrefab, spitSpawnPoint.position, Quaternion.identity);
        SpitProjectile projectile = spit.GetComponent<SpitProjectile>();

        if (projectile)
        {
            bool isFacingLeft = transform.position.x > player.position.x;  // Determine facing direction based on player position
            projectile.SetDirection(isFacingLeft);
            StartCoroutine(MoveProjectile(spit, isFacingLeft ? Vector2.left : Vector2.right));
        }
        else
        {
            Debug.LogError("SpitProjectile component missing on spit prefab!");
        }

        // Reset attack state after the attack duration
        yield return new WaitForSeconds(attackDuration);
        bossAttackManager.SetAttacking(false);  // Reset the attacking flag
    }

    private IEnumerator MoveProjectile(GameObject spit, Vector2 direction)
    {
        while (spit != null)
        {
            spit.transform.Translate(direction * projectileSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
