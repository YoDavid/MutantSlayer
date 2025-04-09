using TMPro;
using UnityEngine;

public class DamagePopUp : MonoBehaviour
{
    // Singleton for easy access
    public static DamagePopUp Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject popUpPrefab;

    [Header("Text Colors")]
    [SerializeField] private Color defaultColor = Color.red;
    [SerializeField] private Color playerColor = Color.red;
    [SerializeField] private Color bossColor = Color.red;
    [SerializeField] private Color critColor = Color.yellow;

    [Header("Text Sizes")]
    [SerializeField] private float normalTextSize = 4f;
    [SerializeField] private float critTextSize = 6f;

    [Header("Spawn Positions")]
    [SerializeField] private float regularEnemiesHeight = 1.0f;
    [SerializeField] private float playerSpawnHeight = 1.5f;
    [SerializeField] private float bossSpawnHeight = 2.0f;
    [SerializeField] private float horizontalRandomness = 1f;

    [Header("Normal Hit Settings")]
    [SerializeField] private float normalMinFloatDistance = 1f;
    [SerializeField] private float normalMaxFloatDistance = 2f;
    [SerializeField] private float normalMinDuration = 0.8f;
    [SerializeField] private float normalMaxDuration = 1f;

    [Header("Critical Hit Settings")]
    [SerializeField] private float critMinFloatDistance = 2f;
    [SerializeField] private float critMaxFloatDistance = 3f;
    [SerializeField] private float critMinDuration = 1f;
    [SerializeField] private float critMaxDuration = 1.2f;

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

    public void CreateDamageText(int damage, Vector3 position, bool isPlayer, bool isBoss, bool isCritical = false)
    {
        // Determine spawn height
        float spawnHeight = isPlayer ? playerSpawnHeight :
                          (isBoss ? bossSpawnHeight : regularEnemiesHeight);

        // Add horizontal randomness
        Vector3 randomOffset = new Vector3(
            Random.Range(-horizontalRandomness, horizontalRandomness),
            0,
            0
        );

        Vector3 spawnPosition = position + Vector3.up * spawnHeight + randomOffset;

        // Create popup
        GameObject popUp = Instantiate(popUpPrefab, spawnPosition, Quaternion.identity);
        TextMeshPro text = popUp.GetComponent<TextMeshPro>();

        // Set text properties with minus sign and space
        text.text = $"-{damage}"; // Added minus sign and space
        text.fontSize = isCritical ? critTextSize : normalTextSize;

        // Color priority: Critical > Player/Boss > Default
        text.color = isCritical ? critColor :
                   (isPlayer ? playerColor :
                   (isBoss ? bossColor : defaultColor));

        // Choose animation parameters
        float floatDistance = isCritical ?
            Random.Range(critMinFloatDistance, critMaxFloatDistance) :
            Random.Range(normalMinFloatDistance, normalMaxFloatDistance);

        float duration = isCritical ?
            Random.Range(critMinDuration, critMaxDuration) :
            Random.Range(normalMinDuration, normalMaxDuration);

        // Animate
        LeanTween.moveY(popUp, spawnPosition.y + floatDistance, duration)
                 .setEaseOutQuad();
        LeanTween.alphaText(popUp.GetComponent<RectTransform>(), 0f, duration)
                 .setOnComplete(() => Destroy(popUp));
    }
}