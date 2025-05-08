using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteEmissive : MonoBehaviour
{
    [Range(1, 5)] public float emissionStrength = 1.5f;

    void Start()
    {
        var renderer = GetComponent<SpriteRenderer>();
        renderer.material.EnableKeyword("_EMISSION");
        renderer.material.SetColor("_EmissionColor", Color.white * emissionStrength);
    }
}