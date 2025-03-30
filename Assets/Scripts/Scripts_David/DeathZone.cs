using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathZone : MonoBehaviour
{
    // טאג של השחקן
    public string playerTag = "Player";
    // נקודת ההתחלה של האזור
    public Transform startPoint;

    void OnTriggerEnter2D(Collider2D other)
    {
        // בדיקה אם האובייקט שנכנס הוא השחקן
        if (other.CompareTag(playerTag))
        {
            // העברת השחקן לנקודת ההתחלה
            other.transform.position = startPoint.position;

            // אופציונלי: הוספת אפקטים או פעולות נוספות (לדוגמה, הפעלת אנימציה)
            Debug.Log("Player died and respawned!");
        }
    }
}