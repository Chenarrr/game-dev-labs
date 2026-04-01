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
    [SerializeField] private float jumpCutMultiplier  = 0.4f;  // tap = short hop, hold = full jump
    [SerializeField] private float fallMultiplier     = 2.5f;  // fall faster than rise
    [SerializeField] private float coyoteTime         = 0.1f;

    private Rigidbody2D rb;
    private bool  isDead         = false;
    private int   groundContacts = 0;
    private float coyoteCounter  = 0f;
    private bool  IsGrounded     => groundContacts > 0;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    void Update()
    {
        if (isDead) return;
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        // ── Horizontal: A/D + arrow keys ────────────────────────────────────
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

        // ── Jump: Space or Up arrow ONLY ─────────────────────────────────────
        bool jumpPressed   = kb.spaceKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame;
        bool jumpReleased  = kb.spaceKey.wasReleasedThisFrame || kb.upArrowKey.wasReleasedThisFrame;

        if (jumpPressed && coyoteCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            coyoteCounter = 0f;
        }

        // Cut jump on release for variable height
        if (jumpReleased && rb.linearVelocity.y > 0f)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);

        // ── Fall boost ───────────────────────────────────────────────────────
        if (rb.linearVelocity.y < 0f)
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1f) * Time.deltaTime;
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
        if (GameManager.Instance != null)
            GameManager.Instance.GameOver();
    }
}
