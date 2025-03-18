using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;

    [Header("Debug Options")]
    [SerializeField] private bool debugSpeed = false; // Toggle for debugging speed
    [SerializeField] private float debugSpeedValue = 0.0f; // Debug speed value to use when debugging
    [SerializeField] private bool debugJumping = false; // Toggle for debugging jumping
    [SerializeField] private bool debugIsJumping = false; // Set the jump state manually when debugging

    [Header("Settings")]
    [SerializeField] private float speedThreshold = 0.1f; // Threshold for switching between idle and running
    private bool isJumping = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Calculate speed (or use debug speed)
        float speed = debugSpeed ? debugSpeedValue : Mathf.Abs(rb.velocity.x);

        // Update the "Speed" parameter in the Animator
        animator.SetFloat("Speed", speed);

        // Determine the jumping state (use debug state if enabled)
        if (debugJumping)
        {
            isJumping = debugIsJumping;
        }

        // Update the "IsJumping" parameter in the Animator
        animator.SetBool("IsJumping", isJumping);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the player lands on the object named "Circle"
        if (!debugJumping && collision.gameObject.name == "Circle")
        {
            isJumping = false; // Reset jump state
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // Detect when the player leaves the object named "Circle"
        if (!debugJumping && collision.gameObject.name == "Circle")
        {
            isJumping = true; // Set jump state
        }
    }
}
    