using UnityEngine;

/// Spawns simple white cloud shapes that scroll across the sky.
public class CloudSpawner : MonoBehaviour
{
    private const float SpawnX    =  12f;
    private const float DestroyX  = -12f;
    private const float MinY      =  0.5f;
    private const float MaxY      =  4.0f;
    private const float MinSpeed  =  0.5f;
    private const float MaxSpeed  =  1.4f;
    private const float MinScale  =  0.8f;
    private const float MaxScale  =  2.2f;

    private float spawnTimer;
    private float spawnInterval = 2.5f;

    void Start()
    {
        // Seed a few clouds at game start so sky isn't empty
        for (int i = 0; i < 5; i++)
            SpawnCloud(Random.Range(-10f, SpawnX));
        spawnTimer = spawnInterval;
    }

    void Update()
    {
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnCloud(SpawnX);
            spawnTimer = Random.Range(spawnInterval * 0.7f, spawnInterval * 1.3f);
        }
    }

    void SpawnCloud(float startX)
    {
        float y     = Random.Range(MinY, MaxY);
        float scale = Random.Range(MinScale, MaxScale);
        float speed = Random.Range(MinSpeed, MaxSpeed);
        float alpha = Random.Range(0.55f, 0.85f);

        // A cloud = 3 overlapping ovals (just scaled squares with rounded look)
        GameObject cloud = new GameObject("Cloud");
        cloud.transform.position = new Vector3(startX, y, 0f);

        int[] xOffsets = { 0, -1, 1 };
        float[] yOffsets = { 0f, -0.2f, -0.2f };
        float[] scales   = { 1f,  0.75f, 0.75f };

        foreach (int i in new[] { 0, 1, 2 })
        {
            GameObject puff = new GameObject("Puff");
            puff.transform.SetParent(cloud.transform, false);
            puff.transform.localPosition = new Vector3(xOffsets[i] * scale * 0.5f, yOffsets[i] * scale, 0f);
            puff.transform.localScale    = new Vector3(scale * scales[i], scale * 0.65f * scales[i], 1f);

            SpriteRenderer sr = puff.AddComponent<SpriteRenderer>();
            sr.sprite       = CreateCircleSprite();
            sr.color        = new Color(1f, 1f, 1f, alpha);
            sr.sortingOrder = -1; // behind ground and player
        }

        cloud.AddComponent<CloudMover>().Init(speed, DestroyX);
    }

    static Sprite CreateCircleSprite()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        float r = size / 2f;
        for (int x = 0; x < size; x++)
        for (int y = 0; y < size; y++)
        {
            float dist = Vector2.Distance(new Vector2(x, y), new Vector2(r, r));
            float a    = Mathf.Clamp01(1f - (dist - (r - 2f)) / 2f);
            tex.SetPixel(x, y, new Color(1, 1, 1, a));
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size * 0.5f);
    }
}

public class CloudMover : MonoBehaviour
{
    private float speed;
    private float destroyX;

    public void Init(float s, float dx) { speed = s; destroyX = dx; }

    void Update()
    {
        transform.Translate(Vector2.left * speed * Time.deltaTime);
        if (transform.position.x < destroyX)
            Destroy(gameObject);
    }
}
