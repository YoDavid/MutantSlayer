using UnityEngine;

public class EnemyLevelScaling : MonoBehaviour
{
    [Header("Scaling Settings")]
    public int baseHealth = 50;
    public int baseDamage = 5;
    public float healthScaling = 0.08f;
    public float damageScaling = 0.2f; 

    [Header("Experience Reward Settings")]
    public int baseExpReward = 7; 
    public float expScalingFactor = 0.9f;

    [Header("Level Information")]
    [SerializeField] private int enemyLevel = 1;

    private HealthSystem healthSystem;
    [SerializeField] private PlayerLevelSystem playerLevelSystem;


    public event System.Action OnLevelUp;

    private void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();
        playerLevelSystem = FindObjectOfType<PlayerLevelSystem>();

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
