using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [SerializeField] private float speed = 3f;
    private const float DestroyX = -10f;

    private Transform player;
    private bool      scored = false;

    public void SetSpeed(float s) => speed = s;

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        transform.Translate(Vector2.left * speed * Time.deltaTime);

        // Score as soon as the obstacle's right edge clears the player —
        // regardless of how far away the player is horizontally.
        if (!scored && player != null)
        {
            float obstacleRight = transform.position.x + transform.lossyScale.x * 0.5f;
            float playerLeft    = player.position.x    - player.lossyScale.x    * 0.5f;

            if (obstacleRight < playerLeft)
            {
                scored = true;
                if (GameManager.Instance != null && !GameManager.Instance.isGameOver)
                    GameManager.Instance.AddScore();
            }
        }

        if (transform.position.x < DestroyX)
            Destroy(gameObject);
    }
}
