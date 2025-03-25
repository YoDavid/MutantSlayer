using UnityEngine;

public abstract class BossAttackBase : MonoBehaviour, IBossAttack
{
    [SerializeField] protected float _duration = 1f;
    public float AttackDuration => _duration;

    protected Animator _animator;
    public bool IsExecuting { get; protected set; }

    protected virtual void Awake()
    {
       
    }

    public virtual void Initialize(Animator animator)
    {
        _animator = animator;
    }

    public abstract void Execute();

    public virtual void CancelAttack()
    {
        IsExecuting = false;
    }
}