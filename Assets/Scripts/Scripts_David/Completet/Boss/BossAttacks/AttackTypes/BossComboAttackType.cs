using System.Collections;
using UnityEngine;

public class BossComboAttackType : BossAttackBase
{
    [Header("Combo Components")]
    [SerializeField] private BossComboAttackHitbox _hitbox;
    [SerializeField] private string _animationTrigger = "ComboAttackTrigger";
    [SerializeField] private float[] _comboTimings = { 0.2f, 0.4f, 0.6f }; // Example timings

    private bool _hitboxActive = false;

    public override void Execute()
    {
        if (IsExecuting) return;
        StartCoroutine(ComboSequence());
    }

    private IEnumerator ComboSequence()
    {
        IsExecuting = true;

        // Start animation
        _animator.SetTrigger(_animationTrigger);

        // Start hitbox sequence
        _hitbox.ActivateComboAttackCollider(_comboTimings);

        // Wait for full attack duration
        yield return new WaitForSeconds(AttackDuration);

        // Ensure hitbox is disabled
        _hitbox.ForceDisable();
        IsExecuting = false;
    }

    public override void CancelAttack()
    {
        base.CancelAttack();
        _hitbox.ForceDisable();
    }
}