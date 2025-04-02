using UnityEngine;

public class PlayerHurtbox : MonoBehaviour
{
    [SerializeField] private Collider2D hurtboxCollider; // Reference to the hurtbox collider

    private void Awake()
    {
        // Get the collider attached to the same GameObject
        hurtboxCollider = GetComponent<Collider2D>();
    }

    // Set invincibility status
    public void SetInvincible(bool isInvincible)
    {
        // Enable or disable the hurtbox collider depending on invincibility
        hurtboxCollider.enabled = !isInvincible;  // Disable the collider when invincible
    }
}
