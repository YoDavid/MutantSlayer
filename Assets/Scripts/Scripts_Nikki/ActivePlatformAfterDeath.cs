using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivePlatformAfterDeath : MonoBehaviour
{
    public GameObject objectToWatch; // האובייקט שהשמדה שלו מפעילה את הפלטפורמה
    private GameObject platform;

    void Start()
    {
        platform = gameObject; // האובייקט הנוכחי (הפלטפורמה)
        platform.SetActive(false); // כיבוי הפלטפורמה בהתחלה
    }

    void Update()
    {
        // בדיקה אם האובייקט שהשמדה שלו מפעילה את הפלטפורמה עדיין קיים
        if (objectToWatch == null)
        {
            platform.SetActive(true); // הפעלת הפלטפורמה
            Destroy(this); // השמדת הסקריפט כדי שלא יבדוק יותר
        }
    }
}