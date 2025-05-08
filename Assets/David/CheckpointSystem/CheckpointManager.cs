using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private int defaultHealth = 100;
    [SerializeField] private float defaultStamina = 100f;
    [SerializeField] private int defaultHealing = 3;

    private Vector2 _lastCheckpointPosition;
    private int _savedHealth;
    private float _savedStamina;
    private int _savedHealing;
    private string _savedScene;
    private LevelProgression _savedLevelProgression; // Add this

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetCheckpoint(Vector2 position, PlayerData data)
    {
        _lastCheckpointPosition = position;
        _savedHealth = data.health;
        _savedStamina = data.stamina;
        _savedHealing = data.healing;
        _savedScene = SceneManager.GetActiveScene().name;

        // Save level progression if it exists
        if (data.levelProgression != null)
        {
            if (_savedLevelProgression == null)
                _savedLevelProgression = new LevelProgression();

            // Copy the data to avoid reference issues
            CopyLevelProgression(data.levelProgression, _savedLevelProgression);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (_savedScene == scene.name)
        {
            StartCoroutine(DelayedRespawn());
        }
    }

    private void CopyLevelProgression(LevelProgression source, LevelProgression destination)
    {
        destination.level = source.level;
        destination.currentExp = source.currentExp;
        destination.expToNextLevel = source.expToNextLevel;

        // Copy all other progression fields...
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

    private IEnumerator DelayedRespawn()
    {
        yield return new WaitForEndOfFrame();
        RespawnPlayer();
    }

    private void RespawnPlayer()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        var player = FindObjectOfType<PlayerHealth>();
        if (player != null)
        {
            player.transform.position = _lastCheckpointPosition;
            player.SetHealth(_savedHealth);
            player.Revive();

            // Restore stamina
            var staminaBar = FindObjectOfType<UIPlayerStaminaBar>();
            if (staminaBar != null)
            {
                staminaBar.SetStamina(_savedStamina);
            }

            // Restore healing items
            var healingController = FindObjectOfType<UIHealingController>();
            if (healingController != null)
            {
                ResetHealingItems(healingController);
            }

            // Restore level progression
            var levelSystem = player.GetComponent<PlayerLevelSystem>();
            if (levelSystem != null && _savedLevelProgression != null)
            {
                CopyLevelProgression(_savedLevelProgression, levelSystem.progression);
                levelSystem.InitializeStats(); // Re-initialize stats with saved progression
            }

            var camera = FindObjectOfType<CameraDeadZoneFollow>();
            camera?.ForceCameraReposition();
        }
    }

    private void ResetHealingItems(UIHealingController healingController)
    {
        foreach (var icon in healingController.healingIcons)
        {
            icon.enabled = false;
        }

        for (int i = 0; i < _savedHealing; i++)
        {
            if (i < healingController.healingIcons.Length)
            {
                healingController.healingIcons[i].enabled = true;
            }
        }

        healingController.currentHealing = _savedHealing;
    }

    public bool HasCheckpoint()
    {
        return !string.IsNullOrEmpty(_savedScene);
    }

    public void ResetCheckpoint()
    {
        _savedScene = null;
        _lastCheckpointPosition = Vector2.zero;
        _savedHealth = 0;
        _savedStamina = 0f;
        _savedHealing = 0;
        _savedLevelProgression = null;

        Debug.Log("Checkpoint data reset.");
    }

    public string GetSavedScene()
    {
        return _savedScene;
    }
}
