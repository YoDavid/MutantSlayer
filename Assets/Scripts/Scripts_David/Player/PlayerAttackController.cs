using UnityEngine;

public class PlayerAttackController : MonoBehaviour
{
    private PlayerAnimationController animationController;
    private int attackCount = 0;
    private float lastAttackTime = 0f;
    private float attackResetTime = 0.5f; // Reset combo after this time if no X press
    private bool isAttacking = false;

    private void Awake()
    {
        animationController = GetComponent<PlayerAnimationController>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            PerformAttack();
        }

        // Reset the attack count if it's been long enough to reset
        if (isAttacking && Time.time - lastAttackTime > attackResetTime)
        {
            ResetAttack();
        }
    }

    private void PerformAttack()
    {
        // If the attack count reaches 3, reset it to 1 (looping the attack combo)
        if (attackCount >= 3)
        {
            attackCount = 1; // Restart combo from attack 1
        }
        else if (Time.time - lastAttackTime > attackResetTime)
        {
            attackCount = 1;  // Start a new combo
        }
        else
        {
            attackCount++; // Continue combo if within reset time
        }

        lastAttackTime = Time.time;
        isAttacking = true;
        animationController.SetAttackState(attackCount); // Update animation based on attack count
    }

    private void ResetAttack()
    {
        attackCount = 0;  // Reset combo count after reset time has passed
        isAttacking = false;  // Reset attacking state
        animationController.SetAttackState(0);  // Stop attack animation
    }
}
