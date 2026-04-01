using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public static class SceneSetup
{
    [MenuItem("Lab2/Fix Scene")]
    public static void FixScene()
    {
        // ── Tags ─────────────────────────────────────────────────────────────
        EnsureTag("Ground");
        EnsureTag("Obstacle");
        EnsureTag("Player");

        // ── Wipe old objects so we start clean ───────────────────────────────
        foreach (string n in new[] { "Ground", "Player", "ObstacleSpawner",
                                     "GameManager", "GameUI" })
        {
            GameObject o = GameObject.Find(n);
            if (o != null) Object.DestroyImmediate(o);
        }

        // ── Camera ────────────────────────────────────────────────────────────
        // Ortho size 5 → visible y: -5 to +5
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags       = CameraClearFlags.SolidColor;
            cam.backgroundColor  = new Color(0.53f, 0.81f, 0.98f); // sky blue
            cam.orthographic     = true;
            cam.orthographicSize = 5f;
            cam.transform.position = new Vector3(0f, 0f, -10f);
        }

        // ── Ground ────────────────────────────────────────────────────────────
        // Centre y=-4, scaleY=1  →  top edge = -3.5
        GameObject ground = new GameObject("Ground");
        ground.tag = "Ground";
        ground.transform.position   = new Vector3(0f, -4f, 0f);
        ground.transform.localScale = new Vector3(22f, 1f, 1f);
        AddSR(ground, new Color(0.18f, 0.65f, 0.18f), 0); // green
        ground.AddComponent<BoxCollider2D>();

        // ── Player ────────────────────────────────────────────────────────────
        // Ground top = -3.5.  Player scaleY=0.8, half=0.4  →  centre = -3.1
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position   = new Vector3(-4f, -3.1f, 0f);
        player.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
        AddSR(player, Color.red, 1);

        BoxCollider2D pc = player.AddComponent<BoxCollider2D>();
        pc.size = Vector2.one;

        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale           = 3f;  // base gravity; script adds extra on fall
        rb.constraints            = RigidbodyConstraints2D.FreezeRotation;
        rb.interpolation          = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        player.AddComponent<PlayerController>();

        // ── Obstacle Spawner ──────────────────────────────────────────────────
        // Obstacles sit on the ground: centre y = groundTop + obstacleHalf = -3.5 + 0.3 = -3.2
        GameObject spawnerGO = new GameObject("ObstacleSpawner");
        ObstacleSpawner spawner = spawnerGO.AddComponent<ObstacleSpawner>();
        SetField(spawner, "spawnY", -3.2f);

        // ── GameManager ───────────────────────────────────────────────────────
        GameObject gmGO = new GameObject("GameManager");
        GameManager gm  = gmGO.AddComponent<GameManager>();

        // ── UI Canvas ─────────────────────────────────────────────────────────
        GameObject canvasGO = new GameObject("GameUI");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        CanvasScaler cs = canvasGO.AddComponent<CanvasScaler>();
        cs.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // Score — top left
        TextMeshProUGUI scoreTMP = MakeText(canvasGO,  "ScoreText", "Score: 0", 36,
            Color.white, FontStyles.Bold,
            new Vector2(0f, 1f), new Vector2(0f, 1f),   // anchor top-left
            new Vector2(0f, 1f),                          // pivot
            new Vector2(20f, -20f), new Vector2(280f, 55f));

        // Game Over panel — fullscreen overlay, hidden until death
        GameObject panel = new GameObject("GameOverPanel");
        panel.transform.SetParent(canvasGO.transform, false);
        Image img = panel.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.72f);
        StretchFull(panel.GetComponent<RectTransform>());

        MakeText(panel, "TitleText",   "GAME OVER",          80, Color.white,
            FontStyles.Bold, Vector2.one * 0.5f, Vector2.one * 0.5f, Vector2.one * 0.5f,
            new Vector2(0f, 60f),  new Vector2(700f, 110f));

        TextMeshProUGUI finalTMP =
        MakeText(panel, "FinalScore",  "Score: 0",            48, new Color(1f, 0.9f, 0.3f),
            FontStyles.Normal, Vector2.one * 0.5f, Vector2.one * 0.5f, Vector2.one * 0.5f,
            new Vector2(0f, -20f), new Vector2(500f, 70f));

        MakeText(panel, "RestartHint", "Press R to Restart",  30, new Color(0.75f, 0.75f, 0.75f),
            FontStyles.Normal, Vector2.one * 0.5f, Vector2.one * 0.5f, Vector2.one * 0.5f,
            new Vector2(0f, -90f), new Vector2(500f, 50f));

        panel.SetActive(false);

        // Wire UI into GameManager
        gm.SetUIReferences(panel, scoreTMP, finalTMP);

        // ── Save ──────────────────────────────────────────────────────────────
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        EditorUtility.DisplayDialog("Done — Hit Play!",
            "Everything is set up:\n\n" +
            "• Green ground with BoxCollider2D (tagged 'Ground')\n" +
            "• Red player with Rigidbody2D + BoxCollider2D\n" +
            "• Dark grey obstacles every 2s from the right\n" +
            "• Score counter (top left)\n" +
            "• Game Over screen + R to restart\n\n" +
            "Controls:  A/D / ←→ move    Space/W/↑ jump    R/Enter restart",
            "Let's go!");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static void AddSR(GameObject go, Color color, int order)
    {
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = SquareSprite();
        sr.color        = color;
        sr.sortingOrder = order;
    }

    static Sprite SquareSprite()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    static TextMeshProUGUI MakeText(GameObject parent, string name, string text,
        float fontSize, Color color, FontStyles style,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 anchoredPos, Vector2 sizeDelta)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = fontSize;
        tmp.color     = color;
        tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.Center;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin      = anchorMin;
        rt.anchorMax      = anchorMax;
        rt.pivot          = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta      = sizeDelta;
        return tmp;
    }

    static void StretchFull(RectTransform rt)
    {
        rt.anchorMin  = Vector2.zero;
        rt.anchorMax  = Vector2.one;
        rt.offsetMin  = Vector2.zero;
        rt.offsetMax  = Vector2.zero;
    }

    static void EnsureTag(string tag)
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tags = tagManager.FindProperty("tags");
        for (int i = 0; i < tags.arraySize; i++)
            if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;
        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
    }

    static void SetField(object obj, string field, object value)
    {
        obj.GetType()
           .GetField(field, System.Reflection.BindingFlags.NonPublic |
                            System.Reflection.BindingFlags.Instance)
           ?.SetValue(obj, value);
    }
}
