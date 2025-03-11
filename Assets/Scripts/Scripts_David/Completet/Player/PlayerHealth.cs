using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100; // Maximum health value
    [SerializeField] private int currentHealth;

    [Header("Player Status")]
    public bool isDead = false; // To track if the player is dead

    // Reference to the Damage Blink script
    private PlayerDamageBlink damageBlink;
    private PlayerMovementController playerMovement; // Cached reference                                               
    private PlayerHurtbox playerHurtbox;


    void Start()
    {
        currentHealth = maxHealth;
        damageBlink = GetComponent<PlayerDamageBlink>();
        playerMovement = GetComponent<PlayerMovementController>(); // Cache the reference
        playerHurtbox = GetComponentInChildren<PlayerHurtbox>();
    }

    public void TakeDamage(int damage)
    {
        // Check if the player is dead or if the hurtbox is disabled (invincible)
        if (isDead || playerHurtbox == null || !playerHurtbox.enabled)
            return; // Prevent damage if the player is dead or invincible

        currentHealth -= damage;
        Debug.Log("Player took " + damage + " damage! Remaining health: " + currentHealth);

        // Trigger the blink effect on damage
        if (damageBlink != null)
        {
            damageBlink.TriggerBlinkEffect();
        }

        // Check if player health is 0 or less, trigger death logic
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public bool IsPlayerInvulnerable()
    {
        // Return true if the player is dashing (or any other invincible condition)
        return playerMovement.isDashing;
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("Player died!");
        // Handle death behavior (e.g., game over, respawn)
    }
}
