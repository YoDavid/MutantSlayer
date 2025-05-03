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
            if (targetObject != null)
            {
                if (playerHealth != null)
                {
                    if (!playerHealth.isDead)
                    {
                        targetObject.SetActive(false);
                    }
                }
                else
                {
                    targetObject.SetActive(false);
                }
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
                if (delayedDisableCoroutine != null)
                {
                    StopCoroutine(delayedDisableCoroutine);
                }
                delayedDisableCoroutine = StartCoroutine(DelayedDisable());
            }
            else if (targetObject != null)
            {
                targetObject.SetActive(true);
            }
        }
    }

    private IEnumerator DelayedDisable()
    {
        yield return new WaitForSeconds(3f);
        if (!isPlayerInside && targetObject != null)
        {
            targetObject.SetActive(false);
        }
        delayedDisableCoroutine = null;
    }
}