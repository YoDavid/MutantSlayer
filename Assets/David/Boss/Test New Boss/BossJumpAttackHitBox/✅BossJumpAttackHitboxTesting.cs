using System.Collections;
using UnityEngine;

public class BossJumpAttackHitboxTesting : MonoBehaviour
{
    [Header("Timings")]
    [SerializeField] private float attackDuration = 0.5f;
    [SerializeField] private float activationDelay = 0.2f;

    private BossJumpAttackColliderControllerTesting colliderController;
    private BossJumpAttackDamageHandlerTesting damageHandler;

    private void Awake()
    {
        colliderController = GetComponent<BossJumpAttackColliderControllerTesting>();
        damageHandler = GetComponent<BossJumpAttackDamageHandlerTesting>();
    }

    public void ActivateJumpAttackCollider(bool isFlipped)
    {
        colliderController.FlipCollider(isFlipped);
        StartCoroutine(ActivateWithDelay());
    }

    private IEnumerator ActivateWithDelay()
    {
        yield return new WaitForSeconds(activationDelay);

        colliderController.EnableCollider();
        yield return new WaitForSeconds(attackDuration);
        colliderController.DisableCollider();
    }
}
