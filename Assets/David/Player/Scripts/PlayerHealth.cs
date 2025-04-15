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
    private bool isInvulnerable = false;


    protected override void Awake()
    {
        // No need to set MaxHealth here - it's set in the inspector
        base.Awake();
        playerMovement = GetComponent<PlayerMovementController>();
        playerHurtbox = GetComponentInChildren<PlayerHurtbox>();
        playerAnimation = GetComponent<PlayerAnimationController>();
    }

    public override void TakeDamage(int damage, bool isCritical = false)
    {
        if (isDead || isInvulnerable || playerHurtbox == null || !playerHurtbox.enabled) return;

        base.TakeDamage(damage, isCritical);

        playerAnimation.TriggerTakenHit();
        PlayRandomTakeHitSound(); // <-- Random sound here
        DamagePopUp.Instance?.CreateDamageText(
            damage, transform.position + Vector3.up * 1.8f,
            isPlayer: true, isBoss: false, isCritical);

        StartCoroutine(InvulnerabilityFrame());
    }

    private void PlayRandomTakeHitSound()
    {
        int rand = Random.Range(1, 4);
        switch (rand)
        {
            case 1:
                AudioManager.Instance.PlayPlayer_TakeHit01();
                break;
            case 2:
                AudioManager.Instance.PlayPlayer_TakeHit02();
                break;
            case 3:
                AudioManager.Instance.PlayPlayer_TakeHit03();
                break;
        }
    }



    private IEnumerator InvulnerabilityFrame()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerabilityTime);
        isInvulnerable = false;
    }

    protected override void Die()
    {
        if (isDead) return;

        isDead = true;
        playerAnimation.enabled = false;
        playerMovement.enabled = false;
        base.Die();
       
        Debug.Log("Player died!");
    }

    public bool IsPlayerInvulnerable() => playerMovement.isDashing || isInvulnerable;

    public void Heal(int amount)
    {
        CurrentHealth += amount;
        OnHealthChanged?.Invoke(CurrentHealth);
    }
}