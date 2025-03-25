using UnityEngine;

public class SmallEnemyHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 30;
    public int currentHealth;
    private Animator animator;

    public event System.Action OnDeath;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            animator.SetTrigger("Hurt");
        }
    }

    private void Die()
    {
        animator.SetTrigger("Die");
        GetComponent<Collider2D>().enabled = false;
        OnDeath?.Invoke();
    }

    public void DestroyEnemy()
    {
        Destroy(gameObject);
    }
}