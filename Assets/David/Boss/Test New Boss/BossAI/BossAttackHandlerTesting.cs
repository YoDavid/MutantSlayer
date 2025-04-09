using System.Collections;
using UnityEngine;

public class BossAttackHandlerTesting : MonoBehaviour
{
    [Header("Attack Timings")]
    public float comboAttackDuration = 2f;
    public float rangedAttackDuration = 1.5f;
    public float aoeAttackDuration = 3f;

    [SerializeField] private BossAttackManagerTesting attackManager;

    void Start()
    {
        attackManager = GetComponentInChildren<BossAttackManagerTesting>();  // Get reference to the attack manager
    }

    public void ExecuteAttack(BossState currentState)
    {
        if (attackManager == null)
        {
            Debug.LogError("Attack Manager is not assigned!");
            return;
        }

        switch (currentState)
        {
            case BossState.ComboAttack:
                attackManager.ComboAttackBehavior();  // Call Combo Attack logic in BossAttackManagerTesting
                break;
            case BossState.RangedAttack:
                attackManager.RangedAttackBehavior();  // Call Ranged Attack logic in BossAttackManagerTesting
                break;
            case BossState.AOEAttack:
                attackManager.AOEAttackBehavior();  // Call AOE Attack logic in BossAttackManagerTesting
                break;
            default:
                break;
        }
    }
}
