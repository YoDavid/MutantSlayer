using UnityEngine;

public class Spike : MonoBehaviour
{
    public DamageConfig damageConfig; // NEW: Replaces attackDamage
    private DamageDealer damageDealer; // NEW
    [SerializeField] private Collider2D spikeCollider; // Collider for this spike
    [SerializeField] private Collider2D playerHurtBoxCollider; // Reference to the player's hurtbox collider
    [SerializeField] private PlayerHealth playerHealth; // Reference to the player's health

    private void Start()
    {
        GameObject playerHurtbox = GameObject.FindWithTag("PlayerHurtBox");
        if (playerHurtbox != null)
        {
            playerHurtBoxCollider = playerHurtbox.GetComponent<Collider2D>();
            if (playerHurtBoxCollider == null)
            {
                Debug.LogError("Collider2D not found on PlayerHurtbox.", this);
            }
        }
        else
        {
            Debug.LogError("PlayerHurtbox not found in scene.", this);
        }

        // Find the player's health component
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth == null)
            {
                Debug.LogError("PlayerHealth component not found on Player.", this);
            }
        }
        else
        {
            Debug.LogError("Player not found in scene.", this);
        }
        damageDealer = gameObject.AddComponent<DamageDealer>(); // NEW
        damageDealer.config = damageConfig; // NEW

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the collided object is the player's hurtbox
        if (other == playerHurtBoxCollider)
        {
            ApplyDamage();
        }
    }

    private void ApplyDamage()
    {
        if (playerHealth != null && !playerHealth.IsPlayerInvulnerable())
        {
            var (damage, isCritical) = damageDealer.CalculateDamage(); // NEW
            playerHealth.TakeDamage(damage, isCritical); // MODIFIED
        }
    }

    public void SetColliderEnabled(bool enabled)
    {
        if (spikeCollider != null)
        {
            spikeCollider.enabled = enabled;
        }
        else
        {
            Debug.LogError("Collider2D is not assigned.", this);
        }
    }
}