using System.Collections;
using UnityEngine;

public class SpitProjectile : MonoBehaviour
{
    [Header("Damage Settings")]
    public DamageConfig damageConfig;
    private BossDamageDealer damageDealer;

    [Header("Projectile Settings")]
    [SerializeField] private float speed = 50f;
    [SerializeField] private float growDuration = 1.5f;
    [SerializeField] private Vector3 initialScale = new Vector3(0.2f, 0.2f, 0.2f);
    [SerializeField] private Vector3 maxScale = new Vector3(1f, 1f, 1f);

    [Header("Player References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Collider2D playerHurtBoxCollider;

    [Header("Damage Scaling")]
    [SerializeField] private BossLevelScaling bossLevelScaling;
    private float currentPhaseMultiplier;

    private float moveDirection;
    private SpriteRenderer spriteRenderer;

    [Header("Damage Reference")]
    [SerializeField] private BossComboAttackHitbox bossHitbox;
    private int currentDamage;
    private bool isCritical;

    private void Awake()
    {
        InitializeComponents();
        bossHitbox = FindAnyObjectByType<BossComboAttackHitbox>();
        // Get current damage values from boss hitbox
        if (bossHitbox != null)
        {
            (currentDamage, isCritical) = bossHitbox.GetCurrentDamageValues();
        }
        else
        {
            Debug.LogError("Boss hitbox reference missing!");
            currentDamage = 10; // Fallback value
            isCritical = false;
        }
    }

    private void UpdateScaledDamage()
    {
        // Create scaled copy of damage config (like combo attack does)
        DamageConfig scaledConfig = Instantiate(damageConfig);
        currentPhaseMultiplier = bossLevelScaling.GetPhaseDamageMultiplier();

        // Apply scaling to damage ranges
        scaledConfig.normalDamageRange = new Vector2Int(
            Mathf.RoundToInt(damageConfig.normalDamageRange.x * currentPhaseMultiplier),
            Mathf.RoundToInt(damageConfig.normalDamageRange.y * currentPhaseMultiplier)
        );

        scaledConfig.criticalDamageRange = new Vector2Int(
            Mathf.RoundToInt(damageConfig.criticalDamageRange.x * currentPhaseMultiplier),
            Mathf.RoundToInt(damageConfig.criticalDamageRange.y * currentPhaseMultiplier)
        );

        damageDealer.config = scaledConfig;
    }

    private void InitializeComponents()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        transform.localScale = initialScale;

        if (playerHealth == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) playerHealth = player.GetComponent<PlayerHealth>();
        }

        if (playerHurtBoxCollider == null)
        {
            GameObject hurtBox = GameObject.FindWithTag("PlayerHurtBox");
            if (hurtBox != null) playerHurtBoxCollider = hurtBox.GetComponent<Collider2D>();
        }
    }

    private void Start()
    {
        StartCoroutine(GrowProjectile());
        StartCoroutine(DestroyAfterLifetime());
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
            ApplyDamage();
            Destroy(gameObject);
        }
    }

    private void ApplyDamage()
    {
        if (playerHealth != null && !playerHealth.IsPlayerInvulnerable())
        {
            playerHealth.TakeDamage(currentDamage, isCritical);
            Debug.Log($"Spit dealt {currentDamage} damage (Critical: {isCritical})");
        }
    }

    private IEnumerator DestroyAfterLifetime()
    {
        yield return new WaitForSeconds(5f);
        Destroy(gameObject);
    }
}