using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    [Header("Normal Attack Shake Effect")]
    [SerializeField] private float normalAttackShakeDuration;
    [SerializeField] private float normalAttackMinshakeMagnitude;
    [SerializeField] private float normalAttackMaxshakeMagnitude;
    [SerializeField] private float normalAttackShakeMagnitude;
    [SerializeField] private float normalAttackDampingSpeed;

    [Header("Normal Attack Shake Effect")]
    [SerializeField] private float criticalAttackShakeDuration;
    [SerializeField] private float criticalAttackMinshakeMagnitude;
    [SerializeField] private float criticalAttackMaxshakeMagnitude;
    [SerializeField] private float criticalAttackShakeMagnitude;
    [SerializeField] private float criticalAttackDampingSpeed;

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

    public void NormalHitShakeCamera()
    {
        if (!isShaking)
        {
            Debug.Log("Shake");
            originalPosition = transform.position;
            StartCoroutine(Shake(normalAttackShakeDuration, normalAttackMinshakeMagnitude, normalAttackMaxshakeMagnitude, normalAttackShakeMagnitude, normalAttackDampingSpeed));
        }
    }

    public void CriticalHitShakeCamera()
    {
        if (!isShaking)
        {
            Debug.Log("Shake");
            originalPosition = transform.position;
            StartCoroutine(Shake(criticalAttackShakeDuration, criticalAttackMinshakeMagnitude, criticalAttackMaxshakeMagnitude, criticalAttackShakeMagnitude, criticalAttackDampingSpeed));
        }
    }

    public void ShakeCameraComboAttack()
    {
        if (!isShaking)
        {
            Debug.Log("Shake");
            originalPosition = transform.position;
            StartCoroutine(Shake(comboShakeDuration, comboMinShakeMagnitude, comboMaxShakeMagnitude, comboShakeMagnitude, comboDampingSpeed));
        }
    }

    public void ShakeCameraAOEAttack()
    {
        if (!isShaking)
        {
            Debug.Log("Shake");
            originalPosition = transform.position;
            StartCoroutine(Shake(aoeShakeDuration, aoeMinShakeMagnitude, aoeMaxShakeMagnitude, aoeShakeMagnitude, aoeDampingSpeed));
        }
    }

    public void ShakeCameraJumpSmashAttack()
    {
        if (!isShaking)
        {
            Debug.Log("Shake");
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
