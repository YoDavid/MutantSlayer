using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    public DamageConfig config;

    public (int damage, bool isCritical) CalculateDamage()
    {
        bool isCritical = Random.value <= config.criticalChance;
        int damage = isCritical ?
            Random.Range(config.criticalDamageRange.x, config.criticalDamageRange.y + 1) :
            Random.Range(config.normalDamageRange.x, config.normalDamageRange.y + 1);

        return (damage, isCritical);
    }


}