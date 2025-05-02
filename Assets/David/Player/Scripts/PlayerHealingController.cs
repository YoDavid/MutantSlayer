using UnityEngine;

public class PlayerHealingController : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private UIHealingController healingController;
    [SerializeField] private UIPlayerStaminaBar staminaBar;

    [SerializeField] private float healPercentage = 0.33f; // Healing percentage of total health per charge
    [SerializeField] private float holdThreshold = 0.25f; // seconds to register as hold

    [SerializeField] private float holdTimer = 0f;
    [SerializeField] private bool isHolding = false;

    [SerializeField] private HealingParticlesPool healingParticlePool;
    [SerializeField] private Transform healingEffectSpawnPoint;
    [SerializeField] private float healingEffectYOffset = 1.0f;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            holdTimer = 0f;
            isHolding = true;
        }

        // Handle holding 'E' logic
        if (Input.GetKey(KeyCode.E))
        {
            holdTimer += Time.deltaTime;

            // Check if stamina is full and the player is holding 'E'
            if (staminaBar.IsFull && healingController.CanGainHeal && holdTimer >= holdThreshold)
            {
                // Deplete stamina gradually
                staminaBar.DepleteStamina(staminaBar.depletionRate);

                // If stamina reaches empty after depletion, restore a healing charge
                if (staminaBar.IsEmpty && healingController.CanGainHeal)
                {
                    healingController.GainHealing();  // Restore one healing charge
                    holdTimer = 0f; // Reset the hold timer after gaining healing
                }
            }
        }

        if (Input.GetKeyUp(KeyCode.E))
        {
            isHolding = false;

            // Heal if the health is not full and there are healing charges available
            if (holdTimer < holdThreshold && playerHealth.CurrentHealth < playerHealth.MaxHealth && healingController.CanHeal)
            {
                playerHealth.Heal(Mathf.RoundToInt(playerHealth.MaxHealth * healPercentage));
                healingController.UseHealing();

                Vector3 spawnPosition = healingEffectSpawnPoint.position + new Vector3(0, healingEffectYOffset, 0);
                healingParticlePool.PlayParticles(spawnPosition);

            }

            holdTimer = 0f;
        }
    }


}
