using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] private float minInterval = 2f;
    [SerializeField] private float maxInterval = 3f;
    [SerializeField] private float spawnX = 10f;
    [SerializeField] private float spawnY = -4.1f;
    [SerializeField] private float obstacleSpeed = 3f;
    [SerializeField] private float obstacleSize = 0.8f;

    private float timer;

    void Start() => ScheduleNext();

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            SpawnObstacle();
            ScheduleNext();
        }
    }

    void ScheduleNext() => timer = Random.Range(minInterval, maxInterval);

    void SpawnObstacle()
    {
        GameObject obs = new GameObject("Obstacle");
        obs.tag = "Obstacle";
        obs.transform.position = new Vector3(spawnX, spawnY, 0f);
        obs.transform.localScale = new Vector3(obstacleSize, obstacleSize, 1f);

        SpriteRenderer sr = obs.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite();
        sr.color = Color.red;
        sr.sortingOrder = 1;

        BoxCollider2D col = obs.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        Obstacle obstacleScript = obs.AddComponent<Obstacle>();
        obstacleScript.SetSpeed(obstacleSpeed);
    }

    static Sprite CreateSquareSprite()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }
}
