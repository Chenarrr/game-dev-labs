using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [SerializeField] private float speed = 3f;
    private const float DestroyX = -9f;

    public void SetSpeed(float s) => speed = s;

    void Update()
    {
        transform.Translate(Vector2.left * speed * Time.deltaTime);

        if (transform.position.x < DestroyX)
        {
            // Player successfully dodged this obstacle — award a point
            if (GameManager.Instance != null && !GameManager.Instance.isGameOver)
                GameManager.Instance.AddScore();

            Destroy(gameObject);
        }
    }
}
