using System.Collections;
using UnityEngine;

public class BossComboAttackHitbox : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private int _attackDamage = 10;
    [SerializeField] private float _activeDuration = 0.1f;

    [Header("Collider Settings")]
    [SerializeField] private float _colliderShift = 0.5f;
    [SerializeField] private Collider2D _attackCollider;
    private Vector2 _originalOffset;

    [Header("References")]
    [SerializeField] private PlayerHealth _playerHealth;
    [SerializeField] private Collider2D _playerHurtBox;
    [SerializeField] private CameraShake _cameraShake;
    private bool _isPlayerInRange = false;

    private void Awake()
    {
        if (_attackCollider == null)
            _attackCollider = GetComponent<Collider2D>();

        _cameraShake = CameraShake.Instance;

        if (_attackCollider is BoxCollider2D boxCollider)
            _originalOffset = boxCollider.offset;
    }

    private void Start()
    {
        var player = GameObject.FindWithTag("Player");
        if (player != null)
            _playerHealth = player.GetComponent<PlayerHealth>();

        var hurtBox = GameObject.FindWithTag("PlayerHurtBox");
        if (hurtBox != null)
            _playerHurtBox = hurtBox.GetComponent<Collider2D>();
    }

    public void ActivateComboAttackCollider(float[] timings)
    {
        StartCoroutine(ComboAttackSequence(timings));
    }

    private IEnumerator ComboAttackSequence(float[] timings)
    {
        float startTime = Time.time;

        foreach (float timing in timings)
        {
            float waitTime = timing - (Time.time - startTime);
            if (waitTime > 0)
                yield return new WaitForSeconds(waitTime);

            ToggleCollider(true);
            yield return new WaitForSeconds(_activeDuration);
            ToggleCollider(false);
        }
    }

    public void ForceDisable()
    {
        StopAllCoroutines();
        ToggleCollider(false);
    }

    private void ToggleCollider(bool state)
    {
        _attackCollider.enabled = state;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == _playerHurtBox && _playerHealth != null)
        {
            if (!_playerHealth.IsPlayerInvulnerable())
            {
                _playerHealth.TakeDamage(_attackDamage);
                _cameraShake.ShakeCamera(); 
            }
        }
    }

    public void FlipCollider(bool isFacingLeft)
    {
        if (_attackCollider is BoxCollider2D boxCollider)
        {
            boxCollider.offset = isFacingLeft
                ? new Vector2(_originalOffset.x - _colliderShift, _originalOffset.y)
                : new Vector2(_originalOffset.x + _colliderShift, _originalOffset.y);
        }
    }
}