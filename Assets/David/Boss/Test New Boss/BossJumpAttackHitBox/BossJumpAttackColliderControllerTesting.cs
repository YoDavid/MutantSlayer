using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BossJumpAttackColliderControllerTesting : MonoBehaviour
{
    [Header("Collider Settings")]
    [SerializeField] private float colliderShift = 0.5f;

    private Collider2D attackCollider;
    private Vector2 originalOffset;

    private void Awake()
    {
        attackCollider = GetComponent<Collider2D>();
        attackCollider.enabled = false;

        if (attackCollider is BoxCollider2D box)
            originalOffset = box.offset;
    }

    public void EnableCollider() => attackCollider.enabled = true;

    public void DisableCollider() => attackCollider.enabled = false;

    public void FlipCollider(bool isFlipped)
    {
        if (attackCollider is BoxCollider2D box)
        {
            box.offset = isFlipped
                ? new Vector2(originalOffset.x + colliderShift, originalOffset.y)
                : originalOffset;
        }
    }

    private void OnDrawGizmos()
    {
        if (attackCollider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(attackCollider.bounds.center, attackCollider.bounds.size);
        }
    }
}
