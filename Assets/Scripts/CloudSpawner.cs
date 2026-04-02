using UnityEngine;

public class CloudSpawner : MonoBehaviour
{
    private Camera cam;
    private float  spawnTimer;
    private const float Interval = 6f;

    void Start()
    {
        cam = Camera.main;
        for (int i = 0; i < 2; i++)
            SpawnCloud(Random.Range(-8f, 8f));
        spawnTimer = Interval;
    }

    void Update()
    {
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            float spawnX = cam != null ? cam.transform.position.x + 14f : 14f;
            SpawnCloud(spawnX);
            spawnTimer = Random.Range(Interval * 0.8f, Interval * 1.4f);
        }
    }

    void SpawnCloud(float startX)
    {
        float y     = Random.Range(1.5f, 3.8f);
        float scale = Random.Range(1.0f, 1.8f);
        float speed = Random.Range(0.3f, 0.7f);
        float alpha = Random.Range(0.4f, 0.65f);

        GameObject cloud = new GameObject("Cloud");
        cloud.transform.position = new Vector3(startX, y, 0.5f);

        float[] xs = { 0f, -0.45f,  0.45f };
        float[] ys = { 0f, -0.18f, -0.18f };
        float[] ss = { 1f,  0.70f,  0.70f };

        for (int i = 0; i < 3; i++)
        {
            var puff = new GameObject("Puff");
            puff.transform.SetParent(cloud.transform, false);
            puff.transform.localPosition = new Vector3(xs[i] * scale, ys[i] * scale, 0f);
            puff.transform.localScale    = new Vector3(scale * ss[i], scale * 0.6f * ss[i], 1f);
            var sr = puff.AddComponent<SpriteRenderer>();
            sr.sprite       = CircleSprite();
            sr.color        = new Color(1f, 1f, 1f, alpha);
            sr.sortingOrder = -1;
        }

        cloud.AddComponent<CloudMover>().Init(speed, startX - 50f);
    }

    static Sprite CircleSprite()
    {
        int sz = 64; float r = sz / 2f;
        var tex = new Texture2D(sz, sz);
        for (int x = 0; x < sz; x++)
        for (int y = 0; y < sz; y++)
        {
            float d = Vector2.Distance(new Vector2(x, y), new Vector2(r, r));
            float a = Mathf.Clamp01(1f - (d - (r - 2f)) / 2f);
            tex.SetPixel(x, y, new Color(1, 1, 1, a));
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, sz, sz), new Vector2(0.5f, 0.5f), sz * 0.5f);
    }
}

public class CloudMover : MonoBehaviour
{
    float speed, destroyX;
    public void Init(float s, float dx) { speed = s; destroyX = dx; }
    void Update()
    {
        transform.Translate(Vector2.left * speed * Time.deltaTime);
        if (transform.position.x < destroyX) Destroy(gameObject);
    }
}
