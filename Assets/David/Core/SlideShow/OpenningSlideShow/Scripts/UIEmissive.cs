using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIEmissive : MonoBehaviour
{
    [Range(1, 5)] public float emissionStrength = 1.5f;
    private Material emissiveMaterial;

    void Start()
    {
        // Create a copy of the material
        var image = GetComponent<Image>();
        emissiveMaterial = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default"));
        image.material = emissiveMaterial;

        // Set emission properties
        emissiveMaterial.EnableKeyword("_EMISSION");
        emissiveMaterial.SetColor("_EmissionColor", Color.white * emissionStrength);
    }
}