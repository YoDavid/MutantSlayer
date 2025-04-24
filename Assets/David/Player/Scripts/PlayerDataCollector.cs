using UnityEngine;

public class PlayerDataCollector : MonoBehaviour
{
    public PlayerData GetCurrentPlayerData()
    {
        int health = 0;
        float stamina = 0f;
        int healing = 0;

        var playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            health = playerHealth.CurrentHealth;
        }

        var staminaBar = FindObjectOfType<UIPlayerStaminaBar>();
        if (staminaBar != null)
        {
            stamina = staminaBar.GetStamina();
        }

        var healingController = FindObjectOfType<UIHealingController>();
        if (healingController != null)
        {
            healing = healingController.GetCurrentHealingCount();
        }

        return new PlayerData(health, stamina, healing);
    }
}
