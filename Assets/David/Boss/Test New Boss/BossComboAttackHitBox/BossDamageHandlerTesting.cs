using UnityEngine;

public class BossDamageHandlerTesting : MonoBehaviour
{
    private DamageDealer damageDealer;
    private PlayerHealth playerHealth;
    private Collider2D playerHurtBoxCollider;
    private bool isPlayerInRange = false;
    private CameraShake cameraShake;

    public BossDamageHandlerTesting(DamageConfig config, PlayerHealth health, Collider2D hurtbox)
    {
        playerHealth = health;
        playerHurtBoxCollider = hurtbox;

        var gameObj = new GameObject("DamageDealerTemp");
        damageDealer = gameObj.AddComponent<DamageDealer>();
        damageDealer.config = config;
        cameraShake = GameObject.FindObjectOfType<CameraShake>();
    }

    public void FindPlayerReferences()
    {
        if (playerHealth == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) playerHealth = player.GetComponent<PlayerHealth>();
        }

        if (playerHurtBoxCollider == null)
        {
            GameObject hurtbox = GameObject.FindWithTag("PlayerHurtBox");
            if (hurtbox != null) playerHurtBoxCollider = hurtbox.GetComponent<Collider2D>();
        }

        if (playerHurtBoxCollider == null)
        {
            Debug.LogError("Player hurtbox collider not assigned or found!");
        }
    }

    public void HandleCollision(Collider2D other)
    {
        if (other == playerHurtBoxCollider)
        {
            isPlayerInRange = true;
            ApplyDamage();
        }
    }

    public void EndCollision(Collider2D other)
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
            cameraShake?.ShakeCameraComboAttack();
        }
    }
}