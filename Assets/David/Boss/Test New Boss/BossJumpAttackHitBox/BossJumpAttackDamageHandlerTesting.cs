using UnityEngine;

public class BossJumpAttackDamageHandlerTesting : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private DamageConfig damageConfig;

    private DamageDealer damageDealer;
    private PlayerHealth playerHealth;
    private Collider2D playerHurtBoxCollider;
    private CameraShake cameraShake;

    private bool isPlayerInRange = false;

    private void Awake()
    {
        damageDealer = gameObject.AddComponent<DamageDealer>();
        damageDealer.config = damageConfig;
        cameraShake = FindAnyObjectByType<CameraShake>();
    }

    private void Start()
    {
        FindPlayerReferences();
    }

    private void FindPlayerReferences()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth == null)
                Debug.LogError("Player found but has no PlayerHealth component!");
        }
        else
        {
            Debug.LogError("Player not found in scene!");
        }

        GameObject hurtBox = GameObject.FindWithTag("PlayerHurtBox");
        if (hurtBox != null)
        {
            playerHurtBoxCollider = hurtBox.GetComponent<Collider2D>();
            if (playerHurtBoxCollider == null)
                Debug.LogError("Player hurtbox found but has no Collider2D component!");
        }
        else
        {
            Debug.LogError("Player hurtbox not found! Make sure it exists and has the 'PlayerHurtBox' tag.");
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

    private void ApplyDamage()
    {
        if (isPlayerInRange && !playerHealth.IsPlayerInvulnerable())
        {
            var (damage, isCritical) = damageDealer.CalculateDamage();
            playerHealth.TakeDamage(damage, isCritical);
            cameraShake?.ShakeCameraJumpSmashAttack();
        }
    }
}
