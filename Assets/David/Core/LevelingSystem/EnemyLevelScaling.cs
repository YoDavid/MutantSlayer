using UnityEngine;

public class EnemyLevelScaling : MonoBehaviour
{
    [Header("Scaling Settings")]
    public int baseHealth = 50;
    public int baseDamage = 5;
    public float healthScaling = 0.08f; // Reduced from 0.2 (8% per level)
    public float damageScaling = 0.05f; // Reduced from 0.15 (5% per level)

    [Header("Experience Reward Settings")]
    public int baseExpReward = 20; // Reduced from 100
    public float expScalingFactor = 1.05f; // Reduced from 1.5 (5% per level)

    [Header("Level Information")]
    [SerializeField] private int enemyLevel = 1; // Exposed in the inspector

    private HealthSystem healthSystem;
    [SerializeField] private PlayerLevelSystem playerLevelSystem;

    // Event to notify when the enemy levels up
    public event System.Action OnLevelUp;

    private void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();
        playerLevelSystem = FindObjectOfType<PlayerLevelSystem>();

        // Apply scaling on startup for pre-placed enemies
        ApplyInitialScaling();
    }

    private void OnEnable()
    {
        if (playerLevelSystem != null)
        {
            playerLevelSystem.OnLevelUp += ScaleStats; // Subscribe to level up event
        }
    }

    private void OnDisable()
    {
        if (playerLevelSystem != null)
        {
            playerLevelSystem.OnLevelUp -= ScaleStats; // Unsubscribe from level up event
        }
    }

    private void ScaleStats()
    {
        if (playerLevelSystem == null) return;

        enemyLevel = playerLevelSystem.progression.level;
        int newMaxHealth = Mathf.RoundToInt(baseHealth * (1 + (enemyLevel * healthScaling)));

        // Preserve current health percentage when scaling dynamically
        float healthPercent = (float)healthSystem.CurrentHealth / healthSystem.MaxHealth;
        healthSystem.MaxHealth = newMaxHealth;
        healthSystem.SetHealth(Mathf.RoundToInt(newMaxHealth * healthPercent));

        OnLevelUp?.Invoke(); // UI updates via UIEnemyHealthBar
    }

    public void ApplyInitialScaling()
    {
        if (playerLevelSystem == null) return;

        enemyLevel = playerLevelSystem.progression.level;
        int newMaxHealth = Mathf.RoundToInt(baseHealth * (1 + (enemyLevel * healthScaling)));

        // Directly set health without triggering events (prevent duplicate UI updates)
        healthSystem.MaxHealth = newMaxHealth;
        healthSystem.SetHealth(newMaxHealth);
    }


    public int GetEnemyLevel()
    {
        return enemyLevel;
    }

    public int GetExpReward()
    {
        // Linear scaling
        //return baseExpReward * enemyLevel; // Scales with level but more slowly

        // Or slower exponential scaling
        return Mathf.RoundToInt(baseExpReward * Mathf.Pow(expScalingFactor, enemyLevel * 0.2f)); // Lower scaling factor
    }


}
