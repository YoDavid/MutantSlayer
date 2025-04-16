using System.Collections;
using UnityEngine;

public class BossJumpAttackHitbox : MonoBehaviour
{
    [Header("Attack Settings")]
    public DamageConfig damageConfig; 
    private DamageDealer damageDealer;
    [SerializeField] private float attackDuration = 0.5f;
    [SerializeField] private float activationDelay = 0.2f;

    [Header("Collider Settings")]
    [SerializeField] private float colliderShift;
    private Collider2D attackCollider;
    private Vector2 originalOffset;

    [Header("Player References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Collider2D playerHurtBoxCollider;
    [SerializeField] private bool isPlayerInRange = false;

    [Header("Camera Shake")]
    private CameraShake cameraShake;

    [SerializeField] private GameObject rockParticlesPrefab;

    private void Awake()
    {
        InitializeComponents();
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
        damageDealer = gameObject.AddComponent<DamageDealer>();
        damageDealer.config = damageConfig;
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
        GameObject playerHurtBox = GameObject.FindWithTag("PlayerHurtBox");
        if (playerHurtBox != null)
        {
            playerHurtBoxCollider = playerHurtBox.GetComponent<Collider2D>();
            if (playerHurtBoxCollider == null)
            {
                Debug.LogError("Player hurtbox found but has no Collider2D component!");
            }
        }
        else
        {
            Debug.LogError("Player hurtbox not found! Make sure it exists and has the 'PlayerHurtBox' tag.");
        }

        // Find player health through the main player object
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth == null)
            {
                Debug.LogError("Player found but has no PlayerHealth component!");
            }
        }
        else
        {
            Debug.LogError("Player not found in scene!");
        }
    }

    public void ActivateJumpAttackCollider(bool isFlipped)
    {
        FlipCollider(isFlipped); // Flip the collider based on the boss's facing direction
        StartCoroutine(ActivateWithDelay());
    }

    private IEnumerator ActivateWithDelay()
    {
        yield return new WaitForSeconds(activationDelay);

        EnableCollider();
        yield return new WaitForSeconds(attackDuration);
        DisableCollider();
    }

    private void EnableCollider()
    {
        attackCollider.enabled = true;
        if (rockParticlesPrefab != null)

        {
            Vector2 spawnPos = new Vector2(attackCollider.bounds.center.x, attackCollider.bounds.min.y + 2f);
            Instantiate(rockParticlesPrefab, spawnPos, Quaternion.identity);
        }

        AudioManager.Instance.PlayJumpAttackBoss();
    }


    public void DisableCollider()
    {
        attackCollider.enabled = false;
    }

    private void ApplyDamage()
    {
        if (isPlayerInRange && !playerHealth.IsPlayerInvulnerable())
        {
            var (damage, isCritical) = damageDealer.CalculateDamage();

            playerHealth.TakeDamage(damage, isCritical);
            cameraShake.ShakeCameraJumpSmashAttack();
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