// BossAttackCoordinator.cs
using UnityEngine;

public class BossAttackCoordinator : MonoBehaviour
{
    [Header("Attack Modules")]
    [SerializeField] private IBossAttack _comboAttack;
    [SerializeField] private IBossAttack _aoeAttack;
    [SerializeField] private IBossAttack _rangedAttack;
    [SerializeField] private IBossAttack _jumpAttack;

    private void Awake()
    {
        var animator = GetComponent<Animator>();

        (_comboAttack as MonoBehaviour)?.GetComponent<IBossAttack>()?.Initialize(animator);
        (_aoeAttack as MonoBehaviour)?.GetComponent<IBossAttack>()?.Initialize(animator);
    }

    public void ExecuteAttack(BossState attackType)
    {
        var attack = GetAttack(attackType);
        if (attack == null || attack.IsExecuting) return;

        attack.Execute();
    }

    private IBossAttack GetAttack(BossState type)
    {
        return type switch
        {
            BossState.ComboAttack => _comboAttack,
            BossState.AOEAttack => _aoeAttack,
            BossState.RangedAttack => _rangedAttack,
            BossState.Jumping => _jumpAttack,
            _ => null
        };
    }
}