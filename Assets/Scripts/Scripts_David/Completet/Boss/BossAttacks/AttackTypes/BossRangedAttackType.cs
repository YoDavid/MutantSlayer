using System.Collections;
using UnityEngine;

public class BossRangedAttack : BossAttackBase
{
    [Header("Ranged Specific")]
    [SerializeField] private Transform _projectileSpawn;
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private string _animationTrigger = "RangedAttackTrigger";
    [SerializeField] private float _projectileSpeed = 10f;
    [SerializeField] private float _attackDelay = 0.3f;

    public override void Execute()
    {
        if (IsExecuting) return;
        StartCoroutine(RangedRoutine());
    }

    private IEnumerator RangedRoutine()
    {
        IsExecuting = true;

        // Animation
        _animator.SetTrigger(_animationTrigger);

        // Wait for animation windup
        yield return new WaitForSeconds(_attackDelay);

        // Spawn projectile
        var projectile = Instantiate(_projectilePrefab, _projectileSpawn.position, Quaternion.identity);
        var direction = transform.right * (GetComponent<BossAI>().isFacingLeft ? -1 : 1);
        projectile.GetComponent<Rigidbody2D>().velocity = direction * _projectileSpeed;

        // Wait remaining duration
        yield return new WaitForSeconds(AttackDuration - _attackDelay);

        IsExecuting = false;
    }
}