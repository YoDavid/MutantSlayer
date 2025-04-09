using UnityEngine;

[CreateAssetMenu(fileName = "DamageConfig", menuName = "Combat/Damage Configuration")]
public class DamageConfig : ScriptableObject
{
    [Header("Normal Attack")]
    public Vector2Int normalDamageRange = new Vector2Int(5, 10);
    public float attackCooldown = 0.5f;

    [Header("Critical Attack")]
    [Range(0, 1)] public float criticalChance = 0.2f;
    public Vector2Int criticalDamageRange = new Vector2Int(15, 20);
}