using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float maxSpeed     = 6f;
    [SerializeField] private float acceleration = 50f;
    [SerializeField] private float deceleration = 40f;

    [Header("Jump")]
    [SerializeField] private float jumpForce         = 12f;
    [SerializeField] private float jumpCutMultiplier  = 0.4f;
    [SerializeField] private float fallMultiplier     = 2.5f;
    [SerializeField] private float coyoteTime         = 0.1f;

    private Rigidbody2D rb;
    private bool  isDead         = false;
    private int   groundContacts = 0;
    private float coyoteCounter  = 0f;
    private bool  IsGrounded     => groundContacts > 0;

    // Squash & stretch
    private Vector3 baseScale;
    private Vector3 targetScale;
    private bool    wasGrounded = false;

    void Awake()
    {
        rb        = GetComponent<Rigidbody2D>();
        baseScale = transform.localScale;
        targetScale = baseScale;
    }

    void Update()
    {
        if (isDead) return;
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        // ── Horizontal ───────────────────────────────────────────────────────
        float input = 0f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  input = -1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) input =  1f;

        float targetVX = input * maxSpeed;
        float rate     = Mathf.Abs(input) > 0.01f ? acceleration : deceleration;
        rb.linearVelocity = new Vector2(
            Mathf.MoveTowards(rb.linearVelocity.x, targetVX, rate * Time.deltaTime),
            rb.linearVelocity.y);

        // ── Coyote time ──────────────────────────────────────────────────────
        if (IsGrounded) coyoteCounter = coyoteTime;
        else            coyoteCounter -= Time.deltaTime;

        // ── Jump ─────────────────────────────────────────────────────────────
        bool jumpPressed  = kb.spaceKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame;
        bool jumpReleased = kb.spaceKey.wasReleasedThisFrame || kb.upArrowKey.wasReleasedThisFrame;

        if (jumpPressed && coyoteCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            coyoteCounter = 0f;
            targetScale = new Vector3(baseScale.x * 0.7f, baseScale.y * 1.4f, baseScale.z); // stretch up
        }

        if (jumpReleased && rb.linearVelocity.y > 0f)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);

        // ── Fall boost ───────────────────────────────────────────────────────
        if (rb.linearVelocity.y < 0f)
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1f) * Time.deltaTime;

        // ── Squash on landing ────────────────────────────────────────────────
        bool justLanded = IsGrounded && !wasGrounded;
        if (justLanded)
            targetScale = new Vector3(baseScale.x * 1.4f, baseScale.y * 0.6f, baseScale.z); // squash

        wasGrounded = IsGrounded;

        // Smooth scale back to normal
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * 18f);
        if (IsGrounded && targetScale != baseScale &&
            Vector3.Distance(transform.localScale, targetScale) < 0.01f)
            targetScale = baseScale;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
            groundContacts++;
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
            groundContacts = Mathf.Max(0, groundContacts - 1);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Obstacle")) Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        // Death squash
        transform.localScale = new Vector3(baseScale.x * 1.5f, baseScale.y * 0.4f, baseScale.z);
        if (GameManager.Instance != null)
            GameManager.Instance.GameOver();
    }
}
