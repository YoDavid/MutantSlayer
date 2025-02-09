using UnityEngine;

public class Teleport : MonoBehaviour
{
    public Transform targetLocation; // מיקום הטלפרוט
    public string targetTag = "Player"; // תגית של האובייקט שיתטפל

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(targetTag)) // בדיקה אם האובייקט הוא בעל התגית הרצויה
        {
            other.transform.position = targetLocation.position; // שינוי מיקום
            Debug.Log("Player teleported!"); // הודעה לקונסול
        }
    }
}