using UnityEngine;

public class BossGroundCheckHandlerTesting : MonoBehaviour
{
    [Header("Ground Check")]
    [SerializeField] private float groundCheckRadius;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;

    public bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckRadius, groundLayer);
        return hit.collider != null && hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground");
    }
}
