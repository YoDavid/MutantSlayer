using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathZone : MonoBehaviour
{
    public string playerTag = "Player";
    public Transform startPoint;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            other.transform.position = startPoint.position;

            Debug.Log("Player died and respawned!");
        }
    }
}