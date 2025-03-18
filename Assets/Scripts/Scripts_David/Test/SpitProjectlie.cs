using System.Collections;
using UnityEngine;

public class SpitProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private int damage = 10; // Damage to the player on collision
    [SerializeField] private float speed = 5f; // Speed of the projectile, exposed to the Inspector

    // These will be assigned during runtime
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Collider2D playerHurtBoxCollider;

    [Header("Scaling Settings")]
    [SerializeField] private float shrinkDuration = 3f; // Time in seconds to shrink the projectile
    [SerializeField] private Vector3 initialScale = new Vector3(1f, 1f, 1f); // Initial scale of the projectile (exposed for debugging)
    [SerializeField] private Vector3 minScale = new Vector3(0f, 0f, 0f); // The minimum scale of the projectile (exposed for debugging)

    private float moveDirection;  // To hold the direction of movement
    private SpriteRenderer spriteRenderer; // Reference to the sprite renderer for flipping

    void Awake()
    {
        // Find the player object by tag (this should be the parent object with the "Player" tag)
        GameObject player = GameObject.FindWithTag("Player");

        // Get PlayerHealth from the Player object (assuming it's attached to the parent)
        playerHealth = player.GetComponent<PlayerHealth>();

        // Find the PlayerHurtBoxCollider using the "PlayerHurtBox" tag
        GameObject hurtBox = GameObject.FindWithTag("PlayerHurtBox");

        // Get the collider from the PlayerHurtBox object (make sure it's the child object)
        playerHurtBoxCollider = hurtBox.GetComponent<Collider2D>();

        // Initialize spriteRenderer (for your projectile sprite flipping)
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Set the initial scale
        transform.localScale = initialScale;
    }

    // Set the direction of the projectile (left or right)
    public void SetDirection(bool isFacingLeft)
    {
        moveDirection = isFacingLeft ? -1f : 1f; // Left (negative) or right (positive)

        // Flip the sprite based on the direction (flip horizontally for right movement)
        spriteRenderer.flipX = !isFacingLeft;
    }

    private void Start()
    {
        // Start the shrinking coroutine
        StartCoroutine(ShrinkProjectile());
    }

    private void Update()
    {
        // Move the projectile in the specified direction at the given speed
        transform.Translate(moveDirection * speed * Time.deltaTime, 0, 0);
    }

    private IEnumerator ShrinkProjectile()
    {
        float elapsedTime = 0f;

        while (elapsedTime < shrinkDuration)
        {
            // Gradually shrink the projectile's scale over time
            transform.localScale = Vector3.Lerp(initialScale, minScale, elapsedTime / shrinkDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Once the shrinking is complete, ensure the scale reaches the minimum size
        transform.localScale = minScale;

        // Destroy the projectile when it reaches the minimum size
        Destroy(gameObject);
    }

    // Detect collision with player hurtbox
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == playerHurtBoxCollider) // Check if it collides with the player's hurtbox
        {
            // Apply damage to the player if the hurtbox is hit
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }

            // Destroy the projectile on collision
            Destroy(gameObject);
        }
    }
}
