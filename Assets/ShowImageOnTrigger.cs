using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowMultipleImagesOnTrigger : MonoBehaviour
{
    public string playerTag = "Player"; // תג האובייקט של השחקן
    public GameObject[] imagesToShow; // מערך של אובייקטים של תמונות שיוצגו

    void Start()
    {
        // ודא שכל התמונות מוסתרות בהתחלה
        if (imagesToShow != null)
        {
            foreach (GameObject image in imagesToShow)
            {
                if (image != null)
                {
                    image.SetActive(false);
                }
                else
                {
                    Debug.LogWarning("One of the Image To Show elements is null on " + gameObject.name);
                }
            }
        }
        else
        {
            Debug.LogError("Images To Show array is not assigned on " + gameObject.name);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // בדוק אם האובייקט שנכנס הוא השחקן
        if (other.CompareTag(playerTag) && imagesToShow != null)
        {
            // הצג את כל התמונות במערך
            foreach (GameObject image in imagesToShow)
            {
                if (image != null)
                {
                    image.SetActive(true);
                }
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // אופציונלי: הסתר את כל התמונות כשהשחקן יוצא מהקוליידר
        if (other.CompareTag(playerTag) && imagesToShow != null)
        {
            // הסתר את כל התמונות במערך
            foreach (GameObject image in imagesToShow)
            {
                if (image != null)
                {
                    image.SetActive(false);
                }
            }
        }
    }
}