using System;
using UnityEngine;

public class PlayerAttackHitbox : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private DamageConfig damageConfig; 
    [SerializeField] private float hitCooldown = 0.3f;
    [SerializeField] private BloodSplashParticlesPool bloodSplashPool;

    private float lastHitTime;

    private PlayerLevelSystem playerLevelSystem; 
    private AudioManager audioManager;
    private CameraShake camerShake;

    public event Action<bool, bool> OnHit; 

    private void Awake()
    {
        playerLevelSystem = GetComponentInParent<PlayerLevelSystem>(); // Assuming it's on the parent GameObject

        if (playerLevelSystem == null)
        {
            Debug.LogError("PlayerLevelSystem not found!");
        }

        camerShake = GameObject.FindObjectOfType<CameraShake>();

        GameObject audioObj = GameObject.Find("AudioManager");
        if (audioObj != null)
        {
            audioManager = audioObj.GetComponent<AudioManager>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Time.time < lastHitTime + hitCooldown) return;

        bool isBoss = false;
        bool hitSuccess = false;
        bool isCritical = false;
        Vector3 hitPosition = transform.position;

        int damage = playerLevelSystem.GetScaledDamage("normal");

        float randomVariation = UnityEngine.Random.Range(-0.2f, 0.2f);
        damage = Mathf.RoundToInt(damage * (1f + randomVariation));

        float critChance = playerLevelSystem.GetCritChance();
        float critMultiplier = playerLevelSystem.GetCritMultiplier();

        if (other.TryGetComponent<EnemyHealth>(out var enemyHealth))
        {
            isCritical = UnityEngine.Random.value <= critChance;
            if (isCritical)
            {
                damage = Mathf.RoundToInt(damage * critMultiplier);
            }

            enemyHealth.TakeDamage(damage, isCritical);
           // CreateComboDamagePopUp(damage, other.transform.position, 0, isCritical, false);

            hitSuccess = true;
            hitPosition = other.transform.position;

        }
        else if (other.TryGetComponent<BossHealth>(out var bossHealth))
        {
            isCritical = UnityEngine.Random.value <= critChance;
            if (isCritical)
            {
                damage = Mathf.RoundToInt(damage * critMultiplier);
            }

            bossHealth.TakeDamage(damage, isCritical);
            //CreateComboDamagePopUp(damage, other.transform.position, 0, isCritical, true);

            isBoss = true;
            hitSuccess = true;
            hitPosition = other.transform.position;
        }

        if (hitSuccess)
        {
            lastHitTime = Time.time;
            PlayRandomHitSound();
            SpawnHitParticles(hitPosition);
            OnHit?.Invoke(isCritical, isBoss);

          

            if (camerShake != null)
            {
                if (isCritical)
                {
                    camerShake.CriticalHitShakeCamera();
                }
                else
                {
                    camerShake.NormalHitShakeCamera();
                }
            }
        }
    }

    private void CreateComboDamagePopUp(int damage, Vector3 position, int hitIndex, bool isCritical, bool isBoss)
    {
        if (DamagePopUp.Instance == null) return;

        float xOffset = hitIndex * 0.5f;
        float yOffset = hitIndex * 0.3f;
        Vector3 popUpPosition = position + new Vector3(xOffset, yOffset, 0);

        DamagePopUp.Instance.CreateDamageText(
            damage,
            popUpPosition,
            isPlayer: true,
            isBoss: isBoss,
            isCritical,
            isCombo: true,
            comboIndex: hitIndex);
    }

    private void PlayRandomHitSound()
    {
        if (audioManager == null) return;

        int rand = UnityEngine.Random.Range(0, 4); // Explicitly use UnityEngine.Random
        string clipName = $"sfx_player_attack_hit_0{rand}";
        audioManager.PlaySFX("Player", clipName);
    }

    private void SpawnHitParticles(Vector3 position)
    {
        if (bloodSplashPool != null)
        {
            position.y -= 2f;
            bloodSplashPool.PlayHitSplash(position);
        }
    }
}
