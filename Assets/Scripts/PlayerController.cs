using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float maxSpeed     = 8f;
    [SerializeField] private float acceleration = 60f;
    [SerializeField] private float deceleration = 50f;

    [Header("Jump")]
    [SerializeField] private float jumpForce        = 13f;
    [SerializeField] private float jumpCutMultiplier = 0.35f;
    [SerializeField] private float fallMultiplier    = 2.8f;
    [SerializeField] private float coyoteTime        = 0.14f;

    [Header("Death")]
    [SerializeField] private float fallDeathY = -8f;

    private Rigidbody2D rb;
    private Animator    anim;
    private int   groundContacts = 0;
    private float coyoteCounter  = 0f;
    private bool  wasGrounded    = false;

    // Squash & stretch
    private Vector3 baseScale;
    private Vector3 targetScale;

    public bool IsDead { get; private set; } = false;

    private bool IsGrounded => groundContacts > 0;

    void Awake()
    {
        rb          = GetComponent<Rigidbody2D>();
        anim        = GetComponent<Animator>();
        baseScale   = transform.localScale;
        targetScale = baseScale;

        // Load sprite at runtime so it always shows regardless of scene setup
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            var sprite = Resources.Load<Sprite>("ball_idle");
            if (sprite != null) { sr.sprite = sprite; sr.color = Color.white; }
        }
    }

    void Update()
    {
        if (IsDead) return;
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;

        // ── Fall into gap ────────────────────────────────────────────────────
        if (transform.position.y < fallDeathY)
        {
            TakeDamage();
            return;
        }

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

        // Prevent scrolling back past camera left edge
        if (Camera.main != null)
        {
            float camLeft = Camera.main.transform.position.x
                          - Camera.main.orthographicSize * Camera.main.aspect;
            float minX = camLeft + transform.lossyScale.x * 0.5f + 0.1f;
            if (transform.position.x < minX)
            {
                transform.position = new Vector3(minX, transform.position.y, 0f);
                if (rb.linearVelocity.x < 0f)
                    rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
        }

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
            targetScale   = new Vector3(baseScale.x * 0.7f, baseScale.y * 1.4f, baseScale.z);
        }

        if (jumpReleased && rb.linearVelocity.y > 0f)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);

        // ── Fall boost ───────────────────────────────────────────────────────
        if (rb.linearVelocity.y < 0f)
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1f) * Time.deltaTime;

        // ── Squash on landing ────────────────────────────────────────────────
        if (IsGrounded && !wasGrounded)
            targetScale = new Vector3(baseScale.x * 1.4f, baseScale.y * 0.6f, baseScale.z);
        wasGrounded = IsGrounded;

        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * 18f);
        if (IsGrounded && Vector3.Distance(transform.localScale, targetScale) < 0.01f)
            targetScale = baseScale;

        // ── Animator ─────────────────────────────────────────────────────────
        if (anim != null)
        {
            anim.SetFloat("Speed",      Mathf.Abs(rb.linearVelocity.x));
            anim.SetBool ("IsGrounded", IsGrounded);
            anim.SetFloat("VelocityY",  rb.linearVelocity.y);
        }

        // Flip sprite when moving left
        if (input < -0.01f)      transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else if (input > 0.01f)  transform.localScale = new Vector3( Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
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

    public void TakeDamage()
    {
        if (IsDead) return;
        IsDead = true;
        rb.linearVelocity = new Vector2(0f, 6f);  // small death hop
        rb.bodyType       = RigidbodyType2D.Kinematic;
        transform.localScale = new Vector3(baseScale.x * 1.5f, baseScale.y * 0.4f, baseScale.z);
        if (GameManager.Instance != null)
            GameManager.Instance.GameOver();
    }
}
