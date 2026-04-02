using UnityEngine;

/// Creates infinite repeating background layers that scroll slower than the camera.
public class ParallaxBackground : MonoBehaviour
{
    private Camera  cam;
    private float   lastCamX;

    // Each layer: a row of wide quads that wrap around
    private Layer[] layers;

    private class Layer
    {
        public GameObject[] tiles;
        public float        speed;   // fraction of camera speed (0=fixed, 1=same as cam)
        public float        tileW;
        public float        y;
        public float        height;
        public Color        color;
    }

    void Start()
    {
        cam      = Camera.main;
        lastCamX = cam.transform.position.x;

        // How wide the camera view is
        float halfW = cam.orthographicSize * cam.aspect;
        float tileW = halfW * 2f + 2f;   // one tile = full screen width + a bit

        layers = new Layer[]
        {
            // Far hills — dark blueish, barely move
            new Layer { speed = 0.05f, tileW = tileW * 1.5f, y = -1.5f, height = 3f,
                        color = new Color(0.45f, 0.60f, 0.75f) },
            // Mid hills — muted green, move a little
            new Layer { speed = 0.2f,  tileW = tileW,        y = -2.5f, height = 2f,
                        color = new Color(0.40f, 0.62f, 0.35f) },
        };

        foreach (var layer in layers)
            BuildLayer(layer);
    }

    void BuildLayer(Layer layer)
    {
        // 3 tiles per layer so one is always off-screen on each side
        layer.tiles = new GameObject[3];
        float camX  = cam.transform.position.x;

        for (int i = 0; i < 3; i++)
        {
            GameObject go = new GameObject("BgTile");
            go.transform.SetParent(transform);

            float x = camX + (i - 1) * layer.tileW;
            go.transform.position   = new Vector3(x, layer.y, 1f);
            go.transform.localScale = new Vector3(layer.tileW, layer.height, 1f);

            var sr         = go.AddComponent<SpriteRenderer>();
            sr.sprite       = WhiteSquare();
            sr.color        = layer.color;
            sr.sortingOrder = -2;

            layer.tiles[i] = go;
        }
    }

    void LateUpdate()
    {
        if (cam == null) return;

        float camX  = cam.transform.position.x;
        float delta = camX - lastCamX;
        lastCamX    = camX;

        foreach (var layer in layers)
        {
            // Move each tile by a fraction of camera movement (parallax)
            foreach (var tile in layer.tiles)
                tile.transform.position += new Vector3(delta * layer.speed, 0f, 0f);

            // Wrap tiles: if a tile falls too far behind, move it ahead
            float halfW    = cam.orthographicSize * cam.aspect;
            float camLeft  = camX - halfW;
            float camRight = camX + halfW;

            foreach (var tile in layer.tiles)
            {
                if (tile.transform.position.x + layer.tileW * 0.5f < camLeft)
                    tile.transform.position += new Vector3(layer.tileW * 3f, 0f, 0f);
                if (tile.transform.position.x - layer.tileW * 0.5f > camRight + layer.tileW)
                    tile.transform.position -= new Vector3(layer.tileW * 3f, 0f, 0f);
            }
        }
    }

    static Sprite WhiteSquare()
    {
        Texture2D t = new Texture2D(1, 1);
        t.SetPixel(0, 0, Color.white);
        t.Apply();
        return Sprite.Create(t, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }
}
