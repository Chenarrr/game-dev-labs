using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private float spawnX = 10f;
    [SerializeField] private float spawnY = -3.1f;
    [SerializeField] private float obstacleSpeed = 4f;
    [SerializeField] private float obstacleSize = 0.6f;

    private float timer;

    void Start() => timer = spawnInterval;

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            SpawnObstacle();
            timer = spawnInterval;
        }
    }

    void SpawnObstacle()
    {
        GameObject obs = new GameObject("Obstacle");
        obs.tag = "Obstacle";
        obs.transform.position = new Vector3(spawnX, spawnY, 0f);
        obs.transform.localScale = new Vector3(obstacleSize, obstacleSize, 1f);

        SpriteRenderer sr = obs.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite();
        sr.color = new Color(0.25f, 0.25f, 0.25f); // dark grey
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
