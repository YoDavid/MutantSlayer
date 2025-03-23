using System.Collections;
using UnityEngine;

public class BossComboAttackHitbox : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private int attackDamage;
    [SerializeField] private float attackDuration;
    [SerializeField] private float[] attackTimings;

    [Header("Collider Settings")]
    [SerializeField] private float colliderShift;
    private Collider2D attackCollider;
    private Vector2 originalOffset;

    [Header("Player References")]
    [SerializeField] private PlayerHealth playerHealth;
    private Collider2D playerHurtBoxCollider;
    private bool isPlayerInRange = false;

    [Header("Camera Shake")]
    private CameraShake cameraShake;

    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        SetupCollider();
        FindPlayerReferences();
    }

    private void InitializeComponents()
    {
        attackCollider = GetComponent<Collider2D>();
        if (attackCollider == null)
        {
            Debug.LogError("Attack Collider is not attached to the BossAttackHitbox object.");
        }

        cameraShake = FindAnyObjectByType<CameraShake>();
    }

    private void SetupCollider()
    {
        attackCollider.enabled = false;

        if (attackCollider is BoxCollider2D boxCollider)
        {
            originalOffset = boxCollider.offset;
        }
    }

    private void FindPlayerReferences()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerHurtBoxCollider = player.GetComponent<Collider2D>();
            playerHealth = player.GetComponent<PlayerHealth>();
        }
        else
        {
            Debug.LogError("Player not found in scene.");
        }
    }

    public void ActivateComboAttackCollider()
    {
        StartCoroutine(ActivateComboWithIntervals());
    }

    private IEnumerator ActivateComboWithIntervals()
    {
        float startTime = Time.time;

        foreach (float attackTime in attackTimings)
        {
            float waitTime = attackTime - (Time.time - startTime);
            if (waitTime > 0)
                yield return new WaitForSeconds(waitTime);

            EnableCollider();
            yield return new WaitForSeconds(attackDuration);
            DisableCollider();
        }
    }

    private void EnableCollider()
    {
        attackCollider.enabled = true;
    }

    private void DisableCollider()
    {
        attackCollider.enabled = false;
    }

    private void ApplyDamage()
    {
        if (isPlayerInRange && !playerHealth.IsPlayerInvulnerable())
        {
            playerHealth.TakeDamage(attackDamage);
            cameraShake.ShakeCameraComboAttack();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == playerHurtBoxCollider)
        {
            isPlayerInRange = true;
            ApplyDamage();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == playerHurtBoxCollider)
        {
            isPlayerInRange = false;
        }
    }

    private void OnDrawGizmos()
    {
        if (attackCollider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(attackCollider.bounds.center, attackCollider.bounds.size);
        }
    }

    public void FlipCollider(bool isFlipped)
    {
        if (attackCollider is BoxCollider2D boxCollider)
        {
            boxCollider.offset = isFlipped
                ? new Vector2(originalOffset.x + colliderShift, originalOffset.y)
                : originalOffset;
        }
    }
}
