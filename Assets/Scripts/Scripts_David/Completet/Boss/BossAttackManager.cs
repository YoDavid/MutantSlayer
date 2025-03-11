using UnityEngine;

public class BossAttackManager : MonoBehaviour
{
    public Animator animator;
    private BossAI bossAI;

    // Reference to the BossAttackHitbox
    [SerializeField] private BossAttackHitbox comboAttackHitbox;

    // Expose combo attack duration to the Inspector
    [SerializeField] private float comboAttackDuration = 2.3f;

    void Start()
    {
        bossAI = GetComponent<BossAI>();

        if (comboAttackHitbox == null)
        {
            Debug.LogError("BossAttackHitbox reference is missing in BossAttackManager.");
        }
    }

    public void AOEAttackBehavior()
    {
        if (!bossAI.IsAttacking())
        {
            bossAI.SetAttacking(true);
            animator.SetTrigger("AOEAttackTrigger");
            Invoke(nameof(ResetAttackState), 1.5f);
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

            // Activate the combo attack hitbox
            comboAttackHitbox.ActivateComboAttackCollider();

            // Use the exposed variable for timing
            Invoke(nameof(ResetAttackState), comboAttackDuration);
        }
    }

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
