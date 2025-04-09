using System.Collections;
using UnityEngine;

public class BossAOEAttackBehaviorTesting : MonoBehaviour
{
    private Animator animator;
    private BossAttackManagerTesting bossAttackManager;  // Reference to the new attack manager
    private BossAOEAttack aoeAttack;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        bossAttackManager = GetComponent<BossAttackManagerTesting>();  // Get the new Attack Manager
        aoeAttack = GetComponentInChildren<BossAOEAttack>();  // Get the AOE attack component
    }

    public void TriggerAOEAttack()
    {
        // Use the new BossAttackManager to check if the boss can attack
        if (!bossAttackManager.IsAttacking())
        {
            bossAttackManager.SetAttacking(true);  // Set attacking flag
            animator.SetTrigger("AOEAttackTrigger");  // Trigger the AOE attack animation
            aoeAttack?.ActivateAOEAttack();  // Activate the AOE attack

            // Use the attack manager's AOE attack duration for timing
            Invoke(nameof(ResetState), bossAttackManager.GetAOEAttackDuration());
        }
    }

    private void ResetState()
    {
        StartCoroutine(ResetAfterDelay());
    }

    private IEnumerator ResetAfterDelay()
    {
        // Small delay before resetting attack state to ensure it finishes correctly
        yield return new WaitForSeconds(0.2f);
        bossAttackManager.SetAttacking(false);  // Reset the attacking flag in the attack manager
    }
}
