using UnityEngine;

public class PlayerAttackHitbox : MonoBehaviour
{
    [SerializeField] private int attackDamage = 10; // Set the damage per hit
    private BossHealth bossHealth; // Cached reference to BossHealth

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Debugging the tag to ensure it matches
        Debug.Log("Collider entered: " + other.tag);

        if (other.CompareTag("BossEnemy"))
        {
            if (bossHealth == null) // If we haven't already cached the component
            {
                bossHealth = other.GetComponent<BossHealth>();
                Debug.Log("BossHealth component found: " + (bossHealth != null)); // Debug if the BossHealth component was found
            }

            if (bossHealth != null)
            {
                Debug.Log("Hit the Boss!");
                bossHealth.TakeDamage(attackDamage);
            }
        }
    }
}
