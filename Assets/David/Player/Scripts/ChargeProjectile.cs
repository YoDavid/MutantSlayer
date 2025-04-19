using UnityEngine;

public class ChargeProjectile : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 10f;

    [Header("Lifetime")]
    public float maxLifetime = 5f;

    private float lifetime;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Launch(bool facingRight)
    {
        rb.velocity = (facingRight ? Vector2.right : Vector2.left) * speed;
    }

    private void Update()
    {
        lifetime += Time.deltaTime;
        if (lifetime >= maxLifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") || collision.CompareTag("BossEnemy"))
        {
            // Deal damage here
            // collision.GetComponent<EnemyHealth>()?.TakeDamage(damageAmount);

            Destroy(gameObject);
        }
    }
}
