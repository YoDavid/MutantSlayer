using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [Header("Shake Effect")]
    [SerializeField] private float shakeDuration;
    [SerializeField] private float minshakeMagnitude;
    [SerializeField] private float maxshakeMagnitude;
    [SerializeField] private float shakeMagnitude;
    [SerializeField] private float dampingSpeed;

    [Header("Combo Attack Shake Settings")]
    [SerializeField] private float comboShakeDuration;
    [SerializeField] private float comboMinShakeMagnitude;
    [SerializeField] private float comboMaxShakeMagnitude;
    [SerializeField] private float comboShakeMagnitude;
    [SerializeField] private float comboDampingSpeed;

    [Header("AOE Attack Shake Settings")]
    [SerializeField] private float aoeShakeDuration;
    [SerializeField] private float aoeMinShakeMagnitude;
    [SerializeField] private float aoeMaxShakeMagnitude;
    [SerializeField] private float aoeShakeMagnitude;
    [SerializeField] private float aoeDampingSpeed;

    [Header("Jump Smash Attack Shake Settings")]
    [SerializeField] private float jumpSmashShakeDuration;
    [SerializeField] private float jumpSmashMinShakeMagnitude;
    [SerializeField] private float jumpSmashMaxShakeMagnitude;
    [SerializeField] private float jumpSmashShakeMagnitude;
    [SerializeField] private float jumpSmashDampingSpeed;

    private Vector3 originalPosition;
    private bool isShaking = false;
    private float fixedZ;

    private Camera cameraComponent;

    void Start()
    {
        cameraComponent = GetComponent<Camera>();
        fixedZ = transform.position.z;
    }

    public void ShakeCamera()
    {
        if (!isShaking)
        {
            originalPosition = transform.position;
            StartCoroutine(Shake(shakeDuration, minshakeMagnitude, maxshakeMagnitude, shakeMagnitude, dampingSpeed));
        }
    }

    public void ShakeCameraComboAttack()
    {
        if (!isShaking)
        {
            originalPosition = transform.position;
            StartCoroutine(Shake(comboShakeDuration, comboMinShakeMagnitude, comboMaxShakeMagnitude, comboShakeMagnitude, comboDampingSpeed));
        }
    }

    public void ShakeCameraAOEAttack()
    {
        if (!isShaking)
        {
            originalPosition = transform.position;
            StartCoroutine(Shake(aoeShakeDuration, aoeMinShakeMagnitude, aoeMaxShakeMagnitude, aoeShakeMagnitude, aoeDampingSpeed));
        }
    }

    public void ShakeCameraJumpSmashAttack()
    {
        if (!isShaking)
        {
            originalPosition = transform.position;
            StartCoroutine(Shake(jumpSmashShakeDuration, jumpSmashMinShakeMagnitude, jumpSmashMaxShakeMagnitude, jumpSmashShakeMagnitude, jumpSmashDampingSpeed));
        }
    }

    private IEnumerator Shake(float duration, float minMagnitude, float maxMagnitude, float magnitude, float damping)
    {
        isShaking = true;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float x = Random.Range(minMagnitude, maxMagnitude) * magnitude;
            float y = Random.Range(minMagnitude, maxMagnitude) * magnitude;

            transform.position = new Vector3(originalPosition.x + x, originalPosition.y + y, fixedZ);

            elapsedTime += Time.deltaTime;
            magnitude = Mathf.Lerp(magnitude, 0, damping * Time.deltaTime);

            yield return null;
        }

        transform.position = originalPosition;
        isShaking = false;
    }
}
