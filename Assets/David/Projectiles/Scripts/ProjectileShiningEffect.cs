using UnityEngine;

public class ShiningEffect : MonoBehaviour
{
    [Header("Shine Settings")]
    [ColorUsage(true, true)]
    public Color shineColor = Color.white;
    [Range(0, 1)] public float minShineAmount = 0.1f;
    [Range(0, 1)] public float maxShineAmount = 0.5f;
    public float pulseSpeed = 1f;
    public float intensity = 1f;

    [Header("Material Properties")]
    public string colorPropertyName = "_FlashColor";
    public string amountPropertyName = "_FlashAmount";

    private Material[] _materials;
    private Color[] _originalColors;
    private bool _isInitialized = false;

    private void Awake()
    {
        InitializeMaterials();
    }

    private void InitializeMaterials()
    {
        var spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        _materials = new Material[spriteRenderers.Length];
        _originalColors = new Color[spriteRenderers.Length];

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            // Create instance material to avoid affecting other objects
            _materials[i] = new Material(spriteRenderers[i].material);
            spriteRenderers[i].material = _materials[i];

            // Store original color
            if (_materials[i].HasProperty(colorPropertyName))
            {
                _originalColors[i] = _materials[i].GetColor(colorPropertyName);
            }
        }
        _isInitialized = true;
    }

    private void Update()
    {
        if (!_isInitialized) return;

        // Pulsing effect
        float pulse = (Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f);
        float currentAmount = Mathf.Lerp(minShineAmount, maxShineAmount, pulse) * intensity;

        // Apply to all materials
        for (int i = 0; i < _materials.Length; i++)
        {
            if (_materials[i].HasProperty(colorPropertyName))
            {
                _materials[i].SetColor(colorPropertyName, shineColor);
            }
            if (_materials[i].HasProperty(amountPropertyName))
            {
                _materials[i].SetFloat(amountPropertyName, currentAmount);
            }
        }
    }

    private void OnDestroy()
    {
        // Clean up instantiated materials if needed
        if (_materials != null)
        {
            foreach (var mat in _materials)
            {
                if (mat != null)
                {
                    Destroy(mat);
                }
            }
        }
    }

    // Public method to update shine color
    public void SetShineColor(Color newColor)
    {
        shineColor = newColor;
    }

    // Public method to update all shine parameters
    public void SetShineParameters(Color color, float newIntensity, float newPulseSpeed,
                                 float newMinAmount, float newMaxAmount)
    {
        shineColor = color;
        intensity = newIntensity;
        pulseSpeed = newPulseSpeed;
        minShineAmount = newMinAmount;
        maxShineAmount = newMaxAmount;
    }
}