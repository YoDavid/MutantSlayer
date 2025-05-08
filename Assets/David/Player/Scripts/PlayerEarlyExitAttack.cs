using System.Collections;
using UnityEngine;

public class PlayerEarlyExitAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float hitDelay = 0.35f;
    [SerializeField][Range(0f, 1f)] private float timeSlowDuration = 0.3f;

    [Header("References")]
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private CameraShake cameraShake;
    [SerializeField] private GameObject hitParticlePrefab;
    [SerializeField] private DamageConfig damageConfig;
    [SerializeField] private PauseMenuController pauseMenuController;
    [SerializeField] private PlayerComboHitbox playerComboHitbox;

    private Collider2D hitCollider;
    private DamageDealer damageDealer;
    private PlayerMovementController playerMovement;
    private bool isAttackActive;

    private float leftMouseButtonHoldTime = 0f;
    [SerializeField] private float minHoldTime = 0.25f; // Minimum time to trigger early exit
    [SerializeField] private PlayerLevelSystem playerLevelSystem;


    private void Awake()
    {
        hitCollider = GetComponent<Collider2D>();
        hitCollider.enabled = false;

        damageDealer = gameObject.AddComponent<DamageDealer>();
        damageDealer.config = damageConfig;

        audioManager = FindAnyObjectByType<AudioManager>();
        cameraShake = FindAnyObjectByType<CameraShake>();
        pauseMenuController = FindAnyObjectByType<PauseMenuController>();
        playerMovement = GetComponentInParent<PlayerMovementController>();
        playerComboHitbox = FindAnyObjectByType<PlayerComboHitbox>(); 
    }

    private void Update()
    {
        if (Input.GetMouseButton(0)) // Left mouse button held
        {
            leftMouseButtonHoldTime += Time.deltaTime;
        }
        else if (Input.GetMouseButtonUp(0)) // Left mouse button released
        {
            if (leftMouseButtonHoldTime >= minHoldTime && leftMouseButtonHoldTime <= playerComboHitbox.windupDuration)
            {
                ExecuteEarlyExit();
            }
            leftMouseButtonHoldTime = 0f; // Reset hold time after button release
        }
    }

    public void ExecuteEarlyExit()
    {
        if (isAttackActive || (playerMovement != null && !playerMovement.isGrounded))
        {
            return;
        }

        StartCoroutine(PerformSingleHitAttack());
    }

    private IEnumerator PerformSingleHitAttack()
    {
        if (playerMovement != null && !playerMovement.isGrounded)
        {
            CleanUpAttack();
            yield break;
        }

        isAttackActive = true;

        // Initial delay before attack
        yield return new WaitForSeconds(hitDelay);

        // Activate hitbox for one frame
        hitCollider.enabled = true;
        yield return new WaitForFixedUpdate();
        hitCollider.enabled = false;

        // Time slowdown effect - only if not paused
        if (!IsGamePaused())
        {
            TimeManager.Instance?.SetTimeScale(timeSlowDuration);
            yield return new WaitForSecondsRealtime(0.1f); // Short slowdown duration

            if (!IsGamePaused())
            {
                TimeManager.Instance?.ResetTimeScale();
            }
        }

        else
        {
            // If paused, just wait without time slowdown
            yield return new WaitForSecondsRealtime(0.1f);
        }

        // Play sound effect
        AudioManager.Instance.PlayEarlyComboExit();
        CleanUpAttack();
    }

    private bool IsGamePaused()
    {
        return pauseMenuController != null && pauseMenuController.IsVisible;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isAttackActive) return;

        bool isBoss = other.CompareTag("BossEnemy");
        if (!other.CompareTag("Enemy") && !isBoss) return;

        var damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            // Get base scaled damage
            int damage = playerLevelSystem.GetScaledDamage("early_exit"); // Changed to "early_exit" type

            // Apply random variation (±20%)
            float randomVariation = Random.Range(-0.2f, 0.7f);
            damage = Mathf.RoundToInt(damage * (1f + randomVariation));

            // Calculate critical hit
            bool isCritical = UnityEngine.Random.value <= playerLevelSystem.GetCritChance();
            if (isCritical)
            {
                damage = Mathf.RoundToInt(damage * playerLevelSystem.GetCritMultiplier());
            }

            damageable.TakeDamage(damage, isCritical, false, 0);
           
            cameraShake?.NormalHitShakeCamera();
        }
    }

    private void CreateHitEffects(Vector3 position, bool isCritical, bool isBoss, int finalDamage)
    {
        // Create damage popup with the exact damage dealt
        DamagePopUp.Instance?.CreateDamageText(
            finalDamage, // Use the calculated damage including random variation
            position,
            true, isBoss, isCritical, false, 0);

        // Spawn hit particles
        if (hitParticlePrefab != null)
        {
            Instantiate(hitParticlePrefab,
                position + new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.1f, 0.1f) - 2f, 0),
                Quaternion.identity);
        }
    }


    private void CleanUpAttack()
    {
        isAttackActive = false;
        hitCollider.enabled = false;
    }
}