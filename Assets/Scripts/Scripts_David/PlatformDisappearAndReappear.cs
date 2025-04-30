using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(TilemapCollider2D))]
[RequireComponent(typeof(TilemapRenderer))]
public class DisappearingPlatform : MonoBehaviour
{
    [Header("Flash Settings")]
    [SerializeField] private int numberOfFlashes = 3;
    [SerializeField] private float flashInterval = 0.2f;

    [Header("Disappear Settings")]
    [SerializeField] private float disappearDuration = 3f;

    private TilemapCollider2D _collider;
    private TilemapRenderer _renderer;
    private bool _isFlashing;
    private bool _isPlayerOnPlatform;
    private float _timer;
    private int _flashCount;

    private void Awake()
    {
        _collider = GetComponent<TilemapCollider2D>();
        _renderer = GetComponent<TilemapRenderer>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !_isFlashing)
        {
            // Check if player is landing on top of the platform
            if (collision.contacts[0].normal.y < -0.5f)
            {
                _isPlayerOnPlatform = true;
                StartFlashing();
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _isPlayerOnPlatform = false;
        }
    }

    private void StartFlashing()
    {
        if (_isFlashing) return;

        _isFlashing = true;
        _flashCount = 0;
        _timer = 0f;

        // Start flashing immediately
        Flash();
    }

    private void Flash()
    {
        _renderer.enabled = !_renderer.enabled;
        _flashCount++;

        if (_flashCount < numberOfFlashes * 2) // *2 because each flash is on+off
        {
            Invoke("Flash", flashInterval);
        }
        else
        {
            // Finished flashing, now disable the platform
            DisablePlatform();
        }
    }

    private void DisablePlatform()
    {
        _collider.enabled = false;
        _renderer.enabled = false;
        _isFlashing = false;

        // Start timer for reappearing
        Invoke("EnablePlatform", disappearDuration);
    }

    private void EnablePlatform()
    {
        _collider.enabled = true;
        _renderer.enabled = true;

        // If player is still on the platform (somehow), restart the flashing
        if (_isPlayerOnPlatform)
        {
            StartFlashing();
        }
    }
}