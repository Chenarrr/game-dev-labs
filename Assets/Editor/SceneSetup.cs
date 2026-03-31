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
        gm.SetGameOverPanel(panel);

        // ── Save scene ──────────────────────────────────────────────────────
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log("Lab2 Platformer Scene built! Hit Play to test. Arrow keys/WASD to move, Space/W to jump, R to restart.");
        EditorUtility.DisplayDialog("Scene Built!",
            "Platformer scene is ready.\n\n• Arrow keys / A&D — move\n• Space / W / Up — jump\n• R — restart after death\n\nHit Play!",
            "Let's go!");
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
