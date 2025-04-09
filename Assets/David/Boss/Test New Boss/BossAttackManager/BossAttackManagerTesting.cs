using System.Collections;
using UnityEngine;

public class BossAttackManagerTesting : MonoBehaviour
{
    private bool isAttacking;
    private BossAttackHandlerTesting attackHandler;  // Reference to BossAttackHandlerTesting
    public Transform player;  // Reference to the player

    private void Awake()
    {
        attackHandler = GetComponentInParent<BossAttackHandlerTesting>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;  // Find the player object in the scene
    }

    public void ComboAttackBehavior()
    {
        if (!isAttacking)
        {
            SetAttacking(true);
            StartCoroutine(WaitForAttack(attackHandler.comboAttackDuration));
        }
    }

    public void RangedAttackBehavior()
    {
        if (!isAttacking)
        {
            SetAttacking(true);
            StartCoroutine(WaitForAttack(attackHandler.rangedAttackDuration));
        }
    }

    public void AOEAttackBehavior()
    {
        if (!isAttacking)
        {
            SetAttacking(true);
            StartCoroutine(WaitForAttack(attackHandler.aoeAttackDuration));
        }
    }

    private IEnumerator WaitForAttack(float duration)
    {
        yield return new WaitForSeconds(duration);
        SetAttacking(false);
    }

    public void SetAttacking(bool attacking)
    {
        isAttacking = attacking;
    }

    public bool IsAttacking()
    {
        return isAttacking;
    }

    // Add this method to get the player reference
    public Transform GetPlayer()
    {
        return player;
    }

    public float GetAOEAttackDuration()
    {
        if (attackHandler != null)
        {
            return attackHandler.aoeAttackDuration;
        }
        return 0f;
    }
}
