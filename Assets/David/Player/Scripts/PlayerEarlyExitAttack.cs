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
        audioManager?.PlaySFX("Player", "sfx_player_attack_early_exit");

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
            var (damage, isCritical) = damageDealer.CalculateDamage();
            damageable.TakeDamage(damage, isCritical, false, 0); // Hit index is 0 for single hit

            CreateHitEffects(other.transform.position, isCritical, isBoss);
            cameraShake?.NormalHitShakeCamera();
        }
    }

    private void CreateHitEffects(Vector3 position, bool isCritical, bool isBoss)
    {
        // Create damage popup
        DamagePopUp.Instance?.CreateDamageText(
            damageDealer.CalculateDamage().damage,
            position,
            true, isBoss, isCritical, false, 0); // Not a combo hit

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