using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBase : MonoBehaviour
{
    // Encapsulation: Use private fields with public properties for controlled access
    private float maxHP = 100f;
    private float currentHP;

    private float maxStamina = 100f;
    private float currentStamina;

    // Properties to expose HP and stamina with read-only or controlled access
    public float CurrentHP
    {
        get { return currentHP; }
        private set { currentHP = Mathf.Clamp(value, 0, maxHP); }
    }

    public float CurrentStamina
    {
        get { return currentStamina; }
        private set { currentStamina = Mathf.Clamp(value, 0, maxStamina); }
    }

    // Properties to get max values
    public float MaxHP => maxHP;
    public float MaxStamina => maxStamina;

    // Unity methods
    protected virtual void Start()
    {
        // Initialize HP and stamina to their maximum values at the start
        currentHP = maxHP;
        currentStamina = maxStamina;
    }

    // Methods to manage HP
    public void TakeDamage(float damage)
    {
        if (damage < 0) return; // Ignore negative damage values
        CurrentHP -= damage;
        Debug.Log($"Took {damage} damage. Current HP: {CurrentHP}");

        if (CurrentHP <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (amount < 0) return; // Ignore negative heal values
        CurrentHP += amount;
        Debug.Log($"Healed {amount}. Current HP: {CurrentHP}");
    }

    // Methods to manage stamina
    public void UseStamina(float amount)
    {
        if (amount < 0) return; // Ignore negative usage values
        if (currentStamina >= amount)
        {
            CurrentStamina -= amount;
            Debug.Log($"Used {amount} stamina. Current Stamina: {CurrentStamina}");
        }
        else
        {
            Debug.Log("Not enough stamina!");
        }
    }

    public void RegainStamina(float amount)
    {
        if (amount < 0) return; // Ignore negative regain values
        CurrentStamina += amount;
        Debug.Log($"Regained {amount} stamina. Current Stamina: {CurrentStamina}");
    }

    // Virtual method for character death (can be overridden by child classes)
    protected virtual void Die()
    {
        Debug.Log("Character has died.");
        // Implement additional death logic (e.g., respawn, game over screen, etc.)
    }
}
