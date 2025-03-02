using UnityEngine;

public class BossHealth : MonoBehaviour
{
    public int health = 100;

    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log("Boss took " + damage + " damage! Remaining health: " + health);

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Boss Defeated!");
        Destroy(gameObject); // Replace with death animation logic
    }
}
