using UnityEngine;

public class PlayerDataCollector : MonoBehaviour
{
    public PlayerData GetCurrentPlayerData()
    {
        int health = 0;
        float stamina = 0f;
        int healing = 0;
        LevelProgression levelProgression = null;

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

        var levelSystem = GetComponent<PlayerLevelSystem>();
        if (levelSystem != null)
        {
            // Create a deep copy of the progression to avoid reference issues
            levelProgression = new LevelProgression();
            CopyLevelProgression(levelSystem.progression, levelProgression);
        }

        return new PlayerData(health, stamina, healing, levelProgression);
    }

    private void CopyLevelProgression(LevelProgression source, LevelProgression destination)
    {
        destination.level = source.level;
        destination.currentExp = source.currentExp;
        destination.expToNextLevel = source.expToNextLevel;

        destination.baseHealth = source.baseHealth;
        destination.baseNormalDamage = source.baseNormalDamage;
        destination.baseProjectileDamage = source.baseProjectileDamage;
        destination.baseCritChance = source.baseCritChance;
        destination.baseCritMultiplier = source.baseCritMultiplier;

        destination.healthPerLevel = source.healthPerLevel;
        destination.normalDamagePerLevel = source.normalDamagePerLevel;
        destination.projectileDamagePerLevel = source.projectileDamagePerLevel;
        destination.critChancePer5Levels = source.critChancePer5Levels;
        destination.critMultiplierPer3Levels = source.critMultiplierPer3Levels;
        destination.expGrowthFactor = source.expGrowthFactor;
    }
}