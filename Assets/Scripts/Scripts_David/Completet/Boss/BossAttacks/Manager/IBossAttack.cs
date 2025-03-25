using UnityEngine;

public interface IBossAttack
{
    // Required methods/properties all attacks must implement
    void Execute();
    float AttackDuration { get; }
    bool IsExecuting { get; }

    // Optional common methods
    void CancelAttack();
    void Initialize(Animator animator);
}