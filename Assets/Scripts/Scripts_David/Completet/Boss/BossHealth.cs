using UnityEngine;

public class BossHealth : MonoBehaviour
{
    public float health = 100f;

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // Handle boss death (e.g., play death animation, etc.)
        Destroy(gameObject);
    }
}
