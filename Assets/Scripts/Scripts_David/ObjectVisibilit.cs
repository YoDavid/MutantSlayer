using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectVisibility : MonoBehaviour
{
    public string playerTag = "Player";
    public GameObject targetObject;

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
            targetObject.SetActive(false);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            targetObject.SetActive(true);
        }
    }
}