using UnityEngine;

public class BossStateManager : MonoBehaviour
{
    public BossState CurrentState { get; private set; }
    public Vector2 StartingPosition { get; private set; }

    public void Initialize(Vector2 startPos)
    {
        StartingPosition = startPos;
        CurrentState = BossState.Idle;
    }

    public void ChangeState(BossState newState)
    {
        CurrentState = newState;
    }
}
