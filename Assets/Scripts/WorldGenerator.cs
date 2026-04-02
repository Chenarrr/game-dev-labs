using System.Collections.Generic;
using UnityEngine;

/// Procedurally generates Mario-style ground tiles, platforms and enemies
/// as the player runs to the right.
public class WorldGenerator : MonoBehaviour
{
    // How far ahead of the camera right-edge to generate
    private const float GenerateAhead = 20f;
    // Destroy tiles this far behind the camera left-edge
    private const float CleanupBehind = 15f;

    private const float GroundY    = -4f;
    private const float GroundH    =  1f;

    private float generatedUpTo;
    private int   chunkIndex = 0;

    private readonly List<GameObject> tiles = new List<GameObject>();

    // Difficulty ramp based on distance
    private float Difficulty => Mathf.Clamp01(generatedUpTo / 300f);

    void Start()
    {
        // Existing static ground is now 8 units wide centred at 0 → right edge = +4
        generatedUpTo = 4f;
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;

        float camRight = Camera.main.transform.position.x
                       + Camera.main.orthographicSize * Camera.main.aspect;

        while (generatedUpTo < camRight + GenerateAhead)
            GenerateChunk();

        CleanupOld();
    }

    void GenerateChunk()
    {
        chunkIndex++;
        bool isFirst = chunkIndex <= 1;  // only very first chunk: no gap, no enemies

        // ── Ground segment ───────────────────────────────────────────────────
        float minW = Mathf.Max(2.5f, 5f - Difficulty * 2.5f);
        float maxW = Mathf.Max(minW + 1f, 8f - Difficulty * 3f);
        float gW   = Random.Range(minW, maxW);

        SpawnTile(generatedUpTo, gW, GroundY, GroundH, BrownColor(), "Ground", 2);

        // ── Platform above the segment ───────────────────────────────────────
        if (!isFirst && Random.value < 0.55f)
        {
            float pW = Random.Range(1.8f, Mathf.Min(gW * 0.7f, 4f));
            float pX = generatedUpTo + Random.Range(0.3f, gW - pW - 0.3f);
            float pY = Random.Range(-2.2f, -1.2f);  // height above ground
            SpawnTile(pX, pW, pY, 0.5f, BrownColor(), "Ground", 2);

            // Enemy on platform
            if (Random.value < 0.5f + Difficulty * 0.3f)
                SpawnEnemy(pX + pW * 0.5f, pY + 0.5f * 0.5f + 0.35f);
        }

        // ── Enemy on ground ──────────────────────────────────────────────────
        if (!isFirst && Random.value < 0.35f + Difficulty * 0.3f)
            SpawnEnemy(generatedUpTo + gW * 0.5f, GroundY + GroundH * 0.5f + 0.35f);

        // ── Gap ──────────────────────────────────────────────────────────────
        float minGap = isFirst ? 0f : Mathf.Lerp(1.5f, 3.5f, Difficulty);
        float gap    = isFirst ? 0.5f : Random.Range(minGap, minGap + 1.2f);

        generatedUpTo += gW + gap;
    }

    void SpawnTile(float leftX, float width, float centerY,
                   float height, Color color, string tag, int sortOrder)
    {
        GameObject g = new GameObject("Tile");
        g.tag = tag;
        g.transform.position   = new Vector3(leftX + width * 0.5f, centerY, 0f);
        g.transform.localScale = new Vector3(width, height, 1f);

        var sr = g.AddComponent<SpriteRenderer>();
        sr.sprite       = WhiteSquare();
        sr.color        = color;
        sr.sortingOrder = sortOrder;

        g.AddComponent<BoxCollider2D>();

        tiles.Add(g);
    }

    void SpawnEnemy(float x, float y)
    {
        GameObject e = new GameObject("Enemy");
        e.tag = "Enemy";
        e.transform.position   = new Vector3(x, y, 0f);
        e.transform.localScale = new Vector3(0.65f, 0.65f, 1f);

        var sr = e.AddComponent<SpriteRenderer>();
        sr.sprite       = WhiteSquare();
        sr.color        = EnemyColor();
        sr.sortingOrder = 3;

        var col = e.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        float speed = Random.Range(1.2f + Difficulty, 2.5f + Difficulty * 2f);
        float range = Random.Range(1.5f, 3f);
        e.AddComponent<Enemy>().Init(speed, range);

        tiles.Add(e);
    }

    void CleanupOld()
    {
        if (Camera.main == null) return;
        float camLeft = Camera.main.transform.position.x
                      - Camera.main.orthographicSize * Camera.main.aspect;
        float removeX = camLeft - CleanupBehind;

        tiles.RemoveAll(t =>
        {
            if (t == null) return true;
            if (t.transform.position.x < removeX)
            {
                Destroy(t);
                return true;
            }
            return false;
        });
    }

    // ── Helpers ──────────────────────────────────────────────────────────────
    static Color BrownColor()
    {
        // Slight random variation to make ground look lively
        float r = Random.Range(0.40f, 0.52f);
        float g = Random.Range(0.23f, 0.32f);
        float b = Random.Range(0.08f, 0.16f);
        return new Color(r, g, b);
    }

    static Color EnemyColor()
    {
        // Red/orange enemies that stand out from brown ground
        Color[] colors = {
            new Color(0.85f, 0.2f,  0.2f),
            new Color(0.9f,  0.5f,  0.1f),
            new Color(0.7f,  0.1f,  0.5f),
        };
        return colors[Random.Range(0, colors.Length)];
    }

    static Sprite WhiteSquare()
    {
        Texture2D t = new Texture2D(1, 1);
        t.SetPixel(0, 0, Color.white);
        t.Apply();
        return Sprite.Create(t, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }
}
