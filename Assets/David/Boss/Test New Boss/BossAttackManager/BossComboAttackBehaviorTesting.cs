using System.Collections;
using UnityEngine;

public class BossComboAttackBehaviorTesting : MonoBehaviour
{
    private Animator animator;
    private BossAttackManagerTesting bossAttackManager;  // Reference to BossAttackManagerTesting
    private BossComboAttackHitbox comboHitbox;
    private BossAttackHandlerTesting bossAttackHandler;  // Reference to BossAttackHandlerTesting

    private void Awake()
    {
        animator = GetComponent<Animator>();
        bossAttackManager = GetComponentInParent<BossAttackManagerTesting>();  // Get reference to BossAttackManagerTesting
        comboHitbox = transform.Find("BossComboAttackCollider")?.GetComponent<BossComboAttackHitbox>();  // Find ComboAttackHitbox
        bossAttackHandler = GetComponentInParent<BossAttackHandlerTesting>();  // Get reference to BossAttackHandlerTesting
    }

    public void TriggerComboAttack()
    {
        if (!bossAttackManager.IsAttacking())
        {
            bossAttackManager.SetAttacking(true);  // Set attacking flag
            animator.SetTrigger("ComboAttackTrigger");  // Trigger the combo attack animation
            comboHitbox?.ActivateComboAttackCollider();  // Activate combo attack hitbox
            Invoke(nameof(ResetState), bossAttackHandler.comboAttackDuration);  // Use the duration from BossAttackHandlerTesting
        }
    }

    private void ResetState() => StartCoroutine(ResetAfterDelay());

    private IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(0.2f);
        bossAttackManager.SetAttacking(false);  // Reset the attacking flag
    }
}
