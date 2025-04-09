using UnityEngine;

public class BossColliderHandlerTesting : MonoBehaviour
{
    private Collider2D attackCollider;
    private Vector2 originalOffset;
    private float colliderShift;

    public BossColliderHandlerTesting(Collider2D collider, float shift)
    {
        attackCollider = collider;
        colliderShift = shift;
    }

    public void SetupCollider()
    {
        attackCollider.enabled = false;
        if (attackCollider is BoxCollider2D boxCollider)
        {
            originalOffset = boxCollider.offset;
        }
    }

    public void EnableCollider() => attackCollider.enabled = true;
    public void DisableCollider() => attackCollider.enabled = false;

    public void FlipCollider(bool isFlipped)
    {
        if (attackCollider is BoxCollider2D boxCollider)
        {
            boxCollider.offset = isFlipped
                ? new Vector2(originalOffset.x + colliderShift, originalOffset.y)
                : originalOffset;
        }
    }

    public Collider2D GetCollider() => attackCollider;
}
