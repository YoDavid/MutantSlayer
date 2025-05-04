using System.Collections;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] protected int _maxHealth = 100;
    [SerializeField] protected int _currentHealth;

    [Header("Death Effects")]
    [SerializeField] protected GameObject bloodSplashPrefab;
    [SerializeField] protected BloodSplashParticlesPool bloodSplashPool;

    public int MaxHealth
    {
        get => _maxHealth;
        set
        {
            _maxHealth = Mathf.Max(1, value);
            if (Application.isPlaying)
            {
                CurrentHealth = Mathf.Min(CurrentHealth, _maxHealth);
            }
        }
    }

    public int CurrentHealth
    {
        get => _currentHealth;
        protected set => _currentHealth = Mathf.Clamp(value, 0, _maxHealth);
    }

    protected SpriteRenderer spriteRenderer;

    public System.Action OnDeath;
    public System.Action<int> OnHealthChanged;

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        CurrentHealth = _maxHealth; // Initialize with serialized value
    }

    public virtual void TakeDamage(int damage, bool isCritical = false)
    {
        CurrentHealth -= damage;
        OnHealthChanged?.Invoke(CurrentHealth);
        if (CurrentHealth <= 0) Die();
    }

    protected virtual void Die()
    {
        if (CurrentHealth > 0) return;

        Debug.Log("Die method called");
        if (bloodSplashPool != null)
        {
            Debug.Log("Playing death splash...");
            bloodSplashPool.PlayDeathSplash(transform.position);
        }
        Destroy(gameObject);
        OnDeath?.Invoke();
    }
}