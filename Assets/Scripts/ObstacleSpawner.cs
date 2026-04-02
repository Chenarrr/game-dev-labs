using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] private float spawnInterval    = 2.2f;
    [SerializeField] private float minSpawnInterval = 0.8f;
    [SerializeField] private float spawnX           = 10f;
    [SerializeField] private float groundY          = -3.1f;
    [SerializeField] private float baseSpeed        = 4f;
    [SerializeField] private float maxSpeed         = 10f;

    private float timer;
    private float elapsed;

    // Obstacle size presets: (width, height) scale
    private static readonly Vector2[] Sizes =
    {
        new Vector2(0.55f, 0.55f),   // small cube
        new Vector2(0.55f, 1.0f),    // tall thin
        new Vector2(1.1f,  0.55f),   // wide flat
        new Vector2(0.75f, 0.75f),   // medium cube
        new Vector2(0.55f, 1.3f),    // very tall
    };

    // Colours that cycle through as difficulty ramps
    private static readonly Color[] ObstacleColors =
    {
        new Color(0.20f, 0.20f, 0.20f), // dark grey  (easy)
        new Color(0.55f, 0.15f, 0.15f), // dark red   (medium)
        new Color(0.15f, 0.15f, 0.55f), // dark blue  (hard)
        new Color(0.50f, 0.15f, 0.50f), // purple     (very hard)
    };

    void Start() => timer = spawnInterval;

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;

        elapsed += Time.deltaTime;
        timer   -= Time.deltaTime;

        if (timer <= 0f)
        {
            SpawnObstacle();
            timer = CurrentInterval();
        }
    }

    float CurrentSpeed()
    {
        // Ramp from baseSpeed to maxSpeed over 120 seconds
        return Mathf.Lerp(baseSpeed, maxSpeed, Mathf.Clamp01(elapsed / 120f));
    }

    float CurrentInterval()
    {
        return Mathf.Lerp(spawnInterval, minSpawnInterval, Mathf.Clamp01(elapsed / 120f));
    }

    Color CurrentColor()
    {
        // Change colour tier every 30 seconds
        int tier = Mathf.Min((int)(elapsed / 30f), ObstacleColors.Length - 1);
        return ObstacleColors[tier];
    }

    void SpawnObstacle()
    {
        // Pick a random size
        Vector2 size = Sizes[Random.Range(0, Sizes.Length)];

        // Bottom of obstacle sits on the ground
        float spawnY = groundY + size.y * 0.5f;

        // Occasionally spawn a double obstacle (gap the player must jump through)
        bool spawnDouble = elapsed > 20f && Random.value < 0.2f;

        SpawnSingle(new Vector3(spawnX, spawnY, 0f), size, CurrentSpeed(), CurrentColor());

        if (spawnDouble)
        {
            float gapX = spawnX + Random.Range(2.5f, 4f);
            Vector2 size2 = Sizes[Random.Range(0, Sizes.Length)];
            float spawnY2 = groundY + size2.y * 0.5f;
            SpawnSingle(new Vector3(gapX, spawnY2, 0f), size2, CurrentSpeed(), CurrentColor());
        }
    }

    void SpawnSingle(Vector3 pos, Vector2 size, float speed, Color color)
    {
        GameObject obs = new GameObject("Obstacle");
        obs.tag = "Obstacle";
        obs.transform.position = pos;
        obs.transform.localScale = new Vector3(size.x, size.y, 1f);

        SpriteRenderer sr = obs.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite();
        sr.color  = color;
        sr.sortingOrder = 3;

        BoxCollider2D col = obs.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        obs.AddComponent<Obstacle>().SetSpeed(speed);
    }

    static Sprite CreateSquareSprite()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }
}
