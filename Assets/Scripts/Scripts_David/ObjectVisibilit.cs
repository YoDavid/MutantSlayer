using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectVisibility : MonoBehaviour
{
    // טאג של השחקן
    public string playerTag = "Player";
    // האובייקט שיועלם או יופיע
    public GameObject targetObject;

    void Start()
    {
        // ודא שהאובייקט קיים
        if (targetObject == null)
        {
            Debug.LogError("Target object not assigned!");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // בדיקה אם השחקן נכנס לאזור
        if (other.CompareTag(playerTag))
        {
            // העלמת האובייקט
            targetObject.SetActive(false);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // בדיקה אם השחקן יצא מהאזור
        if (other.CompareTag(playerTag))
        {
            // הופעת האובייקט
            targetObject.SetActive(true);
        }
    }
}