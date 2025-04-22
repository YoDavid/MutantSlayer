using UnityEngine;

public class ChargeProjectile : MonoBehaviour
{
    [Header("Movement Settings")]
    public float baseSpeed = 10f; // Base speed of the projectile
    public float acceleration = 1f; // Rate of speed increase
    public float maxSpeed = 20f; // Maximum speed the projectile can reach

    [Header("Lifetime")]
    public float maxLifetime = 5f;

    [Header("Popup Settings")]
    public int damage = 10;
    public bool isCritical = false;

    private float lifetime;
    private float currentSpeed;
    private Rigidbody2D rb;
    private bool isMovingRight; // Track movement direction
    private bool soundPlayed = false; // Track if sound has been played

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentSpeed = baseSpeed;
    }

    private void Start()
    {
        // Play sound immediately when projectile is created
        PlayProjectileSound();
    }

    private void Update()
    {
        lifetime += Time.deltaTime;
        if (lifetime >= maxLifetime)
        {
            Destroy(gameObject);
        }

        // Gradually increase the speed over time
        currentSpeed = Mathf.Min(baseSpeed + (acceleration * lifetime), maxSpeed);

        // Update velocity while maintaining direction
        float direction = isMovingRight ? 1 : -1;
        rb.velocity = new Vector2(currentSpeed * direction, rb.velocity.y);
    }

    private void PlayProjectileSound()
    {
        if (!soundPlayed && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlayerProjectileSound();
            soundPlayed = true;
        }
    }

    public void Launch(bool isFacingRight)
    {
        isMovingRight = isFacingRight; // Store the direction

        // Flip the sprite based on direction
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (isMovingRight ? 1 : -1);
        transform.localScale = scale;

        // Set initial velocity
        float direction = isMovingRight ? 1 : -1;
        rb.velocity = new Vector2(currentSpeed * direction, rb.velocity.y);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Vector3 popupPosition = collision.transform.position;

        if (collision.CompareTag("Enemy"))
        {
            DamagePopUp.Instance?.CreateDamageText(
                damage,
                popupPosition,
                isPlayer: false,
                isBoss: false,
                isCritical: isCritical
            );
            AudioManager.Instance.PlayProjectileHit();
            AudioManager.Instance.StopSound("PlayerOthers", "projectile_player");
            Destroy(gameObject);
        }
        else if (collision.CompareTag("BossEnemy"))
        {
            DamagePopUp.Instance?.CreateDamageText(
                damage,
                popupPosition,
                isPlayer: false,
                isBoss: true,
                isCritical: isCritical
            );
            AudioManager.Instance.PlayProjectileHit();
            AudioManager.Instance.StopSound("PlayerOthers", "projectile_player");
            Destroy(gameObject);
        }
    }
}