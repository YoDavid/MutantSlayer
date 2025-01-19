using UnityEngine;

// Parent class for enemies
public class EnemyBase : MonoBehaviour
{
    // Encapsulation: Use private fields with public properties for controlled access
    private float maxHP = 50f;
    private float currentHP;

    private float damageOutput = 10f;

    // Properties to expose HP with controlled access
    public float CurrentHP
    {
        get { return currentHP; }
        private set { currentHP = Mathf.Clamp(value, 0, maxHP); }
    }

    public float MaxHP => maxHP;
    public float DamageOutput => damageOutput;

    // Unity methods
    protected virtual void Start()
    {
        // Initialize HP to its maximum value at the start
        currentHP = maxHP;
    }

    // Methods to manage HP
    public void TakeDamage(float damage)
    {
        if (damage < 0) return; // Ignore negative damage values
        CurrentHP -= damage;
        Debug.Log($"Enemy took {damage} damage. Current HP: {CurrentHP}");

        if (CurrentHP <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (amount < 0) return; // Ignore negative heal values
        CurrentHP += amount;
        Debug.Log($"Enemy healed {amount}. Current HP: {CurrentHP}");
    }

    // Virtual method for enemy attack (can be overridden by child classes)
    public virtual void Attack(GameObject target)
    {
        Debug.Log($"Enemy attacks {target.name} with {damageOutput} damage.");
        // Implement attack logic (e.g., reduce the target's HP)
    }

    // Virtual method for enemy death (can be overridden by child classes)
    protected virtual void Die()
    {
        Debug.Log("Enemy has died.");
        Destroy(gameObject); // Default behavior: destroy the enemy GameObject
    }
}
