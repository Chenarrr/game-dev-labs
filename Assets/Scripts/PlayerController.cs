using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;

    private Rigidbody2D rb;
    private bool isDead = false;
    private int groundContactCount = 0;
    private bool IsGrounded => groundContactCount > 0;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (isDead || (GameManager.Instance != null && GameManager.Instance.isGameOver)) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        float moveInput = 0f;
        if (kb.aKey.isPressed) moveInput = -1f;
        if (kb.dKey.isPressed) moveInput =  1f;
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        if (kb.enterKey.wasPressedThisFrame && IsGrounded)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
            groundContactCount++;
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
            groundContactCount--;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Obstacle"))
            Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        if (GameManager.Instance != null)
            GameManager.Instance.GameOver();
    }
}
