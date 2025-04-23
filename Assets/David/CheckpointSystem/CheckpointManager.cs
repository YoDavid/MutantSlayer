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

        Debug.Log($"Checkpoint saved - Health: {_savedHealth}, Stamina: {_savedStamina}, Healing: {_savedHealing}");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (_savedScene == scene.name)
        {
            StartCoroutine(DelayedRespawn());
        }
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
            // Set position and health
            player.transform.position = _lastCheckpointPosition;
            player.SetHealth(_savedHealth);
            player.Revive();

            // Set stamina
            var staminaBar = FindObjectOfType<UIPlayerStaminaBar>();
            if (staminaBar != null)
            {
                staminaBar.SetStamina(_savedStamina);
            }

            // Set healing items
            var healingController = FindObjectOfType<UIHealingController>();
            if (healingController != null)
            {
                ResetHealingItems(healingController);
            }

            // Force camera reposition
            var camera = FindObjectOfType<CameraDeadZoneFollow>();
            camera?.ForceCameraReposition();
        }
    }

    private void ResetHealingItems(UIHealingController healingController)
    {
        // Disable all healing icons first
        foreach (var icon in healingController.healingIcons)
        {
            icon.enabled = false;
        }

        // Enable only the saved amount
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
}

public struct PlayerData
{
    public int health;
    public float stamina;
    public int healing;

    public PlayerData(int health, float stamina, int healing)
    {
        this.health = health;
        this.stamina = stamina;
        this.healing = healing;
    }
}