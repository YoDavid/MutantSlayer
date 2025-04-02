using TMPro;
using UnityEngine;

public class DamagePopUp : MonoBehaviour
{
    // Singleton for easy access
    public static DamagePopUp Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private GameObject popUpPrefab;
    [SerializeField] private Color defaultColor = Color.red;
    [SerializeField] private Color playerColor = Color.red;
    [SerializeField] private Color bossColor = Color.red;
    [Header("Spawn Position")]
    [SerializeField] private float regularEnemies = 1.0f;
    [SerializeField] private float playerSpawnHeight = 1.5f;
    [SerializeField] private float bossSpawnHeight = 2.0f;
    [SerializeField] private float horizontalRandomness = 1f; // Max X offset

    [Header("Animation Settings")]
    [SerializeField] private float minFloatDistance = 1f;
    [SerializeField] private float maxFloatDistance = 3f;
    [SerializeField] private float minDuration = 0.8f;
    [SerializeField] private float maxDuration = 1f;

    private void Awake()
    {
        
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public void CreateDamageText(int damage, Vector3 basePosition, bool isPlayer = false, bool isBoss = false)
    {
        if (popUpPrefab == null)
        {
            Debug.LogError("PopUp prefab not assigned!", this);
            return;
        }

        // Calculate spawn position with randomness
        float spawnHeight = isPlayer ? playerSpawnHeight : (isBoss ? bossSpawnHeight : regularEnemies);
        Vector3 randomOffset = new Vector3(
            Random.Range(-horizontalRandomness, horizontalRandomness),
            0,
            0
        );

        Vector3 spawnPosition = basePosition +
                              Vector3.up * spawnHeight +
                              randomOffset;

        // Randomize animation parameters
        float floatDistance = Random.Range(minFloatDistance, maxFloatDistance);
        float duration = Random.Range(minDuration, maxDuration);

        // Create popup
        GameObject popUp = Instantiate(popUpPrefab, spawnPosition, Quaternion.identity);
        TextMeshPro text = popUp.GetComponent<TextMeshPro>();
        text.text = damage.ToString();
        text.color = isPlayer ? playerColor : (isBoss ? bossColor : defaultColor);

        // Animate with random values
        LeanTween.moveY(popUp, spawnPosition.y + floatDistance, duration)
                 .setEaseOutQuad();
        LeanTween.alphaText(popUp.GetComponent<RectTransform>(), 0f, duration)
                 .setOnComplete(() => Destroy(popUp));
    }
}

