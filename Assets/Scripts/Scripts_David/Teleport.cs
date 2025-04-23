using UnityEngine;

public class Teleport : MonoBehaviour
{
    public Transform targetLocation; 
    public string targetTag = "Player"; 

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(targetTag)) 
        {
            other.transform.position = targetLocation.position;
            Debug.Log("Player teleported!"); 
        }
    }
}