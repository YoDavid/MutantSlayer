using UnityEngine;

public class PlayerAttackController : MonoBehaviour
{
    private PlayerAnimationController animationController;

    [Header("Attack Debugging")]
    [SerializeField] private int attackCount = 0;
    [SerializeField] private float lastAttackTime = 0f;
    [SerializeField] private bool isAttacking = false;

    [Header("Attack Settings")]
    [SerializeField] private float attackResetTime = 0.8f; // Time before combo resets
    [SerializeField] private float attackCooldownTime = 0.3f; // Minimum delay between attacks
    [SerializeField] private float[] attackDurations = { 0.4f, 0.35f, 0.3f }; // Duration per attack

    public bool IsAttacking => isAttacking;

    private void Awake()
    {
        animationController = GetComponent<PlayerAnimationController>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X) && CanAttack())
        {
            PerformAttack();
        }

        if (isAttacking && Time.time - lastAttackTime > attackResetTime)
        {
            ResetAttack();
        }
    }

    private bool CanAttack()
    {
        return Time.time - lastAttackTime >= attackCooldownTime;
    }

    private void PerformAttack()
    {
        if (attackCount >= attackDurations.Length)
        {
            attackCount = 1; // Restart combo
        }
        else if (Time.time - lastAttackTime > attackResetTime)
        {
            attackCount = 1; // Start new combo
        }
        else
        {
            attackCount++; // Continue combo
        }

        lastAttackTime = Time.time;
        isAttacking = true;

        animationController.SetAttackState(attackCount);

        // Only reset after last attack in the combo
        if (attackCount == attackDurations.Length)
        {
            Invoke(nameof(ResetAttack), attackDurations[attackCount - 1]);
        }
    }

    private void ResetAttack()
    {
        attackCount = 0;
        isAttacking = false;
        animationController.SetAttackState(0);
    }
}
