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

    [Header("Multi Hit Settings")]
    public float sizeThreshold = 0.3f; // Size threshold for multi-hit
    public int maxHits = 3; // Maximum number of hits
    public float timeSlowDuration = 3f; // Duration for time slow effect
    [Range(0.1f, 1f)] public float timeScaleDuringSlow = 0.5f; // Time scale during slow effect

    private float lifetime;
    private float currentSpeed;
    private Rigidbody2D rb;
    private bool isMovingRight; // Track movement direction
    private bool soundPlayed = false; // Track if sound has been played
    private int hitCount = 0; // Track number of hits
    private bool shouldSlowTime = false; // Track if time should be slowed
    private PauseMenuController pauseMenu; // Reference to pause menu

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentSpeed = baseSpeed;
        pauseMenu = FindObjectOfType<PauseMenuController>(); // Get reference to pause menu
    }

    private void Start()
    {
        // Play sound immediately when projectile is created
        PlayProjectileSound();

        // Check if we should slow time based on size
        shouldSlowTime = transform.localScale.x >= sizeThreshold;
        if (shouldSlowTime && !IsGamePaused())
        {
            // Start time slow if within the duration
            if (lifetime < timeSlowDuration)
            {
                TimeManager.Instance?.SetTimeScale(timeScaleDuringSlow);
                Time.fixedDeltaTime = 0.02f * Time.timeScale;
            }
        }
    }

    private void Update()
    {
        // Only update lifetime and check for destruction if game is not paused
        if (!IsGamePaused())
        {
            lifetime += Time.unscaledDeltaTime;

            if (shouldSlowTime && lifetime >= timeSlowDuration)
            {
                RestoreNormalTime();
                shouldSlowTime = false;
            }

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
    }

    private bool IsGamePaused()
    {
        return pauseMenu != null && pauseMenu.IsVisible;
    }

    private void RestoreNormalTime()
    {
        TimeManager.Instance?.ResetTimeScale();
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

        if (collision.CompareTag("Enemy") || collision.CompareTag("BossEnemy"))
        {
            bool isBoss = collision.CompareTag("BossEnemy");

            DamagePopUp.Instance?.CreateDamageText(
                damage,
                popupPosition,
                isPlayer: false,
                isBoss: isBoss,
                isCritical: isCritical
            );

            AudioManager.Instance.PlayProjectileHit();
            AudioManager.Instance.StopSound("PlayerOthers", "projectile_player");

            // Check if we should multi-hit
            if (transform.localScale.x >= sizeThreshold)
            {
                hitCount++;
                if (hitCount >= maxHits)
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnDestroy()
    {
        if (!IsGamePaused())
        {
            RestoreNormalTime();
        }
    }
}