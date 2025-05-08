using UnityEngine;

public class BossMusicTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Collider2D triggerCollider;
    [SerializeField] private Collider2D playerCollider;
    [SerializeField] private AudioManager audioManager;

    [Header("Music Settings")]
    [SerializeField] private bool stopAlternatingMusic = true;

    private void Awake()
    {
        // Set up trigger collider
        if (triggerCollider == null)
        {
            triggerCollider = GetComponent<Collider2D>();
            if (triggerCollider == null)
            {
                Debug.LogError("No collider found on BossMusicTrigger!", this);
            }
            else
            {
                triggerCollider.isTrigger = true;
            }
        }

        // Try to find AudioManager if not assigned
        if (audioManager == null)
        {
            audioManager = FindObjectOfType<AudioManager>();
            if (audioManager == null)
            {
                Debug.LogError("AudioManager not found in scene!", this);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (playerCollider != null && other != playerCollider) return;

        if (audioManager != null)
        {
            if (stopAlternatingMusic)
            {
                // Stop the alternating bg/bgv2 coroutine
                audioManager.StopAllCoroutines();
            }

            // Play boss battle music
            audioManager.PlayMusic("music_boss_battle");
        }
    }

    public void TriggerBossMusic()
    {
        if (audioManager != null)
        {
            if (stopAlternatingMusic)
            {
                audioManager.StopAllCoroutines();  // Stops any current music-changing coroutines
            }

            // Ensure the music fades out before playing the boss music
            audioManager.PlayMusic("music_boss_battle");  // This will trigger the fade effect from the previous track
        }
    }

}