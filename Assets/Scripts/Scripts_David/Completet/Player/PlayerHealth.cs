using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100; // Maximum health value
    private int currentHealth;

    [Header("Player Status")]
    public bool isDead = false; // To track if the player is dead

    // Reference to the Damage Blink script
    private PlayerDamageBlink damageBlink;

    void Start()
    {
        currentHealth = maxHealth;
        damageBlink = GetComponent<PlayerDamageBlink>(); // Get the Damage Blink script component

        if (damageBlink == null)
        {
            Debug.LogWarning("PlayerDamageBlink component not found! Blinking effect will not work.");
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return; // Prevent damage if the player is dead

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

    private void Die()
    {
        isDead = true;
        Debug.Log("Player died!");
        // Handle death behavior (e.g., game over, respawn)
    }
}
