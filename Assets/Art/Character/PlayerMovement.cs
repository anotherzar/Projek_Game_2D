using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public bool canMove = true;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        if (rb != null) rb.freezeRotation = true;
    }

    void Update()
    {
        if (!canMove)
        {
            animator.SetBool("isWalking", false);
            // Pake velocity biasa biar universal
            if (rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }

        float move = 0;

        if (Keyboard.current.aKey.isPressed) move = -1;
        else if (Keyboard.current.dKey.isPressed) move = 1;

        // Gerak pake velocity (Anti-Nyeret)
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(move * speed, rb.linearVelocity.y);
        }
        else
        {
            transform.Translate(Vector2.right * move * speed * Time.deltaTime);
        }

        // Update Animasi
        animator.SetBool("isWalking", move != 0);

        // Flip Sprite
        if (move < 0) spriteRenderer.flipX = true;
        else if (move > 0) spriteRenderer.flipX = false;
    }

    public void EnableMove() { canMove = true; }
    public void DisableMove() { canMove = false; }
}