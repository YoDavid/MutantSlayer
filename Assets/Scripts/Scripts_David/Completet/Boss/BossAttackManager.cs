using UnityEngine;

public class BossAttackManager : MonoBehaviour
{
    public Animator animator;
    private BossAI bossAI;

    void Start()
    {
        bossAI = GetComponent<BossAI>();
    }

    // These methods mirror the test script’s attack behaviors.
    public void AOEAttackBehavior()
    {
        if (!bossAI.IsAttacking())
        {
            bossAI.SetAttacking(true);
            animator.SetTrigger("AOEAttackTrigger");
            Invoke(nameof(ResetAttackState), 1.5f); // Adjust timing as in the test script
        }
    }

    public void RangedAttackBehavior()
    {
        if (!bossAI.IsAttacking())
        {
            bossAI.SetAttacking(true);
            animator.SetTrigger("RangedAttackTrigger");
            Invoke(nameof(ResetAttackState), 1.5f);
        }
    }

    public void ComboAttackBehavior()
    {
        if (!bossAI.IsAttacking())
        {
            bossAI.SetAttacking(true);
            animator.SetTrigger("ComboAttackTrigger");
            Invoke(nameof(ResetAttackState), 2.0f);
        }
    }

    // ResetAttackState replicates the test script’s ResetAttackState:
    // it resets the isAttacking flag, updates the current state based on distance, and randomizes the cooldown.
    private void ResetAttackState()
    {
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
