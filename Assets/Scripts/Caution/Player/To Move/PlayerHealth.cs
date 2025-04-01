using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100; 
    [SerializeField] private int currentHealth;

    [Header("Player Status")]
    public bool isDead = false; 

    private PlayerDamageBlink damageBlink;
    private PlayerMovementController playerMovement;                                            
    private PlayerHurtbox playerHurtbox;


    void Start()
    {
        currentHealth = maxHealth;
        damageBlink = GetComponent<PlayerDamageBlink>();
        playerMovement = GetComponent<PlayerMovementController>(); 
        playerHurtbox = GetComponentInChildren<PlayerHurtbox>();
    }

    public void TakeDamage(int damage)
    {
        if (isDead || playerHurtbox == null || !playerHurtbox.enabled)
            return; 

        currentHealth -= damage;

        if (damageBlink != null)
        {
            damageBlink.TriggerBlinkEffect();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public bool IsPlayerInvulnerable()
    {
      
        return playerMovement.isDashing;
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("Player died!");
    }
}
