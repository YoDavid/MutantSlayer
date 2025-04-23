using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Arrived to checkpoint");
        if (other.CompareTag("Player"))
        {
            var dataCollector = other.GetComponent<PlayerDataCollector>();
            if (dataCollector != null)
            {
                CheckpointManager.Instance.SetCheckpoint(
                    transform.position,
                    dataCollector.GetCurrentPlayerData()
                );
            }
        }
    }
}