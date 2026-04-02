using UnityEngine;

public class Enemy : MonoBehaviour
{
    private float patrolSpeed = 1.8f;
    private float patrolRange = 2f;

    private Vector3 startPos;
    private int     dir  = -1;  // start moving left
    private bool    dead = false;

    public void Init(float speed, float range)
    {
        patrolSpeed = speed;
        patrolRange = range;
    }

    void Start() => startPos = transform.position;

    void Update()
    {
        if (dead) return;
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;

        transform.Translate(Vector2.right * dir * patrolSpeed * Time.deltaTime);

        if (Mathf.Abs(transform.position.x - startPos.x) >= patrolRange)
            dir *= -1;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (dead || !other.CompareTag("Player")) return;

        var pc = other.GetComponent<PlayerController>();
        if (pc == null || pc.IsDead) return;

        var rb = other.GetComponent<Rigidbody2D>();

        // Stomp: player is falling AND player center is above enemy center
        bool isStomp = rb != null
                    && rb.linearVelocity.y < -0.1f
                    && other.transform.position.y > transform.position.y + transform.lossyScale.y * 0.25f;

        if (isStomp)
        {
            Stomp();
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 9f);  // bounce player up
            GameManager.Instance?.AddStomp();
        }
        else
        {
            pc.TakeDamage();
        }
    }

    void Stomp()
    {
        dead = true;
        GetComponent<Collider2D>().enabled = false;
        // Flatten
        transform.localScale = new Vector3(transform.localScale.x * 1.5f,
                                           transform.localScale.y * 0.25f, 1f);
        Destroy(gameObject, 0.25f);
    }
}
