using UnityEngine;

public class BossFlipHandlerTesting : MonoBehaviour
{
    private Transform player;
    private bool isFacingLeft;
    private SpriteRenderer spriteRenderer;
    private BossComboAttackHitboxTesting bossAttackHitbox;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        bossAttackHitbox = GetComponentInChildren<BossComboAttackHitboxTesting>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    public void FlipTowardsPlayer()
    {
        if (player == null) return;

        if (player.position.x < transform.position.x)
        {
            spriteRenderer.flipX = false;
            isFacingLeft = true;
            bossAttackHitbox.FlipCollider(false);
        }
        else
        {
            spriteRenderer.flipX = true;
            isFacingLeft = false;
            bossAttackHitbox.FlipCollider(true);
        }
    }

    public bool IsFacingLeft => isFacingLeft;
}
