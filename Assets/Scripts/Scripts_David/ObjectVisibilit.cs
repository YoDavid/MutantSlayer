using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectVisibility : MonoBehaviour
{
    public string playerTag = "Player";
    public GameObject targetObject;
    private Coroutine delayedDisableCoroutine;
    private bool isPlayerInside = false;

    void Start()
    {
        if (targetObject == null)
        {
            Debug.LogError("Target object not assigned!");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInside = true;
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // If player is alive, hide immediately
                if (!playerHealth.isDead)
                {
                    targetObject.SetActive(false);
                }
                // If player is dead, do nothing (object stays visible)
            }
            else
            {
                targetObject.SetActive(false);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInside = false;
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null && playerHealth.isDead)
            {
                // If player was dead when exiting, start 3-second countdown
                if (delayedDisableCoroutine != null)
                {
                    StopCoroutine(delayedDisableCoroutine);
                }
                delayedDisableCoroutine = StartCoroutine(DelayedDisable());
            }
            else
            {
                targetObject.SetActive(true);
            }
        }
    }

    private IEnumerator DelayedDisable()
    {
        yield return new WaitForSeconds(3f);
        // Only disable if player is no longer inside (prevent race condition)
        if (!isPlayerInside)
        {
            targetObject.SetActive(false);
        }
        delayedDisableCoroutine = null;
    }
}