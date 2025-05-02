    using System.Collections;
using UnityEngine;


public class BossComboAttackHitbox : MonoBehaviour
{
    [Header("Attack Settings")]
    public DamageConfig damageConfig;
    private DamageDealer damageDealer; 
    [SerializeField] private float attackDuration;
    [SerializeField] private float[] attackTimings;

    [Header("Collider Settings")]
    [SerializeField] private float colliderShift;
    [SerializeField] private Collider2D attackCollider;
    private Vector2 originalOffset;

    [Header("Player References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Collider2D playerHurtBoxCollider;
    private bool isPlayerInRange = false;

    [Header("Camera Shake")]
    private CameraShake cameraShake;

    [SerializeField] private GameObject rockParticlesPrefab;
    [SerializeField] private FallingSpikeSpawner fallingSpikeSpawner;

    private void Awake()
    {
        InitializeComponents();
        damageDealer = gameObject.AddComponent<DamageDealer>(); // NEW
        damageDealer.config = damageConfig; // NEW
        fallingSpikeSpawner = FindAnyObjectByType<FallingSpikeSpawner>();
    }

    private void Start()
    {
        SetupCollider();
        FindPlayerReferences();
    }

    private void InitializeComponents()
    {
        attackCollider = GetComponent<Collider2D>();
        cameraShake = FindAnyObjectByType<CameraShake>();
    }

    private void SetupCollider()
    {
        attackCollider.enabled = false;

        if (attackCollider is BoxCollider2D boxCollider)
        {
            originalOffset = boxCollider.offset;
        }
    }

    private void FindPlayerReferences()
    {
        if (playerHealth == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) playerHealth = player.GetComponent<PlayerHealth>();
        }

        // If not assigned in Inspector, try to find automatically
        if (playerHurtBoxCollider == null)
        {
            GameObject hurtbox = GameObject.FindWithTag("PlayerHurtBox"); // Give your hurtbox this tag
            if (hurtbox != null) playerHurtBoxCollider = hurtbox.GetComponent<Collider2D>();
        }

        // Final validation
        if (playerHurtBoxCollider == null)
        {
            Debug.LogError("Player hurtbox collider not assigned or found!");
        }
    }

    public void ActivateComboAttackCollider()
    {
        StartCoroutine(ActivateComboWithIntervals());
    }

    private IEnumerator ActivateComboWithIntervals()
    {
        float startTime = Time.time;

        for (int i = 0; i < attackTimings.Length; i++)
        {
            float attackTime = attackTimings[i];
            float waitTime = attackTime - (Time.time - startTime);
            if (waitTime > 0)
                yield return new WaitForSeconds(waitTime);

            EnableCollider();

            AudioManager.Instance.PlayComboAttackBoss();

            if (rockParticlesPrefab != null)
            {
                Vector2 spawnPos = new Vector2(attackCollider.bounds.center.x, attackCollider.bounds.min.y + 2f);
                Instantiate(rockParticlesPrefab, spawnPos, Quaternion.identity);
            }

            // Check if this is the last attack in the combo
            if (i == attackTimings.Length - 1 && fallingSpikeSpawner != null)
            {
                fallingSpikeSpawner.SpawnSpikes2(); // Spawn two spikes
            }

            cameraShake.ShakeCameraComboAttack();
            yield return new WaitForSeconds(attackDuration);
            DisableCollider();
        }
    }


    private void EnableCollider()
    {
        attackCollider.enabled = true;
    }

    private void DisableCollider()
    {
        attackCollider.enabled = false;
    }

    private void ApplyDamage()
    {
        if (isPlayerInRange && !playerHealth.IsPlayerInvulnerable())
        {
            var (damage, isCritical) = damageDealer.CalculateDamage(); // NEW
            playerHealth.TakeDamage(damage, isCritical); // MODIFIED
            cameraShake?.ShakeCameraComboAttack(); // Moved here from ActivateComboWithIntervals
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == playerHurtBoxCollider)
        {
            isPlayerInRange = true;
            ApplyDamage();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == playerHurtBoxCollider)
        {
            isPlayerInRange = false;
        }
    }

    private void OnDrawGizmos()
    {
        if (attackCollider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(attackCollider.bounds.center, attackCollider.bounds.size);
        }
    }

    public void FlipCollider(bool isFlipped)
    {
        if (attackCollider is BoxCollider2D boxCollider)
        {
            boxCollider.offset = isFlipped
                ? new Vector2(originalOffset.x + colliderShift, originalOffset.y)
                : originalOffset;
        }
    }
}
