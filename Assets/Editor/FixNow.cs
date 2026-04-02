#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class FixNow
{
    public static void Execute()
    {
        // ── 1. Force import ball_idle as Sprite ──────────────────────────
        string path = "Assets/Sprites/ball_idle.png";
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp != null)
        {
            imp.textureType         = TextureImporterType.Sprite;
            imp.spritePixelsPerUnit = 100;
            imp.mipmapEnabled       = false;
            imp.SaveAndReimport();
        }

        // ── 2. Assign sprite + white color to Player ─────────────────────
        var player = GameObject.Find("Player");
        var sr     = player?.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            sr.sprite = sprite;
            sr.color  = Color.white;
            Debug.Log(sprite != null ? "Sprite OK: " + sprite.name : "SPRITE STILL NULL");
        }

        // ── 3. Add CameraFollow to Main Camera directly ──────────────────
        var cam = GameObject.Find("Main Camera");
        if (cam != null)
        {
            var cf = cam.GetComponent<CameraFollow>();
            if (cf == null) cf = cam.AddComponent<CameraFollow>();
            // Set target via reflection since field is private
            var targetField = typeof(CameraFollow).GetField("target",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (targetField != null && player != null)
                targetField.SetValue(cf, player.transform);
            Debug.Log("CameraFollow added and target set to Player");
        }

        EditorUtility.SetDirty(player);
        EditorUtility.SetDirty(cam);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Debug.Log("All fixed!");
    }
}
#endif
