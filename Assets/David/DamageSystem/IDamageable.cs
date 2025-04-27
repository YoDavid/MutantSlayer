public interface IDamageable
{
    void TakeDamage(int damage, bool isCritical = false, bool isCombo = false, int comboCount = 0);
}