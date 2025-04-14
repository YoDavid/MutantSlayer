using System.Collections;
using UnityEngine;

public class EnemyHealth : HealthSystem
{
    [Header("Enemy Settings")]
    public EnemyConfig config;
    [SerializeField] private bool overrideHealth = false;
    [SerializeField] private int customMaxHealth = 30;

    private AudioManager audioManager;
    private Animator animator;

    private bool screamedAt75 = false;
    private bool screamedAt50 = false;
    private bool screamedAt25 = false;

    protected override void Awake()
    {
        MaxHealth = overrideHealth ? customMaxHealth : config.maxHealth;
        base.Awake();
        animator = GetComponent<Animator>();
        MaxHealth = config.maxHealth;
        CurrentHealth = MaxHealth;
        OnHealthChanged?.Invoke(CurrentHealth);
        GameObject audioObj = GameObject.Find("AudioManager");
        if (audioObj != null) audioManager = audioObj.GetComponent<AudioManager>();
    }

    public override void TakeDamage(int damage, bool isCritical = false)
    {
        if (CurrentHealth <= 0) return;

        base.TakeDamage(damage, isCritical);

        if (this.gameObject.name == "MediumEnemy")
        {
            MediumEnemyPlaySound();
        }
        else if (this.gameObject.name == "SmallEnemy")
        {
            SmallEnemyPlaySound();
        }

        DamagePopUp.Instance?.CreateDamageText(
        damage, transform.position + Vector3.up * 1.5f,
        isPlayer: false, isBoss: false, isCritical);

        if (CurrentHealth <= 0) Die();
    }

    private void MediumEnemyPlaySound()
    {
        float healthPercent = (float)CurrentHealth / MaxHealth;

        if (!screamedAt75 && healthPercent <= 0.75f)
        {
            audioManager.PlayMediumEnemyTakeHit();
            Debug.Log("Scream at 75%");
            screamedAt75 = true;
        }
        else if (!screamedAt50 && healthPercent <= 0.5f)
        {
            audioManager.PlayMediumEnemyTakeHit();
            Debug.Log("Scream at 50%");
            screamedAt50 = true;
        }
        else if (!screamedAt25 && healthPercent <= 0.25f)
        {
            audioManager.PlayMediumEnemyLastScream();
            Debug.Log("Scream at 25%");
            screamedAt25 = true;
        }

    }
    private void SmallEnemyPlaySound()
    {
        float healthPercent = (float)CurrentHealth / MaxHealth;

        if (!screamedAt75 && healthPercent <= 0.75f)
        {
            audioManager.PlaySmallEnemyTakeHit();
            Debug.Log("Scream at 75%");
            screamedAt75 = true;
        }
        else if (!screamedAt50 && healthPercent <= 0.5f)
        {
            audioManager.PlaySmallEnemyTakeHit();
            Debug.Log("Scream at 50%");
            screamedAt50 = true;
        }
        else if (!screamedAt25 && healthPercent <= 0.25f)
        {
            audioManager.PlaySmallEnemyLastScream();
            Debug.Log("Scream at 25%");
            screamedAt25 = true;
        }

    }

    protected override void Die()
    {
        GetComponent<Collider2D>().enabled = false;
        base.Die();
        Destroy(gameObject, 1f);
    }

    protected override IEnumerator BlinkEffect()
    {
        for (int i = 0; i < config.enemyBlinkCount; i++)
        {
            spriteRenderer.color = config.enemyBlinkColor;
            yield return new WaitForSeconds(config.enemyBlinkDuration);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(config.enemyBlinkDuration);
        }
    }
}
