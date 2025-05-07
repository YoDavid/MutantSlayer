using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    public DamageConfig config;
    public float levelScaling = 0.2f; // Default scaling factor
    protected int level;

    public void SetLevel(int playerLevel)
    {
        level = playerLevel;
    }

    public virtual (int damage, bool isCritical) CalculateDamage()
    {
        bool isCritical = Random.value <= config.criticalChance;

        int baseDamage = isCritical ?
            Random.Range(config.criticalDamageRange.x, config.criticalDamageRange.y + 1) :
            Random.Range(config.normalDamageRange.x, config.normalDamageRange.y + 1);

        // Apply level scaling
        int scaledDamage = Mathf.RoundToInt(baseDamage * (1 + level * levelScaling));

        return (scaledDamage, isCritical);
    }
}

// This class is for your regular enemies, modifying damage scaling.
public class EnemyDamageDealer : DamageDealer
{
    // Override the CalculateDamage method for scaling specific to the enemy
    public override (int damage, bool isCritical) CalculateDamage()
    {
        bool isCritical = Random.value <= config.criticalChance;

        int baseDamage = isCritical ?
            Random.Range(config.criticalDamageRange.x, config.criticalDamageRange.y + 1) :
            Random.Range(config.normalDamageRange.x, config.normalDamageRange.y + 1);

        // Apply level scaling specific to enemies
        int scaledDamage = Mathf.RoundToInt(baseDamage * (1 + (level * 0.15f))); // Slightly different scaling for enemies

        return (scaledDamage, isCritical);
    }
}

// This class is for your bosses, which would have higher scaling.
public class BossDamageDealer : DamageDealer
{
    private BossLevelScaling bossScaling;

    private void Start()
    {
        bossScaling = GetComponentInParent<BossLevelScaling>();
    }

    public override (int damage, bool isCritical) CalculateDamage()
    {
        bool isCritical = Random.value <= GetCurrentCritChance();
        Vector2Int damageRange = isCritical ?
            config.criticalDamageRange :
            config.normalDamageRange;

        int baseDamage = Random.Range(damageRange.x, damageRange.y + 1);

        if (bossScaling == null)
            return (baseDamage, isCritical);

        // Apply both level and phase scaling
        int scaledDamage = Mathf.RoundToInt(baseDamage *
                         (1 + bossScaling.GetEffectiveLevel() * levelScaling) *
                         bossScaling.GetPhaseDamageMultiplier());

        return (scaledDamage, isCritical);
    }

    private float GetCurrentCritChance()
    {
        float bonus = bossScaling != null ? bossScaling.GetAdditionalCritChance() : 0f;
        return Mathf.Clamp01(config.criticalChance + bonus);
    }
}