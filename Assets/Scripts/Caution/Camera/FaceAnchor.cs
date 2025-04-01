using UnityEngine;

public class FaceAnchor : MonoBehaviour
{
    [Header("Base Settings")]
    public Vector3 baseOffset = new Vector3(0, 1.5f, -10); // Default face position
    public float followSharpness = 15f;

    [Header("Animation Offsets")]
    public Vector3 idleOffset =new Vector3(0f, 0, -10);
    public Vector3 runOffset = new Vector3(4f, -0.6f, -10);
    public Vector3 jumpOffset = new Vector3(3.2f, 0.8f, -10);
    public Vector3 fallOffset = new Vector3(3.2f, 0.8f, -10);
    public Vector3 slideOffset = new Vector3(-1f, -4.5f, -10);
    public Vector3 attack1Offset = new Vector3(3f, -2.15f, -10);
    public Vector3 attack2Offset = new Vector3(0.6f, -3.3f, -10);
    public Vector3 attack3Offset = new Vector3(6f, -3f, -10);

    private Animator animator;
    private Vector3 targetPosition;

    void Awake()
    {
        animator = GetComponentInParent<Animator>();
        targetPosition = baseOffset;
    }

    void LateUpdate()
    {
        // Determine current animation state
        Vector3 currentOffset = idleOffset;
        bool isAttacking = animator.GetBool("IsAttacking");

        if (isAttacking)
        {
            switch (animator.GetInteger("AttackCount"))
            {
                case 1: currentOffset += attack1Offset; break;
                case 2: currentOffset += attack2Offset; break;
                case 3: currentOffset += attack3Offset; break;
            }
        }
        else if (animator.GetBool("IsJumping"))
        {
            currentOffset += jumpOffset;
        }
        else if (animator.GetBool("IsFalling"))
        {
            currentOffset += fallOffset;
        }
        else if (animator.GetBool("IsDashing")) 
        {
            currentOffset += slideOffset;
        }
        else if (animator.GetFloat("Speed") > 0.1f)
        {
            currentOffset += runOffset;
        }

        targetPosition = baseOffset + currentOffset;

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetPosition,
            followSharpness * Time.deltaTime
        );

    }
}