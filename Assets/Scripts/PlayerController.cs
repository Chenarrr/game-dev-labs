using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float maxSpeed     = 7f;
    [SerializeField] private float acceleration = 45f;
    [SerializeField] private float deceleration = 35f;

    [Header("Jump")]
    [SerializeField] private float jumpForce        = 12.5f;
    [SerializeField] private float jumpCutMultiplier = 0.4f;
    [SerializeField] private float fallMultiplier    = 2.5f;
    [SerializeField] private float coyoteTime        = 0.12f;
    [SerializeField] private float jumpBufferTime    = 0.1f;

    [Header("Death")]
    [SerializeField] private float fallDeathY = -8f;

    private Rigidbody2D rb;
    private Animator    anim;
    private int   groundContacts = 0;
    private float coyoteCounter  = 0f;
    private float jumpBuffer     = 0f;
    private bool  wasGrounded    = false;

    // Squash & stretch
    private Vector3 baseScale;
    private Vector3 targetScale;
    private float   scaleSmooth = 14f;

    // Dust particles
    private ParticleSystem dustPS;

    public bool IsDead { get; private set; } = false;

    private bool IsGrounded => groundContacts > 0;

    void Awake()
    {
        rb          = GetComponent<Rigidbody2D>();
        anim        = GetComponent<Animator>();
        baseScale   = transform.localScale;
        targetScale = baseScale;

        // Load sprite at runtime as fallback
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.white;
            sr.sortingOrder = 10;  // Always on top

            var tex = Resources.Load<Texture2D>("ball_idle");
            if (tex != null)
            {
                sr.sprite = Sprite.Create(tex,
                    new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f),
                    tex.width);
            }
        }

        // Create dust particle system
        CreateDustParticles();
    }

    void Update()
    {
        if (IsDead) return;
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;

        if (transform.position.y < fallDeathY)
        {
            TakeDamage();
            return;
        }

        var kb = Keyboard.current;
        if (kb == null) return;

        // ── Horizontal ───────────────────────────────────────────────────
        float input = 0f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  input = -1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) input =  1f;

        float targetVX = input * maxSpeed;
        float rate     = Mathf.Abs(input) > 0.01f ? acceleration : deceleration;
        float newVX    = Mathf.MoveTowards(rb.linearVelocity.x, targetVX, rate * Time.deltaTime);
        rb.linearVelocity = new Vector2(newVX, rb.linearVelocity.y);

        // ── Coyote time ──────────────────────────────────────────────────
        if (IsGrounded) coyoteCounter = coyoteTime;
        else            coyoteCounter -= Time.deltaTime;

        // ── Jump buffer ──────────────────────────────────────────────────
        bool jumpPressed  = kb.spaceKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame;
        bool jumpReleased = kb.spaceKey.wasReleasedThisFrame || kb.upArrowKey.wasReleasedThisFrame;

        if (jumpPressed) jumpBuffer = jumpBufferTime;
        else             jumpBuffer -= Time.deltaTime;

        if (jumpBuffer > 0f && coyoteCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            coyoteCounter = 0f;
            jumpBuffer    = 0f;
            targetScale   = new Vector3(baseScale.x * 0.75f, baseScale.y * 1.3f, baseScale.z);
        }

        if (jumpReleased && rb.linearVelocity.y > 0f)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);

        // ── Fall boost ───────────────────────────────────────────────────
        if (rb.linearVelocity.y < 0f)
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1f) * Time.deltaTime;

        // ── Squash on landing ────────────────────────────────────────────
        if (IsGrounded && !wasGrounded)
        {
            targetScale = new Vector3(baseScale.x * 1.3f, baseScale.y * 0.7f, baseScale.z);
            EmitDust(8);
        }

        // Running dust
        if (IsGrounded && Mathf.Abs(rb.linearVelocity.x) > 2f && dustPS != null)
        {
            if (!dustPS.isPlaying) dustPS.Play();
        }
        else if (dustPS != null && dustPS.isPlaying)
        {
            dustPS.Stop();
        }

        wasGrounded = IsGrounded;

        // Smooth scale recovery
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSmooth);
        if (IsGrounded && Vector3.Distance(transform.localScale, targetScale) < 0.02f)
            targetScale = baseScale;

        // ── Animator ─────────────────────────────────────────────────────
        if (anim != null)
        {
            anim.SetFloat("Speed",      Mathf.Abs(rb.linearVelocity.x));
            anim.SetBool ("IsGrounded", IsGrounded);
            anim.SetFloat("VelocityY",  rb.linearVelocity.y);
        }

        // Flip sprite
        if (input < -0.01f)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else if (input > 0.01f)
            transform.localScale = new Vector3( Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
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
        rb.linearVelocity = new Vector2(0f, 6f);
        rb.bodyType       = RigidbodyType2D.Kinematic;
        transform.localScale = new Vector3(baseScale.x * 1.5f, baseScale.y * 0.4f, baseScale.z);
        if (GameManager.Instance != null)
            GameManager.Instance.GameOver();
    }

    void CreateDustParticles()
    {
        var go = new GameObject("DustPS");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0f, -0.3f, 0f);

        dustPS = go.AddComponent<ParticleSystem>();
        dustPS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = dustPS.main;
        main.startLifetime  = 0.4f;
        main.startSpeed     = 0.8f;
        main.startSize      = 0.15f;
        main.startColor     = new Color(0.6f, 0.5f, 0.35f, 0.5f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles   = 30;
        main.gravityModifier = 0.3f;

        var emission = dustPS.emission;
        emission.rateOverTime = 12f;

        var shape = dustPS.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius    = 0.15f;

        var col = dustPS.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(new Color(0.6f, 0.5f, 0.35f), 0f), new GradientColorKey(new Color(0.6f, 0.5f, 0.35f), 1f) },
            new[] { new GradientAlphaKey(0.5f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        col.color = grad;

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.sortingOrder = 5;
    }

    void EmitDust(int count)
    {
        if (dustPS != null) dustPS.Emit(count);
    }
}
