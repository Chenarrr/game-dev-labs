using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public static class SceneSetup
{
    [MenuItem("Lab2/Build Platformer Scene")]
    public static void BuildScene()
    {
        // ── Tags ────────────────────────────────────────────────────────────
        EnsureTag("Ground");
        EnsureTag("Obstacle");

        // ── Clear old lab2 objects ──────────────────────────────────────────
        string[] toRemove = { "Ground", "Platform1", "Platform2", "Platform3",
                               "Player", "ObstacleSpawner", "GameManager",
                               "GameOverCanvas" };
        foreach (string n in toRemove)
        {
            GameObject old = GameObject.Find(n);
            if (old != null) Object.DestroyImmediate(old);
        }

        // ── Camera ──────────────────────────────────────────────────────────
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.transform.position = new Vector3(0, -1f, -10f);
            cam.orthographicSize = 6f;
            cam.backgroundColor = new Color(0.1f, 0.1f, 0.2f);
        }

        // ── Ground ──────────────────────────────────────────────────────────
        GameObject ground = CreatePlatform("Ground", new Vector3(0, -5f, 0),
                                           new Vector3(28f, 1f, 1f), new Color(0.3f, 0.6f, 0.3f));
        ground.tag = "Ground";

        // ── Elevated Platforms ──────────────────────────────────────────────
        // With gravityScale=1.5, jumpForce=10: max jump height ~3.4 units from y=-4.1
        // Platforms need tops below y=-0.7 to be reachable. Keep them comfortable.
        CreateTaggedPlatform("Platform1", new Vector3(-4f, -2.3f, 0),
                             new Vector3(4f, 0.4f, 1f), new Color(0.4f, 0.5f, 0.7f));
        CreateTaggedPlatform("Platform2", new Vector3(2f, -1.6f, 0),
                             new Vector3(4f, 0.4f, 1f), new Color(0.4f, 0.5f, 0.7f));
        CreateTaggedPlatform("Platform3", new Vector3(6.5f, -2.3f, 0),
                             new Vector3(3f, 0.4f, 1f), new Color(0.4f, 0.5f, 0.7f));

        // ── Player ──────────────────────────────────────────────────────────
        // Spawn y: ground top = -4.5, player half-height = 0.4 → center = -4.1
        float playerY = -4.5f + 0.5f + 0.4f; // ground top + half ground height isn't right
        // Ground center = -5, scale y = 1 → top = -4.5. Player size 0.8 → half = 0.4 → center = -4.1
        playerY = -4.5f + 0.4f; // = -4.1
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = new Vector3(-6f, playerY, 0f);
        player.transform.localScale = new Vector3(0.8f, 0.8f, 1f);

        SpriteRenderer playerSR = player.AddComponent<SpriteRenderer>();
        playerSR.sprite = CreateSquareSprite();
        playerSR.color = new Color(0.2f, 0.6f, 1f);
        playerSR.sortingOrder = 1;

        BoxCollider2D playerCol = player.AddComponent<BoxCollider2D>();
        playerCol.size = Vector2.one;

        Rigidbody2D playerRb = player.AddComponent<Rigidbody2D>();
        playerRb.gravityScale = 1.5f;
        playerRb.constraints = RigidbodyConstraints2D.FreezeRotation;
        playerRb.interpolation = RigidbodyInterpolation2D.Interpolate;
        playerRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        player.AddComponent<PlayerController>();

        // ── Obstacle Spawner ────────────────────────────────────────────────
        GameObject spawnerGO = new GameObject("ObstacleSpawner");
        ObstacleSpawner spawner = spawnerGO.AddComponent<ObstacleSpawner>();
        SetPrivateField(spawner, "spawnY", playerY);

        // ── Game Manager ────────────────────────────────────────────────────
        GameObject gmGO = new GameObject("GameManager");
        gmGO.AddComponent<GameManager>();

        // ── UI Canvas ───────────────────────────────────────────────────────
        GameObject canvasGO = new GameObject("GameOverCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // GameOver Panel (dark overlay)
        GameObject panel = new GameObject("GameOverPanel");
        panel.transform.SetParent(canvasGO.transform, false);
        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0f, 0f, 0f, 0.65f);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // "GAME OVER" text
        GameObject goTextGO = new GameObject("GameOverText");
        goTextGO.transform.SetParent(panel.transform, false);
        TextMeshProUGUI goText = goTextGO.AddComponent<TextMeshProUGUI>();
        goText.text = "GAME OVER";
        goText.fontSize = 72;
        goText.alignment = TextAlignmentOptions.Center;
        goText.color = Color.white;
        goText.fontStyle = FontStyles.Bold;
        RectTransform goRect = goTextGO.GetComponent<RectTransform>();
        goRect.anchorMin = new Vector2(0.5f, 0.55f);
        goRect.anchorMax = new Vector2(0.5f, 0.55f);
        goRect.pivot = new Vector2(0.5f, 0.5f);
        goRect.anchoredPosition = Vector2.zero;
        goRect.sizeDelta = new Vector2(600f, 100f);

        // "Press R to Restart" hint
        GameObject restartTextGO = new GameObject("RestartText");
        restartTextGO.transform.SetParent(panel.transform, false);
        TextMeshProUGUI restartText = restartTextGO.AddComponent<TextMeshProUGUI>();
        restartText.text = "Press R to Restart";
        restartText.fontSize = 36;
        restartText.alignment = TextAlignmentOptions.Center;
        restartText.color = new Color(0.8f, 0.8f, 0.8f);
        RectTransform restartRect = restartTextGO.GetComponent<RectTransform>();
        restartRect.anchorMin = new Vector2(0.5f, 0.45f);
        restartRect.anchorMax = new Vector2(0.5f, 0.45f);
        restartRect.pivot = new Vector2(0.5f, 0.5f);
        restartRect.anchoredPosition = Vector2.zero;
        restartRect.sizeDelta = new Vector2(600f, 60f);

        panel.SetActive(false);

        // Wire panel into GameManager
        GameManager gm = gmGO.GetComponent<GameManager>();
        gm.SetUIReferences(panel, null, null);

        // ── Save scene ──────────────────────────────────────────────────────
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log("Lab2 Platformer Scene built! Hit Play to test. Arrow keys/WASD to move, Space/W to jump, R to restart.");
        EditorUtility.DisplayDialog("Scene Built!",
            "Platformer scene is ready.\n\n• Arrow keys / A&D — move\n• Space / W / Up — jump\n• R — restart after death\n\nHit Play!",
            "Let's go!");
    }

    // ── Basic 2D Scene ──────────────────────────────────────────────────────

    [MenuItem("Lab2/Setup Basic 2D Scene")]
    public static void SetupBasicScene()
    {
        EnsureTag("Ground");

        // Remove any existing ground object
        GameObject existing = GameObject.Find("Ground");
        if (existing != null) Object.DestroyImmediate(existing);

        // ── Camera: light blue sky background ───────────────────────────────
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.53f, 0.81f, 0.98f); // light blue
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.transform.position = new Vector3(0f, 0f, -10f);
        }

        // ── Ground: green platform at the bottom ─────────────────────────────
        // Camera size 5 → bottom edge at y = -5. Place ground so top sits at y = -3.5.
        // Ground center y = -4f, scale y = 1 → top at -3.5
        GameObject ground = new GameObject("Ground");
        ground.tag = "Ground";
        ground.transform.position = new Vector3(0f, -4f, 0f);
        ground.transform.localScale = new Vector3(20f, 1f, 1f);

        SpriteRenderer sr = ground.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite();
        sr.color = new Color(0.18f, 0.65f, 0.18f); // green

        ground.AddComponent<BoxCollider2D>();

        // ── Save ─────────────────────────────────────────────────────────────
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log("Basic 2D scene ready: green ground (tagged 'Ground'), light blue background.");
        EditorUtility.DisplayDialog("Done!", "Green ground added, background set to light blue, tag set to 'Ground'.", "OK");
    }

    // ── Add Player ──────────────────────────────────────────────────────────

    [MenuItem("Lab2/Add Player")]
    public static void AddPlayer()
    {
        EnsureTag("Player");

        // Remove existing player if present
        GameObject existing = GameObject.Find("Player");
        if (existing != null) Object.DestroyImmediate(existing);

        // Ground top = -3.5 (center -4, scaleY 1). Player scaleY 0.8, half = 0.4 → center at -3.1
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = new Vector3(0f, -3.1f, 0f);
        player.transform.localScale = new Vector3(0.8f, 0.8f, 1f);

        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite();
        sr.color = Color.red;
        sr.sortingOrder = 1;

        player.AddComponent<BoxCollider2D>();

        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 1.5f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        player.AddComponent<PlayerController>();

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log("Player added: red square, Rigidbody2D (freeze Z), BoxCollider2D, PlayerController. A/D to move, Space to jump.");
        EditorUtility.DisplayDialog("Player Added!",
            "Red square player placed on the ground.\n\n• A / D — move\n• Space — jump (grounded only)\n\nHit Play!",
            "OK");
    }

    // ── Add Obstacle Spawner ─────────────────────────────────────────────────

    [MenuItem("Lab2/Add Obstacle Spawner")]
    public static void AddObstacleSpawner()
    {
        EnsureTag("Obstacle");

        GameObject existing = GameObject.Find("ObstacleSpawner");
        if (existing != null) Object.DestroyImmediate(existing);

        GameObject spawnerGO = new GameObject("ObstacleSpawner");
        spawnerGO.AddComponent<ObstacleSpawner>();

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log("Obstacle spawner added: dark grey squares, speed 4, every 2 seconds from the right.");
        EditorUtility.DisplayDialog("Spawner Added!",
            "Obstacle spawner is in the scene.\n\nDark grey squares spawn every 2s from the right and move left at speed 4.",
            "OK");
    }

    // ── Setup Death & Score UI ───────────────────────────────────────────────

    [MenuItem("Lab2/Setup Death and Score UI")]
    public static void SetupDeathAndScoreUI()
    {
        // Ensure GameManager exists
        GameManager gm = Object.FindFirstObjectByType<GameManager>();
        if (gm == null)
        {
            GameObject gmGO = new GameObject("GameManager");
            gm = gmGO.AddComponent<GameManager>();
        }

        // Remove old canvas if present
        GameObject oldCanvas = GameObject.Find("GameUI");
        if (oldCanvas != null) Object.DestroyImmediate(oldCanvas);

        // ── Canvas ───────────────────────────────────────────────────────────
        GameObject canvasGO = new GameObject("GameUI");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // ── Score text — top left ────────────────────────────────────────────
        GameObject scoreTxtGO = new GameObject("ScoreText");
        scoreTxtGO.transform.SetParent(canvasGO.transform, false);
        TextMeshProUGUI scoreTMP = scoreTxtGO.AddComponent<TextMeshProUGUI>();
        scoreTMP.text = "Score: 0";
        scoreTMP.fontSize = 36;
        scoreTMP.color = Color.white;
        scoreTMP.fontStyle = FontStyles.Bold;
        RectTransform scoreRect = scoreTxtGO.GetComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(0f, 1f);
        scoreRect.anchorMax = new Vector2(0f, 1f);
        scoreRect.pivot = new Vector2(0f, 1f);
        scoreRect.anchoredPosition = new Vector2(20f, -20f);
        scoreRect.sizeDelta = new Vector2(300f, 60f);

        // ── Game Over panel — hidden until death ─────────────────────────────
        GameObject panel = new GameObject("GameOverPanel");
        panel.transform.SetParent(canvasGO.transform, false);
        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0f, 0f, 0f, 0.7f);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // "GAME OVER" title
        TextMeshProUGUI titleTMP = CreateCenteredText(panel, "GameOverTitle",
            "GAME OVER", 80, Color.white, FontStyles.Bold, new Vector2(0.5f, 0.6f), new Vector2(700f, 110f));

        // Final score (populated at runtime)
        TextMeshProUGUI finalTMP = CreateCenteredText(panel, "FinalScoreText",
            "Score: 0", 48, new Color(1f, 0.9f, 0.3f), FontStyles.Normal, new Vector2(0.5f, 0.47f), new Vector2(500f, 70f));

        // Restart hint
        CreateCenteredText(panel, "RestartHint",
            "Press R to Restart", 32, new Color(0.75f, 0.75f, 0.75f), FontStyles.Normal, new Vector2(0.5f, 0.37f), new Vector2(500f, 50f));

        panel.SetActive(false);

        // ── Wire into GameManager ────────────────────────────────────────────
        gm.SetUIReferences(panel, scoreTMP, finalTMP);

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log("Death & Score UI ready. Score counts up every second. Game Over on obstacle hit. R to restart.");
        EditorUtility.DisplayDialog("UI Ready!",
            "Death & Score UI added.\n\n• Score counts up every second (top left)\n• Obstacle hit → GAME OVER screen + final score\n• R — restart",
            "OK");
    }

    static TextMeshProUGUI CreateCenteredText(GameObject parent, string name, string text,
        float fontSize, Color color, FontStyles style, Vector2 anchorY, Vector2 size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.Center;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorY;
        rt.anchorMax = anchorY;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = size;
        return tmp;
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    static GameObject CreatePlatform(string name, Vector3 pos, Vector3 scale, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.position = pos;
        go.transform.localScale = scale;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite();
        sr.color = color;

        go.AddComponent<BoxCollider2D>();
        return go;
    }

    static void CreateTaggedPlatform(string name, Vector3 pos, Vector3 scale, Color color)
    {
        GameObject go = CreatePlatform(name, pos, scale, color);
        go.tag = "Ground";
    }

    static Sprite CreateSquareSprite()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    static void EnsureTag(string tag)
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        for (int i = 0; i < tagsProp.arraySize; i++)
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag) return;

        tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
        tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
    }

    static void SetPrivateField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(obj, value);
    }
}
