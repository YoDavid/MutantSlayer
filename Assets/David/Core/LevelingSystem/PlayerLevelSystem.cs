using UnityEngine;

[System.Serializable]
public class LevelProgression
{
    public int level = 1;
    public int currentExp = 0;
    public int expToNextLevel = 50;

    [Header("Base Stats")]
    public int baseHealth = 100;
    public int baseNormalDamage = 10;
    public int baseProjectileDamage = 8;
    public float baseCritChance = 0.1f;
    public float baseCritMultiplier = 1.5f;

    [Header("Scaling Factors")]
    public float healthPerLevel = 10f; 
    public float normalDamagePerLevel = 1f; 
    public float projectileDamagePerLevel = 0.8f; 
    public float critChancePer5Levels = 0.02f;
    public float critMultiplierPer3Levels = 0.05f;
    public float expGrowthFactor = 1.3f;
}

public class PlayerLevelSystem : MonoBehaviour
{
    public LevelProgression progression;
    private PlayerHealth playerHealth;

    // Add event to notify when level up occurs
    public event System.Action OnLevelUp;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        InitializeStats();
    }

    public void InitializeStats()
    {
        playerHealth.MaxHealth = progression.baseHealth + Mathf.RoundToInt((progression.level - 1) * progression.healthPerLevel);
        playerHealth.RestoreFullHealth();
    }

    public void AddExperience(int amount)
    {
        progression.currentExp += amount;
        CheckLevelUp();
    }

    private void CheckLevelUp()
    {
        while (progression.currentExp >= progression.expToNextLevel)
        {
            LevelUp();
        }
    }

    public void LevelUp()
    {
        progression.level++;
        progression.currentExp -= progression.expToNextLevel;
        progression.expToNextLevel = Mathf.RoundToInt(progression.expToNextLevel * progression.expGrowthFactor);

        playerHealth.MaxHealth = progression.baseHealth + Mathf.RoundToInt((progression.level - 1) * progression.healthPerLevel);
        playerHealth.RestoreFullHealth();

        if (progression.level % 3 == 0)
            progression.baseCritMultiplier += progression.critMultiplierPer3Levels;

        if (progression.level % 5 == 0)
            progression.baseCritChance += progression.critChancePer5Levels;

        OnLevelUp?.Invoke();
        PlayLevelUpEffects();

        // Prevent player from doing anything briefly after level up
        GetComponent<PlayerAnimationController>()?.SetLevelingUp();
    }


    private void PlayLevelUpEffects()
    {
        // Add your visual/audio effects here
        // Example: AudioManager.Instance.PlayLevelUpSound();
        // Example: Instantiate level up particles
    }

    // Call this from your attack system to get scaled damage values
    public int GetScaledDamage(string attackType)
    {
        float baseDamage = progression.baseNormalDamage +
                          ((progression.level - 1) * progression.normalDamagePerLevel);

        switch (attackType)
        {
            case "normal":
                return Mathf.RoundToInt(baseDamage);
            case "charged":
                return Mathf.RoundToInt(baseDamage * 1.8f);
            case "early_exit":
                return Mathf.RoundToInt(baseDamage * 0.7f);
            case "projectile":
                return Mathf.RoundToInt(progression.baseProjectileDamage +
                                      ((progression.level - 1) * progression.projectileDamagePerLevel));
            default:
                return Mathf.RoundToInt(baseDamage);
        }
    }

    public float GetCritChance()
    {
        return progression.baseCritChance;
    }

    public float GetCritMultiplier()
    {
        return progression.baseCritMultiplier;
    }
}
