using UnityEngine;

public class BossLevelScaling : MonoBehaviour
{
    [Header("Base Stats")]
    public int baseHealth = 300; 
    public int baseDamage = 15; 

    [Header("Scaling Settings")]
    public float healthScaling = 0.15f; 
    public float damageScaling = 0.1f; 
    public int levelOffset = 2;

    [Header("Experience Reward")]
    public int baseExpReward = 100; 
    public float expScalingFactor = 1.1f; 

    [Header("Phase Settings")]
    [Range(0,1)] public float phase2Threshold = 0.6f;
    [Range(0,1)] public float phase3Threshold = 0.3f;
    public float[] phaseDamageMultipliers = { 1f, 1.25f, 1.5f }; 

    [Header("Critical Hit Settings")]
    public float baseCritChance = 0.15f;
    public float baseCritMultiplier = 2.0f;
    public float critChancePerPhase = 0.05f;
    public float critMultiplierPerPhase = 0.25f; 

    // Components
    private BossHealth bossHealth;
    private PlayerLevelSystem playerLevelSystem;

    // Runtime values
    private int currentPhase = 1;
    private int scaledDamage;
    private int effectiveLevel;

    public int GetBaseDamage() => Mathf.RoundToInt(baseDamage * (1 + (effectiveLevel * damageScaling)));

    public event System.Action OnLevelUp;

    private void Awake()
    {
        bossHealth = GetComponent<BossHealth>();
        playerLevelSystem = FindObjectOfType<PlayerLevelSystem>();

        InitializeScaling();
    }

    private void InitializeScaling()
    {
        if (playerLevelSystem == null) return;

        effectiveLevel = playerLevelSystem.progression.level + levelOffset;
        ScaleStats();
        
        playerLevelSystem.OnLevelUp += ScaleStats;
        bossHealth.OnHealthChanged += CheckPhaseTransition;
    }

    private void OnDestroy()
    {
        if (playerLevelSystem != null)
            playerLevelSystem.OnLevelUp -= ScaleStats;
        
        if (bossHealth != null)
            bossHealth.OnHealthChanged -= CheckPhaseTransition;
    }

    private void ScaleStats()
    {
        effectiveLevel = playerLevelSystem.progression.level + levelOffset;

        bossHealth.maxHealth = Mathf.RoundToInt(baseHealth * (1 + (effectiveLevel * healthScaling)));
        bossHealth.currentHealth = bossHealth.maxHealth; // Reset to full health

        scaledDamage = Mathf.RoundToInt(baseDamage * (1 + (effectiveLevel * damageScaling)));

        // Force update the health bar visuals immediately
        bossHealth.ForceHealthUpdate();

    }

    private void OnPhaseChanged(int phase)
    {
    }

    private void CheckPhaseTransition(int currentHealth)
    {
        float healthPercent = (float)currentHealth / bossHealth.maxHealth;
        int newPhase = healthPercent switch
        {
            var p when p <= phase3Threshold => 3,
            var p when p <= phase2Threshold => 2,
            _ => 1
        };

        if (newPhase != currentPhase)
        {
            currentPhase = newPhase;
            OnPhaseChanged(currentPhase);
        }
    }

    public int GetCurrentDamage()
    {
        float multiplier = phaseDamageMultipliers[Mathf.Clamp(currentPhase-1, 0, phaseDamageMultipliers.Length-1)];
        return Mathf.RoundToInt(scaledDamage * multiplier);
    }

    public int GetScaledDamage(int baseDamage)
    {
        float levelMultiplier = 1 + (effectiveLevel * damageScaling);
        float phaseMultiplier = GetPhaseDamageMultiplier();
        return Mathf.RoundToInt(baseDamage * levelMultiplier * phaseMultiplier);
    }

    public float GetAdditionalCritChance()
    {
        return currentPhase * critChancePerPhase;
    }

    public float GetCritDamageMultiplier()
    {
        return baseCritMultiplier + (currentPhase * critMultiplierPerPhase);
    }

    public float GetCritChance() => baseCritChance + (currentPhase * critChancePerPhase);
    public float GetCritMultiplier() => baseCritMultiplier + (currentPhase * critMultiplierPerPhase);
    public int GetExpReward() => Mathf.RoundToInt(baseExpReward * Mathf.Pow(expScalingFactor, effectiveLevel - 1));

    public int GetEffectiveLevel()
    {
        return playerLevelSystem != null ? playerLevelSystem.progression.level + levelOffset : 1;
    }
    public float GetPhaseDamageMultiplier()
    {
        return phaseDamageMultipliers[Mathf.Clamp(currentPhase - 1, 0, phaseDamageMultipliers.Length - 1)];
    }

    public float GetCurrentCritChance(DamageConfig config)
    {
        return Mathf.Clamp01(config.criticalChance + (currentPhase * critChancePerPhase));
    }
}