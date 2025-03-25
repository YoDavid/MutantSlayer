using System.Collections;
using UnityEngine;

public class BossAOEAttackType : BossAttackBase
{
    [Header("AOE Specific")]
    [SerializeField] private BossAOEAttackHitbox _hitbox;
    [SerializeField] private string _animationTrigger = "AOEAttackTrigger";
    

    public override void Execute()
    {
        if (IsExecuting) return;
        StartCoroutine(AOERoutine());
    }

    private IEnumerator AOERoutine()
    {
        IsExecuting = true;

        // Visual/Audio
        _animator.SetTrigger(_animationTrigger);

        // Enable damage
        _hitbox.ActivateAOEAttack();

        yield return new WaitForSeconds(AttackDuration);

        // Clean up
        _hitbox.DeactivateAOEAttack();
        IsExecuting = false;
    }
}