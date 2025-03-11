using System.Collections;
using UnityEngine;

public class BossAttackManager : MonoBehaviour
{
    public Animator animator;
    private BossAI bossAI;

    // Reference to the BossAttackHitbox
    [SerializeField] private BossAttackHitbox comboAttackHitbox;

    void Start()
    {
        bossAI = GetComponent<BossAI>();
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

    public void RangedAttackBehavior()
    {
        if (!bossAI.IsAttacking())
        {
            bossAI.SetAttacking(true);
            animator.SetTrigger("RangedAttackTrigger");

            // Use the ranged attack duration from BossAI
            Invoke(nameof(ResetAttackState), bossAI.rangedAttackDuration);
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
