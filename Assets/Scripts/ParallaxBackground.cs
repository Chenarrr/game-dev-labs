using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    private Camera cam;
    private float  lastCamX;

    private class Layer
    {
        public GameObject[] tiles;
        public float speed, tileW, y, height;
        public Color color;
    }

    private Layer[] layers;

    void Start()
    {
        cam      = Camera.main;
        lastCamX = cam.transform.position.x;

        // Sky blue background on camera
        cam.backgroundColor = new Color(0.53f, 0.81f, 0.98f);
        cam.clearFlags      = CameraClearFlags.SolidColor;

        float halfW = cam.orthographicSize * cam.aspect;
        float tileW = halfW * 2f + 4f;

        layers = new Layer[]
        {
            // Far mountains — soft blue/purple
            new Layer { speed=0.05f, tileW=tileW*2f,   y=-1.8f, height=3.5f,
                        color=new Color(0.62f, 0.72f, 0.88f) },
            // Mid hills — muted green
            new Layer { speed=0.18f, tileW=tileW*1.2f, y=-2.8f, height=2.2f,
                        color=new Color(0.42f, 0.68f, 0.38f) },
        };

        foreach (var l in layers) BuildLayer(l);
    }

    void BuildLayer(Layer layer)
    {
        layer.tiles = new GameObject[4];
        float camX  = cam.transform.position.x;
        for (int i = 0; i < 4; i++)
        {
            var go = new GameObject("BgTile");
            go.transform.SetParent(transform);
            go.transform.position   = new Vector3(camX + (i - 1) * layer.tileW, layer.y, 1f);
            go.transform.localScale = new Vector3(layer.tileW, layer.height, 1f);
            var sr         = go.AddComponent<SpriteRenderer>();
            sr.sprite       = Square();
            sr.color        = layer.color;
            sr.sortingOrder = -3;
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
            foreach (var tile in layer.tiles)
                tile.transform.position += new Vector3(delta * layer.speed, 0f, 0f);

            float halfW    = cam.orthographicSize * cam.aspect;
            float camLeft  = camX - halfW;
            float camRight = camX + halfW;
            foreach (var tile in layer.tiles)
            {
                if (tile.transform.position.x + layer.tileW * 0.5f < camLeft - 1f)
                    tile.transform.position += new Vector3(layer.tileW * 4f, 0f, 0f);
                if (tile.transform.position.x - layer.tileW * 0.5f > camRight + layer.tileW)
                    tile.transform.position -= new Vector3(layer.tileW * 4f, 0f, 0f);
            }
        }
    }

    static Sprite Square()
    {
        var t = new Texture2D(1, 1);
        t.SetPixel(0, 0, Color.white); t.Apply();
        return Sprite.Create(t, new Rect(0,0,1,1), new Vector2(0.5f,0.5f), 1f);
    }
}
