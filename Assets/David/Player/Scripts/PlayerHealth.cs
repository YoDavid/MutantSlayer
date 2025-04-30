using UnityEngine;
using System.Collections;

public class PlayerHealth : HealthSystem
{
    [Header("Player Status")]
    public bool isDead = false;
    [SerializeField] private float invulnerabilityTime = 0.5f;

    private PlayerMovementController playerMovement;
    private PlayerHurtbox playerHurtbox;
    private PlayerAnimationController playerAnimation;
    public bool isInvulnerable = false;

    private bool isHeartbeatPlaying = false;
    private Coroutine heartbeatCoroutine;

    [SerializeField] private float comboInvulnerabilityTime = 1.5f; // Combo-specific invulnerability time
    [SerializeField] private bool isComboInvulnerable = false;


    protected override void Awake()
    {
        base.Awake();
        playerMovement = GetComponent<PlayerMovementController>();
        playerHurtbox = GetComponentInChildren<PlayerHurtbox>();
        playerAnimation = GetComponent<PlayerAnimationController>();
    }

    private void Update()
    {
        CheckHeartbeat();
    }

    private void CheckHeartbeat()
    {
        float healthPercentage = (float)CurrentHealth / MaxHealth;
        bool shouldPlay = healthPercentage < 0.25f && !isDead;

        if (shouldPlay && !isHeartbeatPlaying)
        {
            // Start playing the 32-second heartbeat track
            AudioManager.Instance.PlayHeartbeatLowHP();
            isHeartbeatPlaying = true;

            // Start monitoring for when to replay
            if (heartbeatCoroutine != null) StopCoroutine(heartbeatCoroutine);
            heartbeatCoroutine = StartCoroutine(MonitorHeartbeat());
        }
        else if (!shouldPlay && isHeartbeatPlaying)
        {
            // Stop the heartbeat immediately
            StopHeartbeat();
        }
    }

    private IEnumerator MonitorHeartbeat()
    {
        while ((float)CurrentHealth / MaxHealth < 0.25f && !isDead)
        {
            // Wait for the full 32-second duration
            yield return new WaitForSeconds(32f);

            // If still below 25% health, play again
            if ((float)CurrentHealth / MaxHealth < 0.25f && !isDead)
            {
                AudioManager.Instance.PlayHeartbeatLowHP();
            }
        }

        // Health recovered or player died
        StopHeartbeat();
    }

    private void StopHeartbeat()
    {
        if (heartbeatCoroutine != null)
        {
            StopCoroutine(heartbeatCoroutine);
            heartbeatCoroutine = null;
        }
        AudioManager.Instance.StopSound("Status", "sfx_player_heartbeat_lowhp");
        isHeartbeatPlaying = false;
    }

    public override void TakeDamage(int damage, bool isCritical = false)
    {
        if (isDead || isInvulnerable || isComboInvulnerable || playerHurtbox == null || !playerHurtbox.enabled) return;

        base.TakeDamage(damage, isCritical);

        playerAnimation.TriggerTakenHit();
        PlayRandomTakeHitSound();
        DamagePopUp.Instance?.CreateDamageText(
            damage, transform.position + Vector3.up * 1.8f,
            isPlayer: true, isBoss: false, isCritical);

        // Check if the player is not grounded after taking damage
        if (!playerMovement.isGrounded)
        {
            // Reset the Rigidbody's velocity (X and Y axis)
            Rigidbody2D rb = playerMovement.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero; // Reset velocity
                Debug.Log("Rigidbody velocity reset due to being airborne after hit.");
            }
        }

        StartCoroutine(InvulnerabilityFrame());
        CheckHeartbeat();
    }



    private void PlayRandomTakeHitSound()
    {
        int rand = Random.Range(1, 4);
        switch (rand)
        {
            case 1:
                AudioManager.Instance.PlayPlayer_TakeHit00();
                break;
            case 2:
                AudioManager.Instance.PlayPlayer_TakeHit01();
                break;
            case 3:
                AudioManager.Instance.PlayPlayer_TakeHit02();
                break;
        }
    }

    private IEnumerator InvulnerabilityFrame()
    {
        isInvulnerable = true;
        if (playerHurtbox != null)
            playerHurtbox.SetInvincible(true); // Disable hurtbox collider

        // Use real-time instead of scaled time to prevent time slowing from affecting invulnerability
        yield return new WaitForSecondsRealtime(invulnerabilityTime);

        isInvulnerable = false;
        if (playerHurtbox != null)
            playerHurtbox.SetInvincible(false); // Enable hurtbox collider
    }


    protected override void Die()
    {
        if (isDead) return;

        isDead = true;
        playerAnimation.enabled = false;
        playerMovement.enabled = false;
        StopHeartbeat();
        base.Die();
    }

    public void KillPlayer()
    {
        Die();
    }

    public bool IsPlayerInvulnerable() => playerMovement.isDashing || isInvulnerable;

    public void Heal(int amount)
    {
        if (!playerMovement.isGrounded) return;

        CurrentHealth += amount;
        CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth);
        OnHealthChanged?.Invoke(CurrentHealth);
        CheckHeartbeat();

        if (playerAnimation != null)
        {
            playerAnimation.TriggerHealingAnimation();
            StartCoroutine(EndHealingAfterDelay(0.9f));
        }
    }

    private IEnumerator EndHealingAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        playerAnimation.animator.SetBool("IsHealing", false);
    }

    public void HealPercentage(float percentage)
    {
        if (!playerMovement.isGrounded) return;

        int healAmount = Mathf.FloorToInt(MaxHealth * percentage);

        CurrentHealth += healAmount;
        CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth);
        OnHealthChanged?.Invoke(CurrentHealth);
        CheckHeartbeat();

        if (playerAnimation != null)
        {
            playerAnimation.TriggerHealingAnimation();
            StartCoroutine(EndHealingAfterDelay(0.9f));
        }
    }

    public void SetHealth(int amount)
    {
        CurrentHealth = Mathf.Clamp(amount, 0, MaxHealth);
        OnHealthChanged?.Invoke(CurrentHealth);
        CheckHeartbeat();
    }

    public void Revive()
    {
        isDead = false;
        playerAnimation.enabled = true;
        playerMovement.enabled = true;
    }

    public void SetComboInvulnerability(bool value)
    {
        isComboInvulnerable = value;
        playerHurtbox?.SetInvincible(value);
    }

}