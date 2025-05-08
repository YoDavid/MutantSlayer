using UnityEngine;

public class GroundCheckController : MonoBehaviour
{
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    public bool IsGrounded { get; private set; }

    private PlayerAnimationController playerAnimationController;

    private void Awake()
    {
        playerAnimationController = GetComponent<PlayerAnimationController>();
    }

    private void Update()
    {
        CheckIfGrounded();
    }

    public void CheckIfGrounded()
    {
        IsGrounded = Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckDistance, groundLayer);
        playerAnimationController.SetGroundedState(IsGrounded);
    }

    private void OnDrawGizmos()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(groundCheckPoint.position, 0.1f);
            Gizmos.DrawLine(groundCheckPoint.position, groundCheckPoint.position + Vector3.down * groundCheckDistance);
        }
    }
}
