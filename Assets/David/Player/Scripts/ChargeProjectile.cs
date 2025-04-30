using UnityEngine;
using System.Collections;

public class ChargeProjectile : MonoBehaviour
{
    #region Settings
    #region Movement
    [Header("Movement Settings")]
    public float baseSpeed = 10f;
    public float acceleration = 1f;
    public float maxSpeed = 20f;
    public float hitSpeed = 5f;
    #endregion

    #region Lifetime
    [Header("Lifetime")]
    public float maxLifetime = 5f;
    #endregion

    #region Damage
    [Header("Damage Settings")]
    public int damage = 10;
    public bool isCritical = false;
    #endregion

    #region Collider
    [Header("Collider Settings")]
    [Tooltip("Enable to adjust collider when facing different directions")]
    public bool adjustColliderDirection = true;
    [Tooltip("Offset adjustment when facing right")]
    public Vector2 rightFacingOffset = Vector2.zero;
    [Tooltip("Size when facing right")]
    public Vector2 rightFacingSize = Vector2.one;
    [Tooltip("Offset adjustment when facing left")]
    public Vector2 leftFacingOffset = Vector2.zero;
    [Tooltip("Size when facing left")]
    public Vector2 leftFacingSize = Vector2.one;
    [Tooltip("Direction of the capsule (vertical or horizontal)")]
    public CapsuleDirection2D rightFacingDirection = CapsuleDirection2D.Horizontal;
    public CapsuleDirection2D leftFacingDirection = CapsuleDirection2D.Horizontal;
    #endregion

    #region Time Effects
    [Header("Time Manipulation")]
    public float sizeThreshold = 0.3f;
    public bool enableTimeEffects = true;

    [Header("Spawn Effects")]
    public bool enableTimeSlowOnSpawn = true;
    [SerializeField] private float delayTimeSlowAfterSpawn;
    public float spawnTimeSlowDuration = 3f;
    [Range(0.1f, 1f)] public float spawnTimeScale = 0.5f;

    [Header("Hit Effects")]
    public bool enableTimeStopOnHit = true;
    public float hitTimeStopDuration = 0.1f;
    [Range(0.1f, 1f)] public float postHitTimeScale = 0.3f;
    public float postHitSlowDuration = 1f;
    #endregion

    #region Hit Behavior
    [Header("Hit Behavior")]
    public bool enableHitSlowdown = true;
    public bool stopAccelerationAfterHit = true;
    public int maxHits = 3;
    public float minTimeBetweenHits = 0.05f;
    #endregion

    #region Debug
    [Header("Debug Info")]
    [SerializeField] private Vector3 initialScale;
    [SerializeField] private Vector3 startScale;
    [SerializeField] private float calculatedSize;
    [SerializeField] private bool isLargeEnough;
    #endregion
    #endregion

    #region Private Variables
    private float lifetime;
    private float currentSpeed;
    private Rigidbody2D rb;
    private CapsuleCollider2D capsuleCollider;
    private bool isMovingRight;
    private bool soundPlayed;
    private int hitCount;
    private bool hasHitEnemy;
    private Coroutine currentTimeEffect;
    private float lastHitTime;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        currentSpeed = baseSpeed;
        initialScale = transform.localScale;
    }

    private void Start()
    {
        startScale = transform.localScale;
        calculatedSize = Mathf.Abs(startScale.x);
        isLargeEnough = calculatedSize >= sizeThreshold;

        PlayProjectileSound();
        ApplySpawnTimeEffect();
    }

    private void Update()
    {
        if (IsGamePaused()) return;

        lifetime += Time.unscaledDeltaTime;

        if (lifetime >= maxLifetime)
        {
            DestroyProjectile();
            return;
        }

        UpdateMovement();
        UpdateCollider();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy") && !other.CompareTag("BossEnemy")) return;
        Debug.Log("Hit Enemy");
        HandleEnemyHit(other);
    }
    #endregion

    #region Public Methods
    public void Launch(bool isFacingRight)
    {
        isMovingRight = isFacingRight;
        UpdateVisualDirection();
        UpdateColliderDirection();
        rb.velocity = new Vector2(currentSpeed * (isMovingRight ? 1 : -1), 0);
    }
    #endregion

    #region Movement & Collision
    private void UpdateMovement()
    {
        if (!hasHitEnemy || !stopAccelerationAfterHit)
        {
            currentSpeed = Mathf.Min(baseSpeed + (acceleration * lifetime), maxSpeed);
        }
        rb.velocity = new Vector2(currentSpeed * (isMovingRight ? 1 : -1), rb.velocity.y);
    }

    private void UpdateCollider()
    {
        if (adjustColliderDirection)
        {
            UpdateColliderDirection();
        }
    }

    private void UpdateColliderDirection()
    {
        if (capsuleCollider == null) return;

        if (isMovingRight)
        {
            capsuleCollider.direction = rightFacingDirection;
            capsuleCollider.offset = rightFacingOffset;
            capsuleCollider.size = rightFacingSize;
        }
        else
        {
            capsuleCollider.direction = leftFacingDirection;
            capsuleCollider.offset = leftFacingOffset;
            capsuleCollider.size = leftFacingSize;
        }
    }

    private void UpdateVisualDirection()
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (isMovingRight ? 1 : -1);
        transform.localScale = scale;
    }
    #endregion

    #region Hit Handling
    private void HandleEnemyHit(Collider2D enemy)
    {
        if (Time.time - lastHitTime < minTimeBetweenHits)
            return;

        Debug.Log($"Hit #{hitCount + 1} at {Time.time:F2}s | " +
                 $"Enemy: {enemy.name} | " +
                 $"Projectile Speed: {currentSpeed:F1}");

        lastHitTime = Time.time;
        ShowDamagePopup(enemy);
        PlayHitSound();
        ApplyHitSlowdown();
        ApplyHitTimeEffect();
        IncrementHitCount();
    }

    private void ShowDamagePopup(Collider2D enemy)
    {
        DamagePopUp.Instance?.CreateDamageText(
            damage,
            enemy.transform.position,
            isPlayer: false,
            isBoss: enemy.CompareTag("BossEnemy"),
            isCritical: isCritical
        );
    }

    private void IncrementHitCount()
    {
        hitCount++;
        if (hitCount >= maxHits || !isLargeEnough)
        {
            DestroyProjectile();
        }
    }

    private void ApplyHitSlowdown()
    {
        if (enableHitSlowdown)
        {
            hasHitEnemy = true;
            currentSpeed = hitSpeed;

            if (capsuleCollider != null)
            {
                capsuleCollider.enabled = false;
                StartCoroutine(ReenableColliderAfterDelay(minTimeBetweenHits * 0.8f));
            }
        }
    }

    private IEnumerator ReenableColliderAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (capsuleCollider != null)
        {
            capsuleCollider.enabled = true;
        }
    }
    #endregion

    #region Time Effects
    private void ApplySpawnTimeEffect()
    {
        if (isLargeEnough && enableTimeEffects && enableTimeSlowOnSpawn && !IsGamePaused())
        {
            if (delayTimeSlowAfterSpawn <= Mathf.Epsilon)
            {
                currentTimeEffect = StartCoroutine(TimeEffectRoutine(spawnTimeScale, spawnTimeSlowDuration));
                Debug.Log("[Projectile] Applying spawn time slow immediately");
            }
            else
            {
                StartCoroutine(DelayedSpawnTimeEffect());
            }
        }
    }

    private IEnumerator DelayedSpawnTimeEffect()
    {
        float timer = 0;
        while (timer < delayTimeSlowAfterSpawn && !IsGamePaused())
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!IsGamePaused() && this != null && isLargeEnough)
        {
            currentTimeEffect = StartCoroutine(TimeEffectRoutine(spawnTimeScale, spawnTimeSlowDuration));
        }
    }

    private void ApplyHitTimeEffect()
    {
        if (isLargeEnough && enableTimeEffects && enableTimeStopOnHit)
        {
            if (currentTimeEffect != null) StopCoroutine(currentTimeEffect);
            currentTimeEffect = StartCoroutine(HitTimeEffectRoutine());
        }
    }

    private IEnumerator HitTimeEffectRoutine()
    {
        if (TimeManager.Instance == null || IsGamePaused()) yield break;

        TimeManager.Instance.SetTimeScale(0f);
        yield return new WaitForSecondsRealtime(hitTimeStopDuration);

        TimeManager.Instance.SetTimeScale(postHitTimeScale);
        yield return new WaitForSecondsRealtime(postHitSlowDuration * 1.2f);

        RestoreNormalTime();
        currentTimeEffect = null;
    }

    private IEnumerator TimeEffectRoutine(float timeScale, float duration)
    {
        TimeManager.Instance?.SetTimeScale(timeScale);
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        yield return new WaitForSecondsRealtime(duration);

        RestoreNormalTime();
        currentTimeEffect = null;
    }
    #endregion

    #region Audio
    private void PlayProjectileSound()
    {
        if (!soundPlayed && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlayerProjectileSound();
            soundPlayed = true;
        }
    }

    private void PlayHitSound()
    {
        AudioManager.Instance.PlayProjectileHit();
        AudioManager.Instance.StopSound("PlayerOthers", "projectile_player");
    }
    #endregion

    #region Utility
    private void DestroyProjectile()
    {
        if (currentTimeEffect != null) StopCoroutine(currentTimeEffect);
        RestoreNormalTime();
        Destroy(gameObject);
    }

    private bool IsGamePaused() => TimeManager.Instance?.IsPaused ?? false;
    private void RestoreNormalTime() => TimeManager.Instance?.ResetTimeScale();
    #endregion
}