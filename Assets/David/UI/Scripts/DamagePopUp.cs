using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DamagePopUp : MonoBehaviour
{
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

    [Header("Combo Settings")]
    [SerializeField] private float comboSpacing = 0.5f;  // Space between combo numbers
    [SerializeField] private float comboVerticalSpread = 0.3f;
    [SerializeField] private float comboRandomness = 0.2f;  // Small position variation
    [SerializeField] private float comboDelay = 1f;  // Delay before showing combo numbers

    [Header("Flash Effect")]
    [SerializeField] private Image flashImage;

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

    public void CreateDamageText(int damage, Vector3 position, bool isPlayer, bool isBoss,
                               bool isCritical = false, bool isCombo = false, int comboIndex = 0)
    {
        if (isCombo)
        {
            // For combos, delay everything by comboDelay seconds
            LeanTween.delayedCall(comboDelay, () => {
                CreatePopUp(damage, position, isPlayer, isBoss, isCritical, isCombo, comboIndex);
            });
        }
        else
        {
            // For non-combos, create immediately
            CreatePopUp(damage, position, isPlayer, isBoss, isCritical, isCombo, comboIndex);
        }
    }

    private void CreatePopUp(int damage, Vector3 position, bool isPlayer, bool isBoss,
                           bool isCritical, bool isCombo, int comboIndex)
    {
        Vector3 spawnPosition = CalculateSpawnPosition(position, isPlayer, isBoss, isCombo, comboIndex);

        if (isCritical)
        {
            FlashScreen();
        }

        GameObject popUp = Instantiate(popUpPrefab, spawnPosition, Quaternion.identity);
        TextMeshPro text = popUp.GetComponent<TextMeshPro>();
        text.text = $"-{damage}";

        // Set visual properties
        text.color = isCritical ? critColor :
                    (isPlayer ? playerColor :
                    (isBoss ? bossColor : defaultColor));

        float startSize = isCritical ? 10f : normalTextSize;
        text.fontSize = startSize;

        if (isCritical)
        {
            LeanTween.value(text.gameObject, startSize, critTextSize, 0.2f)
                    .setOnUpdate((float val) => text.fontSize = val);
        }

        // Animation
        float floatDistance = isCritical ?
            Random.Range(critMinFloatDistance, critMaxFloatDistance) :
            Random.Range(normalMinFloatDistance, normalMaxFloatDistance);

        float duration = isCritical ?
            Random.Range(critMinDuration, critMaxDuration) :
            Random.Range(normalMinDuration, normalMaxDuration);

        LeanTween.moveY(popUp, spawnPosition.y + floatDistance, duration)
                .setEaseOutQuad();

        LeanTween.alphaText(popUp.GetComponent<RectTransform>(), 0f, duration)
                .setOnComplete(() => Destroy(popUp));
    }

    private Vector3 CalculateSpawnPosition(Vector3 position, bool isPlayer, bool isBoss, bool isCombo, int comboIndex)
    {
        float spawnHeight = isPlayer ? playerSpawnHeight :
                          (isBoss ? bossSpawnHeight : regularEnemiesHeight);

        Vector3 spawnPosition = position + Vector3.up * spawnHeight;

        if (isCombo)
        {
            // Calculate perfect left/right spreading
            int side = comboIndex % 2 == 0 ? 1 : -1; // Alternates sides
            int multiplier = (comboIndex + 1) / 2;    // Increases distance progressively

            spawnPosition += new Vector3(
                side * multiplier * comboSpacing + Random.Range(-comboRandomness, comboRandomness),
                Random.Range(-comboVerticalSpread, comboVerticalSpread),
                0
            );
        }
        else
        {
            // Regular random offset for non-combo attacks
            spawnPosition += new Vector3(
                Random.Range(-horizontalRandomness, horizontalRandomness),
                0,
                0
            );
        }

        return spawnPosition;
    }

    private void FlashScreen()
    {
        if (flashImage != null)
        {
            flashImage.color = new Color(1f, 1f, 1f, 1f);
            LeanTween.alpha(flashImage.rectTransform, 0f, 0.2f).setEaseOutQuad();
        }
    }
}