using UnityEngine;
using System.Collections;

public class BossHealth : MonoBehaviour, IDamageable
{
    [Header("Settings")]
    [SerializeField] public int maxHealth = 500;
    [SerializeField] private Vector3 popupOffset = new Vector3(0, 2f, 0);

    [SerializeField] private BloodSplashParticlesPool bloodSplashPool;
    private DamageFlash damageFlash; // Reference to DamageFlash component

    public int currentHealth;

    public event System.Action<int> OnHealthChanged;
    public event System.Action OnDeath;
    public int MaxHealth => maxHealth;

    [Header("Level Scaling")]
    [SerializeField] private BossLevelScaling levelScaling;

    public void Initialize(int maxHP)
    {
        maxHealth = maxHP;
        currentHealth = maxHealth;
    }

    private void Awake()
    {
        AssignReferences();
        currentHealth = maxHealth;
    }

    public void ForceHealthUpdate()
    {
        OnHealthChanged?.Invoke(currentHealth);
    }

    private void AssignReferences()
    {
        // Assign DamageFlash reference
        damageFlash = GetComponent<DamageFlash>();
        if (damageFlash == null)
        {
            damageFlash = GetComponentInChildren<DamageFlash>();
            if (damageFlash == null)
            {
                Debug.LogError("DamageFlash component missing on boss or its children!");
            }
        }
    }

    public void TakeDamage(int damage, bool isCritical = false, bool isCombo = false, int comboCount = 0)
    {
        currentHealth -= damage;

        OnHealthChanged?.Invoke(currentHealth);

        if (DamagePopUp.Instance != null)
        {
            Vector3 spawnPosition = transform.position + popupOffset;

            if (isCombo)
            {
                // Apply combo-specific offset
                spawnPosition += new Vector3(
                    comboCount * 0.5f,  // Horizontal spacing
                    comboCount * 0.3f,  // Vertical offset
                    0
                );
            }
            else
            {
                // Add some randomness for regular hits
                spawnPosition += new Vector3(
                    Random.Range(-0.5f, 0.5f),
                    0,
                    0
                );
            }

            DamagePopUp.Instance.CreateDamageText(
                damage,
                spawnPosition,
                isPlayer: false,
                isBoss: true,
                isCritical: isCritical,
                isCombo: isCombo,
                comboIndex: comboCount
            );
        }

        if (damageFlash != null)
        {
            damageFlash.CallDamageFlash();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (bloodSplashPool != null)
        {
            bloodSplashPool.PlayDeathSplash(transform.position);
        }
        OnDeath?.Invoke();
        Destroy(gameObject);
    }
}