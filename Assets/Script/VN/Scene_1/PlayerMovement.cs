using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public bool canMove = true;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private float moveInput = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        if (rb != null) rb.freezeRotation = true;
    }

    void Update()
    {
        // 1. Logika Permainan (Input & Animasi) berjalan di Update()
        if (!canMove)
        {
            moveInput = 0f;
            animator.SetBool("isWalking", false);
            return;
        }

        moveInput = 0f;
        if (Keyboard.current.aKey.isPressed) moveInput = -1f;
        else if (Keyboard.current.dKey.isPressed) moveInput = 1f;

        // Update Animasi & Flip Sprite
        animator.SetBool("isWalking", moveInput != 0);

        if (moveInput < 0) spriteRenderer.flipX = true;
        else if (moveInput > 0) spriteRenderer.flipX = false;
    }

    void FixedUpdate()
    {
        // 2. Logika Fisika (Rigidbody velocity) berjalan di FixedUpdate()
        if (!canMove)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);
        }
        else
        {
            transform.Translate(Vector2.right * moveInput * speed * Time.fixedDeltaTime);
        }
    }

    public void EnableMove() { canMove = true; }
    public void DisableMove() { canMove = false; }
}