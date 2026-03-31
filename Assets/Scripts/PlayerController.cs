using UnityEngine;

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

        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        bool jumpPressed = Input.GetKeyDown(KeyCode.Space)
                        || Input.GetKeyDown(KeyCode.W)
                        || Input.GetKeyDown(KeyCode.UpArrow);

        if (jumpPressed && IsGrounded)
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
