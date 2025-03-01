using System.Collections;
using UnityEngine;

public class PlayerAttackController : MonoBehaviour
{
    private PlayerAnimationController animationController;

    [Header("Attack Debugging")]
    [SerializeField, HideInInspector] private int attackCount = 0;
    [SerializeField, HideInInspector] private float lastAttackTime = 0f;
    [SerializeField, HideInInspector] private bool isAttacking = false;

    [Header("Attack Settings")]
    [SerializeField] private float attackResetTime = 0.8f; // Time before combo resets
    [SerializeField] private float attackCooldownTime = 0.3f; // Minimum delay between attacks
    [SerializeField, Tooltip("Duration for each attack in the combo")] private float[] attackDurations = { 0.4f, 0.35f, 0.3f }; // Duration per attack

    public bool IsAttacking => isAttacking;

    private void Awake()
    {
        animationController = GetComponent<PlayerAnimationController>();
        if (animationController == null)
        {
            Debug.LogError("PlayerAnimationController component is missing on this GameObject.");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X) && CanAttack())
        {
            PerformAttack();
        }

        // Reset attack state if the player hasn't attacked for a while
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
        if (Time.time - lastAttackTime > attackResetTime || attackCount >= attackDurations.Length)
        {
            attackCount = 0; // Start new combo
        }

        attackCount++; // Increment attack count
        lastAttackTime = Time.time;
        isAttacking = true;

        animationController.SetAttackState(attackCount);

        // Reset animator to idle after the attack's duration to prevent looping
        StartCoroutine(ResetAnimatorAfterAttack(attackDurations[attackCount - 1]));
    }

    private IEnumerator ResetAnimatorAfterAttack(float delay)
    {
        yield return new WaitForSeconds(delay);
        animationController.SetAttackState(0); // Reset animator's AttackCount
    }

    private void ResetAttack()
    {
        attackCount = 0;
        isAttacking = false;
        animationController.SetAttackState(0);
    }

    private void OnValidate()
    {
        if (attackDurations.Length == 0)
        {
            Debug.LogWarning("attackDurations array is empty. Add durations for each attack.");
        }
    }
}