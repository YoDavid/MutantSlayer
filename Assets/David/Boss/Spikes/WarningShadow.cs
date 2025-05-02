using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarningShadow : MonoBehaviour
{
    public float duration = 0.3f;
    private SpriteRenderer sprite;

    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();

        // Set the starting alpha and scale
        LeanTween.alpha(gameObject, 0.75f, duration).setFrom(0f);

        // Animate the scale to make it shrink over the duration
        LeanTween.scale(gameObject, Vector3.zero, duration).setEase(LeanTweenType.easeInQuad);

        // Destroy the warning shadow after the duration
        Destroy(gameObject, duration);
    }
}
