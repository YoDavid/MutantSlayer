using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class EnemyColliderAdjuster : MonoBehaviour
{
    public EnemyConfig config;
    private BoxCollider2D mainCollider;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        mainCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        Vector2 targetOffset = spriteRenderer.flipX ? config.leftFacingColliderOffset : config.rightFacingColliderOffset;
        mainCollider.offset = targetOffset;
    }
}