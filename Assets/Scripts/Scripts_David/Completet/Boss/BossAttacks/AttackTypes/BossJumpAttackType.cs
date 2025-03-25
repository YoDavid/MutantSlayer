using System.Collections;
using UnityEngine;

public class BossJumpAttack : BossAttackBase
{
    [Header("Jump Specific")]
    [SerializeField] private BossJumpAttackHitbox _hitbox;
    [SerializeField] private string _anticipationTrigger = "JumpAnticipation";
    [SerializeField] private string _landingTrigger = "JumpLanding";
    [SerializeField] private float _jumpHeight = 5f;
    [SerializeField] private float _anticipationTime = 0.7f;

    private Rigidbody2D _rb;
    private BossAI _bossAI;

    protected override void Awake()
    {
        base.Awake();
        _rb = GetComponent<Rigidbody2D>();
        _bossAI = GetComponent<BossAI>();
    }

    public override void Execute()
    {
        if (IsExecuting) return;
        StartCoroutine(JumpRoutine());
    }

    private IEnumerator JumpRoutine()
    {
        IsExecuting = true;

        // Anticipation phase
        _animator.SetTrigger(_anticipationTrigger);
        yield return new WaitForSeconds(_anticipationTime);

        // Calculate jump
        Vector2 target = _bossAI.player.position;
        float gravity = Mathf.Abs(Physics2D.gravity.y);
        float jumpForce = Mathf.Sqrt(2 * gravity * _jumpHeight);
        float airTime = (2 * jumpForce) / gravity;
        float horizontalSpeed = (target.x - transform.position.x) / airTime;

        // Launch
        _rb.velocity = new Vector2(horizontalSpeed, jumpForce);

        // Wait for landing
        yield return new WaitUntil(() => _bossAI.isGrounded);

        // Impact
        _animator.SetTrigger(_landingTrigger);
        _hitbox.ActivateJumpAttackCollider();
        CameraShake.Instance.ShakeCamera();

        yield return new WaitForSeconds(0.2f); // Impact duration

        // Clean up
        _hitbox.DisableCollider();
        IsExecuting = false;
    }
}