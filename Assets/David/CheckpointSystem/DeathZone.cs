using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var player = other.GetComponent<PlayerHealth>();
        if (player != null && !player.isDead)
        {
            player.KillPlayer();
           
        }
    }
}