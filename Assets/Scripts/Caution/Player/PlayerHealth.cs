using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100; 
    public int currentHealth;

    [Header("Player Status")]
    public bool isDead = false; 

    private PlayerDamageBlink damageBlink;
    private PlayerMovementController playerMovement;                                            
    private PlayerHurtbox playerHurtbox;

    public event Action<int> OnHealthChanged;


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
        OnHealthChanged?.Invoke(currentHealth);

        if (DamagePopUp.Instance != null)
        {
            Vector3 popupPosition = transform.position + Vector3.up * 1.8f; 
            DamagePopUp.Instance.CreateDamageText( damage, popupPosition,   isPlayer: true,   isBoss: false);
        }

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

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(currentHealth);
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("Player died!");
    }
}
