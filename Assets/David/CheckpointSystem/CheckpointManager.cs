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
            player.transform.position = _lastCheckpointPosition;
            player.SetHealth(_savedHealth);
            player.Revive();

            var staminaBar = FindObjectOfType<UIPlayerStaminaBar>();
            if (staminaBar != null)
            {
                staminaBar.SetStamina(_savedStamina);
            }

            var healingController = FindObjectOfType<UIHealingController>();
            if (healingController != null)
            {
                ResetHealingItems(healingController);
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

        Debug.Log("Checkpoint data reset.");
    }

    public string GetSavedScene()
    {
        return _savedScene;
    }
}
