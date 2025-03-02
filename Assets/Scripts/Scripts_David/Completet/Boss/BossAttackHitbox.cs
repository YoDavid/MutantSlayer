using System.Collections;
using UnityEngine;

public class BossAttackHitbox : MonoBehaviour
{
    [SerializeField] private int attackDamage = 10; // Damage per hit
    [SerializeField] private float comboAttackStartTime = 0.2f; // Delay before the first hit
    [SerializeField] private float comboAttackDuration = 0.1f; // Duration to keep the collider active during each attack
    private Collider2D attackCollider; // The collider for the attack hitbox

    private int currentComboHits = 0; // To track how many hits have been applied

    private void Awake()
    {
        attackCollider = GetComponent<Collider2D>();
        if (attackCollider != null)
        {
            attackCollider.enabled = false; // Ensure the collider is initially disabled
        }
        else
        {
            Debug.LogError("Attack Collider is not attached to the BossAttackHitbox object.");
        }
    }

    // Activate the combo attack hitbox for a series of attacks
    public void ActivateComboAttackCollider()
    {
        currentComboHits = 0; // Reset the combo hits counter before starting
        StartCoroutine(ActivateComboWithIntervals());
    }

    // Coroutine to manage multiple activations of the collider
    private IEnumerator ActivateComboWithIntervals()
    {
        attackCollider.enabled = true; // Enable the collider for the whole combo duration

        // The total duration of the combo is 1.3 seconds, divided into 3 hits
        float intervalBetweenHits = 1.3f / 3f; // Divide the 1.3s into 3 intervals

        for (int i = 0; i < 3; i++)
        {
            // Wait for the interval before applying damage
            yield return new WaitForSeconds(i == 0 ? comboAttackStartTime : intervalBetweenHits); // Wait before the first hit and then between each subsequent hit

            // Apply damage if the collider is active
            if (currentComboHits < 3)
            {
                attackCollider.enabled = true; // Ensure the collider is active during each damage application

                // Apply damage to the player
                PlayerHealth playerHealth = GameObject.FindWithTag("Player").GetComponent<PlayerHealth>(); // Assuming you can find the player this way
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(attackDamage);
                    currentComboHits++; // Increment the hit counter
                }
            }
        }

        attackCollider.enabled = false; // Disable the collider after all hits
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Optional: If you need the collider to check when the player is hit (you might not need this now since we're controlling damage directly)
        if (other.CompareTag("Player"))
        {
            Debug.Log("Boss hit the player!");
        }
    }
}
