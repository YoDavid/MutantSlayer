using System.Collections;
using UnityEngine;

public class SpitProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public DamageConfig damageConfig;
    private DamageDealer damageDealer;
    [SerializeField] private float speed = 50f;

    // These will be assigned during runtime
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Collider2D playerHurtBoxCollider;

    [Header("Scaling Settings")]
    [SerializeField] private float growDuration = 1.5f;
    [SerializeField] private Vector3 initialScale = new Vector3(0.2f, 0.2f, 0.2f);
    [SerializeField] private Vector3 maxScale = new Vector3(1f, 1f, 1f);


    private float moveDirection; 
    private SpriteRenderer spriteRenderer; 

    void Awake()
    {
        GameObject player = GameObject.FindWithTag("Player");

        playerHealth = player.GetComponent<PlayerHealth>();

        GameObject hurtBox = GameObject.FindWithTag("PlayerHurtBox");

        playerHurtBoxCollider = hurtBox.GetComponent<Collider2D>();

        spriteRenderer = GetComponent<SpriteRenderer>();

        transform.localScale = initialScale;
    }

    private void Start()
    {
        StartCoroutine(GrowProjectile());
        StartCoroutine(DestroyAfterLifetime());

        damageDealer = gameObject.AddComponent<DamageDealer>();
        damageDealer.config = damageConfig;
    }


    private void Update()
    {
        transform.Translate(moveDirection * speed * Time.deltaTime, 0, 0);
    }

    public void SetDirection(bool isFacingLeft)
    {
        moveDirection = isFacingLeft ? -1f : 1f;

        spriteRenderer.flipX = !isFacingLeft;
    }

    private IEnumerator GrowProjectile()
    {
        float elapsedTime = 0f;

        while (elapsedTime < growDuration)
        {
            transform.localScale = Vector3.Lerp(initialScale, maxScale, elapsedTime / growDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localScale = maxScale;
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == playerHurtBoxCollider)
        {
            if (playerHealth != null)
            {
                var (damage, isCritical) = damageDealer.CalculateDamage();
                playerHealth.TakeDamage(damage, isCritical); 
            }
            Destroy(gameObject);
        }
    }

    private IEnumerator DestroyAfterLifetime()
    {
        yield return new WaitForSeconds(5f);
        Destroy(gameObject);
    }

}
