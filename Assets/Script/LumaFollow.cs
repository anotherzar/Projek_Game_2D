using UnityEngine;

public class LumaFollow : MonoBehaviour
{
    public Transform target;

    public float speed = 3f;
    public float stopDistance = 1.5f;
    public float followDistance = 2f;

    public bool canFollow = true;

    private Animator animator;

    private Vector3 originalScale;

    private float lastDirection = 1f;

    private Vector3 lastTargetPosition;

    private bool isTurning = false;

    // TAMBAHAN
    private float turnCooldown = 0f;

    private bool isFollowing = false; // Track follow state

    void Start()
    {
        animator = GetComponent<Animator>();

        originalScale = transform.localScale;

        lastTargetPosition = target.position;
    }

    void Update()
    {
        if (!canFollow || target == null)
        {
            animator.SetBool("isWalking", false);
            return;
        }

        // COOLDOWN TURN
        if (turnCooldown > 0)
        {
            turnCooldown -= Time.deltaTime;
        }

        // DETEKSI GERAK AREN
        float movement = target.position.x - lastTargetPosition.x;

        if (movement > 0.01f)
        {
            lastDirection = 1f;
        }
        else if (movement < -0.01f)
        {
            lastDirection = -1f;
        }

        lastTargetPosition = target.position;

        // POSISI TARGET DI BELAKANG AREN
        Vector3 targetPosition =
            target.position - new Vector3(lastDirection * followDistance, 0, 0);

        float distance = Vector2.Distance(
            transform.position,
            targetPosition
        );

        // CEK APAKAH SUDAH SEJAJAR
        float relativePosition = target.position.x - transform.position.x;

        // SAAT AREN BALIK ARAH
        if (
            Mathf.Abs(relativePosition) < 0.2f &&
            !isTurning &&
            turnCooldown <= 0 &&
            isFollowing // Only turn if we were following
        )
        {
            isTurning = true;
            isFollowing = false; // Reset following state during turn
            turnCooldown = 0.3f;

            animator.SetBool("isWalking", false);

            // FLIP SETELAH SEJAJAR
            if (lastDirection > 0)
            {
                transform.localScale = new Vector3(
                    originalScale.x,
                    originalScale.y,
                    originalScale.z
                );
            }
            else
            {
                transform.localScale = new Vector3(
                    -originalScale.x,
                    originalScale.y,
                    originalScale.z
                );
            }

            return;
        }

        // SETELAH FLIP LANJUT FOLLOW
        if (distance > stopDistance + 0.1f)
        {
            // Only move and update animation if we're actually following
            if (!isTurning)
            {
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    targetPosition,
                    speed * Time.deltaTime
                );

                if (!animator.GetBool("isWalking"))
                {
                    animator.SetBool("isWalking", true);
                }

                isFollowing = true;
            }

            isTurning = false;
        }
        else
        {
            // Only update animation state if it needs to change
            if (animator.GetBool("isWalking"))
            {
                animator.SetBool("isWalking", false);
            }

            isFollowing = false;

            // IKUT ARAH AREN SAAT DIAM (only when not turning)
            if (!isTurning)
            {
                if (lastDirection > 0)
                {
                    transform.localScale = new Vector3(
                        originalScale.x,
                        originalScale.y,
                        originalScale.z
                    );
                }
                else
                {
                    transform.localScale = new Vector3(
                        -originalScale.x,
                        originalScale.y,
                        originalScale.z
                    );
                }
            }
        }
    }
}