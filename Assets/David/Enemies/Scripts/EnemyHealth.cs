using System.Collections;
using UnityEngine;

public class EnemyHealth : HealthSystem, IDamageable
{
    [Header("Enemy Settings")]
    public EnemyConfig config;
    [SerializeField] private bool overrideHealth = false;
    [SerializeField] private int customMaxHealth = 30;

    private AudioManager audioManager;
    private Animator animator;
    private DamageFlash damageFlash;

    private bool screamedAt75 = false;
    private bool screamedAt50 = false;
    private bool screamedAt25 = false;

    public enum EnemyType { Small, Medium }
    public EnemyType enemyType;

    [SerializeField] private PlayerLevelSystem playerLevelSystem;

    private EnemyLevelScaling enemyLevelScaling;

    protected override void Awake()
    {
        enemyLevelScaling = GetComponent<EnemyLevelScaling>();

        if (enemyLevelScaling != null)
        {
            enemyLevelScaling.ApplyInitialScaling();
        }
        else
        {
            MaxHealth = overrideHealth ? customMaxHealth : config.maxHealth;
        }

        base.Awake();

        animator = GetComponent<Animator>();
        CurrentHealth = MaxHealth;
        OnHealthChanged?.Invoke(CurrentHealth);

        GameObject audioObj = GameObject.Find("AudioManager");
        if (audioObj != null)
            audioManager = audioObj.GetComponent<AudioManager>();

        damageFlash = GetComponent<DamageFlash>();
    }

    private void Start()
    {
        if (bloodSplashPool == null)
            bloodSplashPool = FindObjectOfType<BloodSplashParticlesPool>();
    }

    public void TakeDamage(int damage, bool isCritical = false, bool isCombo = false, int comboCount = 0)
    {
        if (CurrentHealth <= 0) return;

        base.TakeDamage(damage, isCritical);

        PlayHitSound();
        damageFlash.CallDamageFlash();

        ShowDamagePopup(damage, transform.position, isCritical, isCombo, comboCount);

        OnHealthChanged?.Invoke(CurrentHealth);

        if (CurrentHealth <= 0)
            Die();
    }

    private void ShowDamagePopup(int damage, Vector3 position, bool isCritical, bool isCombo = false, int comboIndex = 0)
    {
        if (DamagePopUp.Instance == null) return;

        Vector3 offset = isCombo
            ? new Vector3(comboIndex * 0.5f, comboIndex * 0.3f, 0)
            : Vector3.up * 1.5f;

        DamagePopUp.Instance.CreateDamageText(
            damage,
            position + offset,
            isPlayer: false,
            isBoss: false,
            isCritical: isCritical,
            isCombo: isCombo,
            comboIndex: comboIndex
        );
    }

    private void PlayHitSound()
    {
        float healthPercent = (float)CurrentHealth / MaxHealth;

        if (gameObject.name == "MediumEnemy")
        {
            if (!screamedAt75 && healthPercent <= 0.75f)
            {
                audioManager?.PlayMediumEnemyTakeHit();
                screamedAt75 = true;
            }
            else if (!screamedAt50 && healthPercent <= 0.5f)
            {
                audioManager?.PlayMediumEnemyTakeHit();
                screamedAt50 = true;
            }
            else if (!screamedAt25 && healthPercent <= 0.25f)
            {
                audioManager?.PlayMediumEnemyLastScream();
                screamedAt25 = true;
            }
        }
        else if (gameObject.name == "SmallEnemy")
        {
            if (!screamedAt75 && healthPercent <= 0.75f)
            {
                audioManager?.PlaySmallEnemyTakeHit();
                screamedAt75 = true;
            }
            else if (!screamedAt50 && healthPercent <= 0.5f)
            {
                audioManager?.PlaySmallEnemyTakeHit();
                screamedAt50 = true;
            }
            else if (!screamedAt25 && healthPercent <= 0.25f)
            {
                audioManager?.PlaySmallEnemyLastScream();
                screamedAt25 = true;
            }
        }
    }

    protected override void Die()
    {
        if (CurrentHealth > 0) return;

        GetComponent<Collider2D>().enabled = false;

        if (bloodSplashPool != null)
            bloodSplashPool.PlayDeathSplash(transform.position);

        if (enemyLevelScaling != null)
        {
            int expReward = enemyLevelScaling.GetExpReward();
            if (playerLevelSystem != null)
            {
                playerLevelSystem.AddExperience(expReward);
            }
        }

        GameObject toDestroy = transform.parent != null ? transform.parent.gameObject : gameObject;
        Destroy(toDestroy, 0.1f);

        OnDeath?.Invoke();
    }
}
